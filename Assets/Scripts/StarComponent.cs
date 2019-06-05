using UnityEngine;
using UnityEngine.EventSystems;

public class StarComponent : MonoBehaviour
{
    public Star starData;
    public Color starColor;
    public Color constellationColor;

    private GameObject dataControllerObj;
    private DataController dataController;

    private Vector3 initialScale;

    private bool pulse = false;
    private float pulseSpeed = 5f;
    private float maxScaleFactor = 2f;

    void Start()
    {
        dataControllerObj = GameObject.Find("DataController");
        if (dataControllerObj)
        {
            dataController = dataControllerObj.GetComponent<DataController>();
        }
        initialScale = transform.localScale;
    }

    void Update()
    {
        if (pulse)
        {
            float currScale = transform.localScale.x;
            if (currScale > initialScale.x * maxScaleFactor || currScale < initialScale.x)
            {
                pulseSpeed *= -1f;
            }
            currScale += pulseSpeed * Time.deltaTime;
            transform.localScale = new Vector3(currScale, currScale, currScale);
        }
    }

    void OnMouseDown()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && dataController && dataController.starInfoPanel)
        {
            dataController.ChangeConstellationHighlight(starData.Constellation);
            Debug.Log("(BD=" + starData.BayerDesignation + "),(FD=" + starData.FlamsteedDesignation + "),(ConstShort=" + starData.Constellation + "),(ConstFull=" + starData.ConstellationFullName + ")");
        }
    }
    void OnMouseOver()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && dataController && dataController.starInfoPanel)
        {
            dataController.ChangeStarSelection(this.gameObject);
            pulse = true;
        }
    }

    void OnMouseExit()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && dataController && dataController.starInfoPanel)
        {
            pulse = false;
            transform.localScale = initialScale;
        }
    }

    public void SetStarColor(Color constellationColor, Color starColor)
    {
        // Store star color - not yet using real values
        this.starColor = starColor;
        // Store constellation color
        this.constellationColor = constellationColor;

    }
}
