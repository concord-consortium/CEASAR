using UnityEngine;

public class StarComponent : MonoBehaviour
{
    public Star starData;
    public Color starColor;

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
        if (dataController && dataController.starInfoPanel)
        {
            dataController.ChangeConstellationHighlight(starData.Constellation);
        }
    }
    void OnMouseOver()
    {
        if (dataController && dataController.starInfoPanel)
        {
            dataController.ChangeStarSelection(this.gameObject);
            pulse = true;
        }
    }

    void OnMouseExit()
    {
        if (dataController && dataController.starInfoPanel)
        {
            pulse = false;
            transform.localScale = initialScale;
        }
    }
}
