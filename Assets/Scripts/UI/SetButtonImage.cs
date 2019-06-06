using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetButtonImage : MonoBehaviour
{
    public Sprite initialSprite;
    public Sprite changeSprite;
    public Button button;
    // Start is called before the first frame update
    void Start()
    {
        button.image.sprite = initialSprite;
    }

    public void SetImage()
    {
        if (button.image.sprite == initialSprite)
        {
            button.image.sprite = changeSprite;
        }
        else
        {
            button.image.sprite = initialSprite;
        }
    }
}