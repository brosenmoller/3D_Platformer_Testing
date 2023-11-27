using Cinemachine;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float cameraRotationSpeed;

    [Header("References")]
    [SerializeField] private Transform playerTarget;
    [SerializeField] private Transform playerBody;
    [SerializeField] private CinemachineVirtualCameraBase playerCamera;

    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void FixedUpdate()
    {
        UpdateCamera();
    }

    private void UpdateCamera()
    {
        Vector3 cameraLookDirection = transform.position - playerCamera.transform.position;
        cameraLookDirection.y = 0;
        playerTarget.forward = cameraLookDirection.normalized;

        if (playerMovement.relativeMovementVector != Vector3.zero || (playerBody.position - playerMovement.relativeMovementVector).sqrMagnitude < 0.1f)
        {
            playerBody.forward = Vector3.Slerp(
                playerBody.forward,
                playerMovement.relativeMovementVector,
                Time.deltaTime * cameraRotationSpeed
            );
        }
    }
}
