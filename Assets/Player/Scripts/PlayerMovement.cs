using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public bool IsMoving { get { return relativeMovementVector.sqrMagnitude > 0.1f; } }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float groundDrag = 1f;
    [SerializeField] private float airMultiplier = 0.4f;

    [Header("Jump")]
    public float maxJumpVelocity = 18f;
    public float gravity = 9.81f;
    [SerializeField][Range(0, 1)] float jumpCutOff = 0.5f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundDistance;
    [SerializeField] private float jumpDelay = 0.15f;
    [SerializeField] private float groundDelay = 0.15f;

    [Header("References")]
    [SerializeField] private Transform playerTarget;
    [SerializeField] private Transform playerBody;

    [HideInInspector] public Rigidbody playerRigidbody;
    [HideInInspector] public Vector3 relativeMovementVector;

    private Vector2 normalizedMovementVector;

    private float groundTimer;
    private float jumpTimer;
    private float currentJumpVelocity;

    [HideInInspector] public bool isGrounded;
    private bool wasGrounded;

    private PlayerInput playerInput;

    public event Action OnJump;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        playerInput = new PlayerInput();
    }

    private void OnEnable()
    {
        playerInput.Enable();
    }

    private void OnDisable()
    {
        playerInput.Disable();

        playerInput.Player.Move.performed -= SetMovementVector;
        playerInput.Player.Move.canceled -= CancelMovement;

        playerInput.Player.Jump.started -= InitiateJump;
        playerInput.Player.Jump.canceled -= CutJump;
    }

    private void Start()
    {
        playerInput.Player.Move.performed += SetMovementVector;
        playerInput.Player.Move.canceled += CancelMovement;

        playerInput.Player.Jump.started += InitiateJump;
        playerInput.Player.Jump.canceled += CutJump;

        currentJumpVelocity = maxJumpVelocity;
    }
    private void SetMovementVector(InputAction.CallbackContext callbackContext)
    {
        normalizedMovementVector = callbackContext.ReadValue<Vector2>();
    }

    private void CancelMovement(InputAction.CallbackContext callbackContext)
    {
        normalizedMovementVector = Vector2.zero;
    }

    private void FixedUpdate()
    {
        CalculateRelativeMovementVector();
        HorizontalMovement();
        ApplyGravity();

        if (jumpTimer > Time.time && (groundTimer > Time.time || isGrounded))
        {
            Jump(currentJumpVelocity);
        }
    }

    private void ApplyGravity()
    {
        playerRigidbody.AddForce(gravity * playerRigidbody.mass * Vector3.down);
    }

    private void CalculateRelativeMovementVector()
    {
        relativeMovementVector =
            normalizedMovementVector.y * playerTarget.forward +
            normalizedMovementVector.x * playerTarget.right;

        relativeMovementVector.Normalize();
    }

    private void HorizontalMovement()
    {
        //Vector3 newPlayerVelocity = moveSpeed * Time.deltaTime * relativeMovementVector;

        //newPlayerVelocity.y = playerRigidbody.velocity.y;
        //playerRigidbody.velocity = newPlayerVelocity;

        Vector3 moveForce = 10f * moveSpeed * relativeMovementVector.normalized;
        if (!isGrounded) { moveForce *= airMultiplier; }

        playerRigidbody.AddForce(moveForce, ForceMode.Force);
    }

    private void Update()
    {
        wasGrounded = isGrounded;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundDistance, groundLayer);

        // take off (coyote time)
        if (wasGrounded && !isGrounded)
        {
            groundTimer = Time.time + groundDelay;
            wasGrounded = false;
        }

        if (isGrounded) { playerRigidbody.drag = groundDrag; }
        else { playerRigidbody.drag = 0; }

        SpeedControl();
    }

    private void SpeedControl()
    {
        Vector3 flatVelocity = new(playerRigidbody.velocity.x, 0f, playerRigidbody.velocity.z);

        if (flatVelocity.magnitude > moveSpeed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * moveSpeed;
            playerRigidbody.velocity = new Vector3(limitedVelocity.x, playerRigidbody.velocity.y, limitedVelocity.z);
        }
    }

    private void InitiateJump(InputAction.CallbackContext callbackContext)
    {
        jumpTimer = Time.time + jumpDelay;
    }

    private void CutJump(InputAction.CallbackContext callbackContext)
    {
        if (jumpTimer > 0)
        {
            currentJumpVelocity = maxJumpVelocity * (jumpCutOff + 0.1f);
        }
        else if (playerRigidbody.velocity.y > 0)
        {
            playerRigidbody.velocity = new Vector3(
                playerRigidbody.velocity.x,
                playerRigidbody.velocity.y * jumpCutOff,
                playerRigidbody.velocity.z
            );
        }
    }

    private void Jump(float jumpVelocity)
    {
        playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, jumpVelocity, playerRigidbody.velocity.z);

        OnJump?.Invoke();

        jumpTimer = 0;
        groundTimer = 0;
        currentJumpVelocity = maxJumpVelocity;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, Vector2.down * groundDistance);
    }
}

