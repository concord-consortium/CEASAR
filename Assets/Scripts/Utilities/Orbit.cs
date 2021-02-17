using UnityEngine;

public class Orbit : MonoBehaviour
{
    public float orbitSpeed = 10f;
    public Transform orbitCenter;
    private GameObject orbitPivot;
    public float orbitRadius;

    private void Start()
    {
        if (orbitCenter)
        {
            orbitPivot = new GameObject();
            orbitPivot.name = gameObject.name + "_pivot";
            orbitPivot.transform.parent = orbitCenter;
            orbitPivot.transform.localPosition = new Vector3(0, 0, 0);
            transform.parent = orbitPivot.transform;
        }
    }
    void Update()
    {
        if (orbitCenter && orbitPivot)
        {
            orbitPivot.transform.Rotate(Vector3.up, orbitSpeed * Time.deltaTime, Space.World);

        }
    }
}