using UnityEngine;
using UnityEngine.UI;

// i got some inspiration from this video 
// https://www.youtube.com/watch?v=tRTbPGalJXk 

public class DroneCameraController : MonoBehaviour
{
    public Transform drone;
    public RawImage droneFeedImage;
    public float heightAboveDrone = 10f;
    public Camera droneCamera;

    void Start()
    {
        if (droneCamera != null && droneFeedImage != null)
        {
            droneCamera.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // double checks that the camera’s rotation is looking straight down
            if (droneCamera.targetTexture == null)
            {
                RenderTexture rt = new RenderTexture(512, 512, 24);
                droneCamera.targetTexture = rt;
                droneFeedImage.texture = rt;
            }
        }
    }

    void Update()
    {
        if (drone != null && droneCamera != null)
        {
            droneCamera.transform.position = drone.position + Vector3.up * heightAboveDrone;
        }
    }
}