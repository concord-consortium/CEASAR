using UnityEngine;
using UnityEngine.EventSystems;

public class StarComponent : MonoBehaviour
{
    public Star starData;
    public Color starColor;
    public Color constellationColor;

    private GameObject dataControllerObj;
    private DataController dataController;
    private GameObject mainUIControllerObj;
    private MainUIController mainUIController;

    void Start()
    {
        dataControllerObj = GameObject.Find("DataController");
        if (dataControllerObj)
        {
            dataController = dataControllerObj.GetComponent<DataController>();
        }
        mainUIControllerObj = GameObject.Find("MainUI");
        if (mainUIControllerObj)
        {
            mainUIController = mainUIControllerObj.GetComponent<MainUIController>();
        }
    }

    void OnMouseDown()
    {
        if (!EventSystem.current.IsPointerOverGameObject()
            && mainUIController && mainUIController.starInfoPanel
            && dataController)
        {
            dataController.ChangeConstellationHighlight(starData.Constellation);
            mainUIController.ChangeStarSelection(this.gameObject);
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
