using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This component is a MonoBehaviour so we can add references to prefabs and scene stuff in the inspector
public class SimulationManagerComponent : MonoBehaviour
{
    // A local reference to the Singleton manager
    private SimulationManager manager;

    [SerializeField]
    private GameObject networkControllerPrefab;

    private void Awake()
    {
        manager = SimulationManager.GetInstance();
    }
    private void Start()
    {
        if (manager.NetworkControllerObject == null)
        {
            Debug.Log("Creating new network object");
            manager.NetworkControllerObject = Instantiate(networkControllerPrefab);
        }
    }
}