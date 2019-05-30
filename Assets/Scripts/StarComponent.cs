using UnityEngine;

public class StarComponent : MonoBehaviour
{
    public Star starData;
    public Color starColor;

    private GameObject dataController;

    private Vector3 initialScale;

    private bool pulse = false;
    private float pulseSpeed = 5f;


    void Start()
    {
        dataController = GameObject.Find("DataController");
        initialScale = transform.localScale;
    }

    void Update()
    {
        if (pulse)
        {
            float currScale = transform.localScale.x;
            if (currScale > initialScale.x * 2f || currScale < initialScale.x)
            {
                pulseSpeed *= -1f;
            }
            currScale += pulseSpeed * Time.deltaTime;
            transform.localScale = new Vector3(currScale, currScale, currScale);
        }
    }

    void OnMouseDown()
    {
        dataController.GetComponent<DataController>().ChangeConstellationHighlight(starData.Constellation);
    }
    void OnMouseOver()
    {
        dataController.GetComponent<DataController>().ChangeStarSelection(this.gameObject);
        pulse = true;
    }

    void OnMouseExit()
    {
        pulse = false;
        transform.localScale = initialScale;
    }
}
