using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ARTrackedImageManager))]
public class PlaceOnImageTarget : MonoBehaviour
{
    ARTrackedImageManager m_TrackedImageManager;
    [SerializeField]
    private Canvas debugCanvas;

    void Awake()
    {
        m_TrackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void UpdateInfo(ARTrackedImage trackedImage)
    {
        if (debugCanvas)
        {
            Text t = debugCanvas.GetComponentInChildren<Text>();
            t.text = string.Format(
                "{0}\ntrackingState: {1}\nGUID: {2}\nReference size: {3} cm\nDetected size: {4} cm",
                trackedImage.referenceImage.name,
                trackedImage.trackingState,
                trackedImage.referenceImage.guid,
                trackedImage.referenceImage.size * 100f,
                trackedImage.size * 100f);
        }
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            trackedImage.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            
            foreach (Transform t in trackedImage.transform)
            {
                Debug.Log(t.name + " " + trackedImage.referenceImage.name);
                if (trackedImage.referenceImage.name.StartsWith(t.name))
                {
                    t.gameObject.SetActive(true);
                }
                else
                {
                    t.gameObject.SetActive(false);
                }
            }
            UpdateInfo(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated)
            UpdateInfo(trackedImage);
    }
}
