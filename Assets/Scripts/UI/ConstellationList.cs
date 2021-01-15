using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstellationList : MonoBehaviour
{
    private List<string> allConstellations;
    [SerializeField] private GameObject constellationTextObject;
    [SerializeField] private RectTransform contentContainer;
    public void InitConstellations(List<string> constellations, string currentConstellation)
    {
        if (allConstellations == null || allConstellations.Count == 0)
        {
            constellations.Sort();
            allConstellations = new List<string>();
            allConstellations.Add("all");
            allConstellations.Add("none");
            allConstellations.AddRange(constellations);
            
            string[] _constellations = allConstellations.ToArray();
            
            for (int i = 0; i < _constellations.Length; i++)
            {
                GameObject constellationObject = Instantiate(constellationTextObject);
                constellationObject.transform.SetParent(contentContainer);
                constellationObject.transform.localScale = Vector3.one;
                bool isSelected = currentConstellation == _constellations[i];
                constellationObject.GetComponent<ConstellationItem>().Init(_constellations[i], isSelected);
            }

            // cache the list of names
            allConstellations = constellations;
        }
        
    }

}
