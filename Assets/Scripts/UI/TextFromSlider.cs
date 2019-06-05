using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextFromSlider : MonoBehaviour
{
    public int decimalDigits = 2;

    private TextMeshProUGUI TMText;

    // Start is called before the first frame update
    void Start()
    {
        TMText = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TextUpdate(float val)
    {
        string formatter = "F" + decimalDigits.ToString();
        TMText.text = val.ToString(formatter);
    }
}
