using UnityEngine;
using UnityEngine.SceneManagement;
using static SimulationConstants;

public class PlayerMovement : MonoBehaviour
{
    private Vector3 lastPos;
    private Quaternion lastRot;
    private float lastSend = 0;
    NetworkController network;
    public bool useLocalRotation = false;
    string sceneName = "";
    private Transform cameraTransform;
    [SerializeField]
    float threshold = 5f;

    private SimulationManager manager
    {
        get { return SimulationManager.Instance; }
    }
    private void Awake()
    {
        lastPos = transform.position;
        lastRot = transform.rotation;
        sceneName = SceneManager.GetActiveScene().name;
    }

    void FixedUpdate()
    {
        if (sceneName != SCENE_LOAD) {
            if (shouldSendPositionUpdate())
            {
                // Broadcast movement to network:
                if (!network) network = FindObjectOfType<NetworkController>();
                Quaternion rot = transform.rotation;
                if (useLocalRotation)
                {
                    rot = transform.localRotation;
                }
                    
                network.BroadcastPlayerMovement(transform.position, rot);
                getCameraRotationAndUpdatePin();
                    
                // Log movement:
                string movementInfo = "local player moved to P:" +
                    transform.position.ToString() + " R:" + rot.ToString();
                CCLogger.Log(LOG_EVENT_PLAYER_MOVE, movementInfo);

                // update local comparators
                lastPos = transform.position;
                lastRot = rot;
                lastSend = Time.time;
            }
        }
    }
    bool shouldSendPositionUpdate()
    {
        float timeDeltaSinceLastSend = Time.time - manager.MovementSendInterval;
        float maxTimeDelta = Time.time - (manager.MovementSendInterval * 10);
        
        bool shouldSendUpdate = false;
        // send update - no more frequently than once per second
        if (timeDeltaSinceLastSend > lastSend)
        {
            bool hasMoved = (lastPos != transform.position || lastRot != transform.rotation);
            // we definitely want to send once every 10s if there has been any movement, even small
            if (hasMoved && (maxTimeDelta > lastSend))
            {
                shouldSendUpdate = true;
            }
            else if (hasMoved)
            {
                float delta = Mathf.Abs(Vector3.Magnitude(lastRot.eulerAngles - transform.rotation.eulerAngles));
                shouldSendUpdate = delta > threshold;
            }
        }
        return shouldSendUpdate;
        
    }
    private void getCameraRotationAndUpdatePin()
    {
        if (sceneName == SCENE_HORIZON)
        {

            if (cameraTransform == null)
            {
                cameraTransform = Camera.main.transform;
            }

            Vector3 cameraRotation = new Vector3(cameraTransform.rotation.eulerAngles.x, cameraTransform.rotation.eulerAngles.y, 0);

            if (manager.LocalPlayerLookDirection != cameraRotation)
            {
                manager.LocalPlayerLookDirection = cameraRotation;

                CCDebug.Log("Sending updated pin", LogLevel.Verbose, LogMessageCategory.Networking);
                SimulationEvents.Instance.PushPinUpdated.Invoke(manager.LocalPlayerPin, manager.LocalPlayerLookDirection);
            }
        }
    }

    public void SetLookDirection(Vector3 dir)
    {
        // adjust camera direction to match remote player
        cameraTransform.rotation = Quaternion.Euler(dir.x, dir.y, 0);
    }
}
