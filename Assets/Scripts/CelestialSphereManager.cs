using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialSphereManager : MonoBehaviour
{
    private float moveSpeed = .1f;
    private float scaleSpeed = 5f;
    private float maxScale = 100f;
    private float minScale = .025f;
    private float rotateSpeed = 10f;
    private float autoRotateSpeed = 1f;
    private bool rotating = false;
    public GameObject markers;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        handleAutoRotation();
    }

    public void MoveLeft()
    {
        Vector3 pos = this.transform.position;
        pos.x -= Time.deltaTime + moveSpeed;
        this.transform.position = pos;
    }

    public void MoveRight()
    {
        Vector3 pos = this.transform.position;
        pos.x += Time.deltaTime + moveSpeed;
        this.transform.position = pos;
    }

    public void MoveDown()
    {
        Vector3 pos = this.transform.position;
        pos.y -= Time.deltaTime + moveSpeed;
        this.transform.position = pos;
    }

    public void MoveUp()
    {
        Vector3 pos = this.transform.position;
        pos.y += Time.deltaTime + moveSpeed;
        this.transform.position = pos;
    }

    public void MoveBack()
    {
        Vector3 pos = this.transform.position;
        pos.z += Time.deltaTime + moveSpeed;
        this.transform.position = pos;
    }

    public void MoveForward()
    {
        Vector3 pos = this.transform.position;
        pos.z -= Time.deltaTime + moveSpeed;
        this.transform.position = pos;
    }

    public void DecreaseScale()
    {
        if (this.transform.localScale.x > minScale)
        {
            float scaleIncrement = this.transform.localScale.x * .25f * Time.deltaTime;
            this.transform.localScale -= new Vector3(scaleIncrement, scaleIncrement, scaleIncrement);
        }
    }
    public void IncreaseScale()
    {
        if (this.transform.localScale.x < maxScale)
        {
            float scaleIncrement = this.transform.localScale.x * .25f * Time.deltaTime;
            this.transform.localScale += new Vector3(scaleIncrement, scaleIncrement, scaleIncrement);
        }
    }

    public void RotateYAxisUp()
    {
        this.transform.Rotate(Vector3.down, rotateSpeed * Time.deltaTime);
    }
    public void RotateYAxisDown()
    {
        this.transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }
    public void RotateXAxisLeft()
    {
        this.transform.Rotate(Vector3.left, rotateSpeed  * Time.deltaTime);
    }
    public void RotateXAxisRight()
    {
        this.transform.Rotate(Vector3.right, rotateSpeed * Time.deltaTime);
    }
    public void RotateZAxisForward()
    {
        this.transform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);
    }
    public void RotateZAxisBack()
    {
        this.transform.Rotate(Vector3.back, rotateSpeed * Time.deltaTime);
    }

    public void ToggleMarkerVisibility()
    {
        if (markers.activeSelf)
        {
            markers.SetActive(false);
        }
        else
        {
            markers.SetActive(true);
        }
    }

    public void ToggleAutoRotate()
    {
        rotating = !rotating;
    }

    private void handleAutoRotation()
    {
        if (rotating)
        {
            this.transform.Rotate(Vector3.right, autoRotateSpeed * Time.deltaTime);
            this.transform.Rotate(Vector3.back, autoRotateSpeed * Time.deltaTime);
            this.transform.Rotate(Vector3.down, autoRotateSpeed * Time.deltaTime);
        }
    }

    public void Reset()
    {
        this.transform.position = new Vector3(0, 0, 0f);
        this.transform.localScale = new Vector3(1f, 1f, 1f);
        this.transform.rotation = Quaternion.identity;
    }
}
