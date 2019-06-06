using UnityEngine;

// This class is the manager Singleton, and contains specific references to application-level objects
public class SimulationManager
{

    // can't use constructor, guaranteed singleton
    protected SimulationManager() { }
    private static SimulationManager instance;

    public static SimulationManager GetInstance()
    {
        return instance ?? (instance = new SimulationManager());
    }

    public GameObject NetworkControllerObject;
}