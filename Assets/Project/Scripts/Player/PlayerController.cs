using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float gravity = -20f;

    // Components
    private CharacterController cc;
    private PlayerAnimController animController;
    private PlayerCombat playerCombat;
    private Camera mainCam;

    // Internal state
    private Vector3 velocity;
    private Vector2 moveInput;
    private bool isMoving;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        animController = GetComponent<PlayerAnimController>();
        playerCombat = GetComponent<PlayerCombat>();
        mainCam = Camera.main;
    }

    void Update()
    {
        ApplyMovement();
        ApplyGravity();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        isMoving = moveInput.magnitude > 0.1f;
    }

    public void OnLightAttack(InputValue value)
    {
        if (value.isPressed)
            playerCombat?.OnLightAttack();
    }

    public void OnHeavyAttack(InputValue value)
    {
        if (value.isPressed)
            playerCombat?.OnHeavyAttack();
    }

    void ApplyMovement()
    {
        if (!isMoving)
        {
            animController?.SetMoving(false, 0f);
            return;
        }

        Vector3 camForward = mainCam.transform.forward;
        Vector3 camRight   = mainCam.transform.right;
        camForward.y = 0f;
        camRight.y   = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = (camForward * moveInput.y + camRight * moveInput.x);

        Quaternion targetRot = Quaternion.LookRotation(moveDir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

        cc.Move(moveDir * moveSpeed * Time.deltaTime);
        animController?.SetMoving(true, moveInput.magnitude);
    }

    void ApplyGravity()
    {
        if (cc.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
    }

    public Vector2 MoveInput => moveInput;
    public bool IsMoving => isMoving;
}