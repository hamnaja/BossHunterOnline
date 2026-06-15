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
    private Camera mainCam;

    // Internal state
    private Vector3 velocity;
    private Vector2 moveInput;
    private bool isMoving;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        animController = GetComponent<PlayerAnimController>();
        mainCam = Camera.main;
    }

    void Update()
    {
        ApplyMovement();
        ApplyGravity();
    }

    // New Input System เรียก callback นี้อัตโนมัติเมื่อกด WASD
    // ต้องผูก Input Action ชื่อ "Move" ใน Player Input component
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        isMoving = moveInput.magnitude > 0.1f;
    }

    void ApplyMovement()
    {
        if (!isMoving)
        {
            animController?.SetMoving(false, 0f);
            return;
        }

        // แปลง input ให้สัมพันธ์กับทิศกล้อง
        Vector3 camForward = mainCam.transform.forward;
        Vector3 camRight   = mainCam.transform.right;
        camForward.y = 0f;
        camRight.y   = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = (camForward * moveInput.y + camRight * moveInput.x);

        // หมุนตัวละครให้หันตามทิศเดิน
        Quaternion targetRot = Quaternion.LookRotation(moveDir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

        // ขยับจริง
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