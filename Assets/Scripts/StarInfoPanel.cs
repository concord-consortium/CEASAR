using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StarInfoPanel : MonoBehaviour
{
    public TextMeshProUGUI starXByerFlamsteedText;
    public TextMeshProUGUI starMagText;
    public TextMeshProUGUI starConstellationText;

    private bool isEnabled = true;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void setEnabled(bool enabled)
    {
        if (enabled)
        {
            isEnabled = true;
        }
        else
        {
            gameObject.SetActive(false);
            isEnabled = false;
        }
    }

    public void UpdateStarInfoPanel(string XByerFlamsteed, string magnitude, string constellation)
    {
        if (isEnabled && !gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        starXByerFlamsteedText.text = "ID: " + XByerFlamsteed;
        starMagText.text = "Mag: " + magnitude;
        starConstellationText.text = "Const: " + constellation;
    }
}
