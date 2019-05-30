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
        Vector2 canvasPos;
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(target.transform.position);

        // Offset position below object (in world space)
        float UIscale = canvasRect.localScale.x;
        Vector2 offsetPos = new Vector2(screenPoint.x + 90f * UIscale, screenPoint.y - 55f * UIscale);

        // Convert screen position to Canvas / RectTransform space <- leave camera null if Screen Space Overlay
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, offsetPos, null, out canvasPos);

        this.transform.localPosition = canvasPos;
    }
}
