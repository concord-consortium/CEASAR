using UnityEngine;
using UnityEngine.EventSystems;

public class StarComponent : MonoBehaviour
{
    public Star starData;
    public Color starColor;
    public Color constellationColor;

    private GameObject mainUIControllerObj;
    private MainUIController mainUIController;
    private GameObject constManagerObj;
    private ConstellationManager constManager;

    void Start()
    {
        mainUIControllerObj = GameObject.Find("MainUI");
        if (mainUIControllerObj)
        {
            mainUIController = mainUIControllerObj.GetComponent<MainUIController>();
        }
        constManagerObj = GameObject.Find("Constellations");
        if (constManagerObj)
        {
            constManager = constManagerObj.GetComponent<ConstellationManager>();
        }
    }

    void OnMouseDown()
    {
        if (!EventSystem.current.IsPointerOverGameObject()
            && mainUIController && mainUIController.starInfoPanel)
        {
            constManager.HighlightSingleConstellation(starData.ConstellationFullName);
            mainUIController.ChangeStarSelection(this.gameObject);
            mainUIController.ChangeConstellationHighlight(starData.ConstellationFullName);
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
