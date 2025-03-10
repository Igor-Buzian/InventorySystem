using UnityEngine;
using Cinemachine;


public class FirstPersonView : MonoBehaviour
{
    public Transform characterBody; // The body of the character
    public CinemachineVirtualCamera cinemachineCamera; // Reference to the virtual camera

    [Header("Camera Rotation Limits")]
    public float minVerticalAngle = -45f; // Minimum vertical angle of the camera
    public float maxVerticalAngle = 45f;  // Maximum vertical angle of the camera

    private CinemachinePOV cinemachinePOV; // Camera rotation controller


    private void Start()
    {
        // Get the POV component from the Cinemachine Virtual Camera
        cinemachinePOV = cinemachineCamera.GetCinemachineComponent<CinemachinePOV>();
        if (cinemachinePOV == null)
        {
            Debug.LogError("CinemachinePOV component not found! Check camera setup.");
        }
    }

    /// <summary>
    /// Called once per frame, after all Update methods.
    /// Clamps the vertical rotation of the camera and rotates the character body.
    /// </summary>
    private void LateUpdate()
    {
        // Limit the vertical rotation angle of the camera
        if (cinemachinePOV != null)
        {
            cinemachinePOV.m_VerticalAxis.Value = Mathf.Clamp(cinemachinePOV.m_VerticalAxis.Value, minVerticalAngle, maxVerticalAngle);
        }

        // Rotate the character body according to the camera's horizontal rotation
        RotateCharacterWithCamera();
    }

    /// <summary>
    /// Rotates the character body to match the camera's horizontal rotation.
    /// </summary>
    private void RotateCharacterWithCamera()
    {
        // Get the current horizontal rotation angle of the camera
        float cameraYaw = cinemachinePOV.m_HorizontalAxis.Value;

        // Rotate the character body around the Y axis
        characterBody.rotation = Quaternion.Euler(0, cameraYaw, 0);
    }
}