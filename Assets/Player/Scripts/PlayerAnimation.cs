using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("Animation Smoothing")]
    [SerializeField, Range(0, 1f)] private float HorizontalAnimSmoothTime = 0.2f;
    [SerializeField, Range(0, 1f)] private float VerticalAnimTime = 0.2f;
    [SerializeField, Range(0, 1f)] private float StartAnimTime = 0.3f;
    [SerializeField, Range(0, 1f)] private float StopAnimTime = 0.15f;
    [SerializeField] private float allowPlayerRotation = 0.1f;

    private Animator animator;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        playerMovement = GetComponent<PlayerMovement>();

        playerMovement.OnJump += SetJumpTrigger;
    }

    private void Update()
    {
        animator.SetBool("Grounded", playerMovement.isGrounded);
        animator.SetBool("Moving", playerMovement.IsMoving);
    }

    private void SetJumpTrigger()
    {
        animator.SetTrigger("Jump");
    }
}

