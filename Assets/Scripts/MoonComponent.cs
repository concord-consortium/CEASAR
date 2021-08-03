using UnityEngine;
using UnityEngine.EventSystems;

public class MoonComponent : MonoBehaviour, IPointerDownHandler, IPointerExitHandler, IPointerEnterHandler
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

    public void HandleSelectMoon(bool broadcastToNetwork = false)
    {
        if (!menuController) menuController = FindObjectOfType<MenuController>();
        if (!menuController.IsDrawing)
        {
            CCDebug.Log("Selected moon", LogLevel.Info, LogMessageCategory.Interaction);
            SimulationEvents.Instance.MoonSelected.Invoke(true);
        }
    }

    public void CursorHighlightMoon(bool highlight)
    {
        if (highlight)
        {
            transform.localScale = sceneInitialScale * scaleFactor;
        } else
        {
            transform.localScale = sceneInitialScale;
        }
    }

    #region MouseEvent Handling
    public void OnPointerDown(PointerEventData eventData)
    {
        HandleSelectMoon(true);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        CursorHighlightMoon(true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        CursorHighlightMoon(false);
    }
    #endregion
}
