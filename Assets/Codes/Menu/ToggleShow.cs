using UnityEngine;

public class ToggleShow : MonoBehaviour
{
    public float yOffset = -0.1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Toggle()
    {
        if (this.gameObject.activeInHierarchy == true)
        {
            this.gameObject.SetActive(false);
        }
        else
        {
            this.gameObject.SetActive(true);
        }
    }

    public void SetPositionFrontOfCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Vector3 cameraForward = mainCamera.transform.forward;
            Vector3 spawnPosition = mainCamera.transform.position + cameraForward * 0.5f;
            spawnPosition.y += yOffset;

            this.transform.position = spawnPosition;
            this.transform.LookAt(mainCamera.transform);
        }
    }
}
