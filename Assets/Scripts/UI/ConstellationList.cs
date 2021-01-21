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
            allConstellations.Add(SimulationConstants.CONSTELLATIONS_ALL);
            allConstellations.Add(SimulationConstants.CONSTELLATIONS_NONE);
            allConstellations.AddRange(constellations);
            
            string[] _constellations = allConstellations.ToArray();
            
            for (int i = 0; i < _constellations.Length; i++)
            {
                string c = _constellations[i];
                if (!string.IsNullOrEmpty(c))
                {
                    GameObject constellationObject = Instantiate(constellationTextObject);
                    constellationObject.transform.SetParent(contentContainer);
                    constellationObject.transform.localScale = Vector3.one;
                    bool isSelected = currentConstellation == c;
                    constellationObject.GetComponent<ConstellationItem>().Init(c, isSelected);
                }
            }

            // cache the list of names
            allConstellations = constellations;
        }
        
    }

}
