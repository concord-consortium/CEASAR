/*===============================================================================
Copyright (c) 2018 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/
using UnityEngine;
using UnityEngine.UI;

public class StatusMessage : MonoBehaviour
{
    #region PRIVATE_MEMBERS

    CanvasGroup canvasGroup;
    Text message;
    bool initialized;
    static StatusMessage statusMessage;

    #endregion // PRIVATE_MEMBERS


    #region PUBLIC_PROPERTIES

    public static StatusMessage Instance
    {
        get
        {
            if (statusMessage == null)
            {
                GameObject prefab = (GameObject)Resources.Load("StatusMessage");
                if (prefab)
                {
                    statusMessage = Instantiate(prefab.GetComponent<StatusMessage>());
                    statusMessage.Init();
                    return statusMessage;
                }
                // If prefab not found, return null
                return null;
            }
            return statusMessage;
        }
    }

    #endregion // PUBLIC_PROPERTIES


    #region PRIVATE_METHODS

    void Init()
    {
        if (!this.initialized)
        {
            this.canvasGroup = GetComponentInChildren<CanvasGroup>();
            this.canvasGroup.alpha = 0;
            this.message = GetComponentInChildren<Text>();
            this.message.text = "";
            this.initialized = true;
        }
    }

    #endregion // PRIVATE_METHODS


    #region PUBLIC_METHODS

    public void Display(string message)
    {
        this.message.text = message;

        this.canvasGroup.alpha = (message.Length > 0) ? 1 : 0;
    }

    #endregion PUBLIC_METHODS

}
