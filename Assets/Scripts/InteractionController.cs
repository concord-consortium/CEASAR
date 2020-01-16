using UnityEngine;
using System.Collections;


public class InteractionController : MonoBehaviour
{
    public GameObject interactionIndicator;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
  
    public void ShowInteraction(Vector3 pos, Quaternion rot, Color playerColor, bool isLocal)
    {
        if (interactionIndicator)
        {
            GameObject indicatorObj = Instantiate(interactionIndicator);
            GameObject earth = GameObject.Find("Earth");
            indicatorObj.transform.localRotation = rot;
            indicatorObj.transform.position = pos;
            Utils.SetObjectColor(indicatorObj, playerColor);
            if (earth)
            {

                Vector3 earthPos = pos - earth.transform.position;
                Vector3 size = earth.GetComponent<Renderer>().bounds.size;
                float radius = size.x / 2;

                Debug.Log("lat lng " + Utils.LatLngFromPosition(earthPos, radius));
            }
            else
            {
                Debug.Log("No Earth found in interacion");
            }
            StartCoroutine(selfDestruct(indicatorObj));
        }
        if (isLocal)
        {
            // now need to broadcast to remotes
            //colyseusClient.SendNetworkTransformUpdate(pos, rot, "interaction");
            string interactionInfo = "local interaction P:" +
                    pos.ToString() + " R:" + rot.ToString();
            CCLogger.Log(CCLogger.EVENT_ADD_INTERACTION, interactionInfo);
        }
    }

    IEnumerator selfDestruct(GameObject indicatorObj)
    {
        yield return new WaitForSeconds(3.0f);
        if (indicatorObj)
        {
            Destroy(indicatorObj);
        }
    }

}