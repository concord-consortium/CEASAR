using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SetButtonText : MonoBehaviour
{
    public TextMeshProUGUI buttonText;
    public string initialText = "";
    public string changeText = "";
    int counter = 0;
    // Start is called before the first frame update
    void Start()
    {
        buttonText.text = initialText;
    }

    public void SetText()
    {
        if (buttonText.text == initialText)
        {
            buttonText.text = changeText;
        }
        else
        {
            buttonText.text = initialText;
        }
    }
}
