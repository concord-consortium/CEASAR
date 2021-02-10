using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConstellationList : MonoBehaviour
{
    private List<string> allConstellations;
    [SerializeField] private GameObject constellationTextObject;
    [SerializeField] private RectTransform contentContainer;
    public bool HasCompletedSetup = false;
    public IEnumerator InitConstellations(List<string> constellations, string currentConstellation, bool forceUpdate)
    {
        yield return new WaitForEndOfFrame();
        ConstellationItem[] _existingConstellationItems = FindObjectsOfType<ConstellationItem>();
        // We have no constellations in the list, populate dynamically from resources
        // Keeping this option to regenerate the city list in case our list changes
        // in future updates. Ideally, we'll run this in the editor and use a pre-populated prefab
        // to speed things up at runtime. This also helps when moving to VR since all child elements are
        // positioned together when switching to world-space canvas
        if (_existingConstellationItems.Length == 0)
        {
            if (allConstellations == null || allConstellations.Count == 0)
            {
                if (forceUpdate)
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
                }
                // cache the list of names
                allConstellations = constellations;
            }
        }
        HasCompletedSetup = true;
    }

}
