using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CleanStars : MonoBehaviour
{
    public bool cleanStars = false;
    private bool hasCleanedStars = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!hasCleanedStars && cleanStars)
        {
            foreach (StarComponent star in FindObjectsOfType<StarComponent>())
            {
                Destroy(star.GetComponent<SphereCollider>());
                Destroy(star);
            }
            foreach (Constellation constellation in FindObjectsOfType<Constellation>())
            {
                Destroy(constellation);
            }
            foreach (ConstellationsController controller in FindObjectsOfType<ConstellationsController>())
            {
                Destroy(controller);
            }
            foreach (DataController data in FindObjectsOfType<DataController>())
            {
                Destroy(data);
            }
            Destroy(GameObject.Find("Markers"));
            Debug.Log("Clean!");
            hasCleanedStars = true;
        }
    }
}
