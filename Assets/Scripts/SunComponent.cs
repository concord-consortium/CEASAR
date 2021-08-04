using UnityEngine;
using UnityEngine.EventSystems;

public class SunComponent : MonoBehaviour, IPointerDownHandler, IPointerExitHandler, IPointerEnterHandler
{
    private MenuController menuController;
    // Scene-specific value
    private Vector3 sceneInitialScale;
    private float scaleFactor = 1.1f;

    // Start is called before the first frame update
    void Start()
    {
       sceneInitialScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CursorHighlightSun(bool highlight)
    {
        if (highlight)
        {
            transform.localScale = sceneInitialScale * scaleFactor;
        } else
        {
            transform.localScale = sceneInitialScale;
        }
    }

    public void HandleSelectSun(bool broadcastToNetwork = false)
    {
        if (!menuController) menuController = FindObjectOfType<MenuController>();
        if (!menuController.IsDrawing)
        {
            CCDebug.Log("Selected sun", LogLevel.Info, LogMessageCategory.Interaction);
            SimulationEvents.Instance.SunSelected.Invoke(true);
        }
    }

    #region MouseEvent Handling
    public void OnPointerDown(PointerEventData eventData)
    {
        HandleSelectSun(true);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        CursorHighlightSun(true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        CursorHighlightSun(false);
    }
    #endregion
}
