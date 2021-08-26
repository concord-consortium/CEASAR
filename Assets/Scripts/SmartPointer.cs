using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Text;

public class SmartPointer : MonoBehaviour
{
    private SimulationManager manager { get { return SimulationManager.Instance; } }
    private SimulationEvents events { get { return SimulationEvents.Instance; } }

    public GameObject FloatingInfoPanelPrefab;
    private GameObject FloatingInfoPanel;

    // Start is called before the first frame update
    void Start()
    {
        events.StarSelected.AddListener(StarSelected);
        events.MoonSelected.AddListener(MoonSelected);
        events.MoonSelected.AddListener(SunSelected);
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(new Vector3(0,0,0));
    }

    private void StarSelected(Star selectedStarData, string playerName, Color playerColor)
    {
        if (FloatingInfoPanel)
            Destroy(FloatingInfoPanel);
    }
    private void MoonSelected(bool selected)
    {
        if (FloatingInfoPanel)
            Destroy(FloatingInfoPanel);
    }
    private void SunSelected(bool selected)
    {
        if (FloatingInfoPanel)
            Destroy(FloatingInfoPanel);
    }

    public void SmartPoint(Vector3 position, float radius, string playerName, Color playerColor)
    {
        this.transform.SetPositionAndRotation(position, Quaternion.identity);
        // GameObject PanelHolder = Instantiate(), startPointForDrawing, Quaternion.identity, this.transform);
        Vector3 spherePos = this.transform.localPosition;

        if (FloatingInfoPanel)
            Destroy(FloatingInfoPanel);

        // make a new floating info panel that is a child of the star
        FloatingInfoPanel = Instantiate(FloatingInfoPanelPrefab, new Vector3(0, -8f, 6f), new Quaternion(0, 0, 0, 0), this.transform);
        FloatingInfoPanel.transform.localPosition = new Vector3(0, -6f, 5f);
        FloatingInfoPanel.transform.localScale = new Vector3(.8f, .8f, .8f);
        FloatingInfoPanel.GetComponent<FloatingInfoPanel>().playerName = playerName;
        FloatingInfoPanel.GetComponent<FloatingInfoPanel>().playerColor = playerColor;

        SetSmartPointerText(spherePos, radius);
    }

    private void SetSmartPointerText(Vector3 position, float radius)
    {
        // float azimuth = 180f + (azimuth * 180f / Mathf.PI);
        // float altitude = altitude * 180f / Mathf.PI;
        float declinationInRadians = Mathf.Asin(position.y / radius);
        float declination = declinationInRadians * 180f / Mathf.PI;
        float ra = Mathf.Acos( (position.x / (radius * Mathf.Cos(declinationInRadians))) );
        float raInDegrees = ra * 180f / Mathf.PI;
        float raInHours = raInDegrees / 15f;
        if (raInHours < 0) raInHours = 24f + raInHours;
        // Debug.Log("smart pointer radian dec: " + declinationInRadians);
        // Debug.Log("smart pointer deg dec: " + declination);
        // Debug.Log("smart pointer radian ra: " + ra);
        // Debug.Log("smart pointer deg ra: " + raInDegrees);
        // Debug.Log("smart pointer hour ra: " + raInHours);

        StringBuilder description = new StringBuilder();
        description.AppendLine("Celestial Sphere Point");
        // description.Append("Alt/Az: ")
        //     .Append(altitude.ToString("F2"))
        //     .Append("°, ")
        //     .Append(azimuth.ToString("F2"))
        //     .AppendLine("°");
        description.Append("R.A.: ")
            .Append(raInHours.ToString("F2"))
            .Append("h  Dec: ")
            .Append(declination.ToString("F2"))
            .AppendLine("°");
        FloatingInfoPanel.GetComponent<FloatingInfoPanel>().InfoText.SetText(description.ToString());
    }

    private void removeAllListeners()
    {
        events.StarSelected.RemoveListener(StarSelected);
        events.MoonSelected.RemoveListener(MoonSelected);
        events.SunSelected.RemoveListener(SunSelected);
    }

    private void OnDestroy()
    {
       removeAllListeners();
    }
}
