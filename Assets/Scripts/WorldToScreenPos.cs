using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldToScreenPos : MonoBehaviour
{
    public RectTransform canvasRect;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdatePosition(GameObject target)
    {
        // Calculate *screen* position
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(target.transform.position);

        // Offset position below object (in world space)
        float UIscale = canvasRect.localScale.x;
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 offsetPos = new Vector2(screenPoint.x + rectTransform.rect.width * UIscale * .5f,
                                        screenPoint.y - rectTransform.rect.height * UIscale * .5f);

        // Convert screen position to Canvas / RectTransform space <- leave camera null if Screen Space Overlay
        Vector2 canvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, offsetPos, null, out canvasPos);

        transform.localPosition = canvasPos;
    }
}
