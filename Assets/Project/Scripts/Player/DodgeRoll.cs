using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class DodgeRoll : MonoBehaviour
{
    [Header("Dodge Settings")]
    [SerializeField] private float dodgeSpeed = 12f;
    [SerializeField] private float dodgeDuration = 0.3f;
    [SerializeField] private float staminaCost = 20f;
    [SerializeField] private int iFrames = 12;           // ตาม GDD Section 16.1
    [SerializeField] private int perfectDodgeWindow = 3; // 3 เฟรมสุดท้าย

    // Components
    private CharacterController cc;
    private HealthSystem healthSystem;
    private StaminaSystem staminaSystem;
    private PlayerController playerController;

    // State
    private bool isDodging = false;
    private Vector3 dodgeDirection;

    // Events
    public static event System.Action OnPerfectDodge;

    // BossAttackTrigger จะ set ค่านี้เมื่อบอสโจมตี
    public static bool BossIsAttacking = false;

    void Awake()
    {
        cc               = GetComponent<CharacterController>();
        healthSystem     = GetComponent<HealthSystem>();
        staminaSystem    = GetComponent<StaminaSystem>();
        playerController = GetComponent<PlayerController>();
    }

    // Player Input เรียก method นี้เมื่อกด Space
    public void OnDodge(InputValue value)
    {
        if (!value.isPressed) return;
        if (isDodging) return;
        if (!staminaSystem.ConsumeStamina(staminaCost)) return;

        // ทิศหลบ = ทิศที่กำลังเดินอยู่ ถ้าไม่ได้กดให้หลบถอยหลัง
        dodgeDirection = playerController.IsMoving
            ? transform.forward
            : -transform.forward;

        StartCoroutine(DodgeRoutine());
    }

    IEnumerator DodgeRoutine()
    {
        isDodging = true;
        healthSystem.isInvulnerable = true;

        float elapsed = 0f;

        while (elapsed < dodgeDuration)
        {
            cc.Move(dodgeDirection * dodgeSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;

            // คำนวณว่าอยู่เฟรมไหน
            int currentFrame = Mathf.FloorToInt((elapsed / dodgeDuration) * iFrames);
            bool inPerfectWindow = currentFrame >= (iFrames - perfectDodgeWindow);

            // Perfect Dodge — หลบในช่วง 3 เฟรมสุดท้ายขณะบอสโจมตี
            if (inPerfectWindow && BossIsAttacking)
            {
                OnPerfectDodge?.Invoke();
                Debug.Log("[DodgeRoll] PERFECT DODGE! Bullet Time + Crit 100%");
            }

            yield return null;
        }

        healthSystem.isInvulnerable = false;
        isDodging = false;
    }

    public bool IsDodging => isDodging;
}
