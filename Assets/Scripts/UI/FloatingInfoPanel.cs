using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class FloatingInfoPanel : MonoBehaviour, IPointerDownHandler, IPointerExitHandler, IPointerEnterHandler
{
    public TextMeshPro InfoText;
    public string playerName;
    public Color playerColor;

    public void HandleClose()
    {
        Destroy(this.gameObject);
    }

    #region MouseEvent Handling
    public void OnPointerDown(PointerEventData eventData)
    {
        HandleClose();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        // WTD add highlight
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        // WTD add highlight
    }
    #endregion
}
