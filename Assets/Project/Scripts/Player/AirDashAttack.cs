using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class AirDashAttack : MonoBehaviour
{
    [Header("Air Attack")]
    [SerializeField] private float plungeSpeed = 20f;
    [SerializeField] private float plungeAoERadius = 3f;
    [SerializeField] private float plungeDamageMultiplier = 1.8f;
    [SerializeField] private LayerMask bossLayer;

    [Header("Dash Attack")]
    [SerializeField] private float dashAttackSpeed = 10f;
    [SerializeField] private float dashAttackDuration = 0.2f;
    [SerializeField] private float dashAttackDamageMultiplier = 1.3f;

    // Components
    private CharacterController cc;
    private PlayerCombat playerCombat;
    private PlayerController playerController;
    private StaminaSystem staminaSystem;
    private Animator anim;

    // State
    private bool isPlunging = false;
    private bool wasInAir = false;

    void Awake()
    {
        cc               = GetComponent<CharacterController>();
        playerCombat     = GetComponent<PlayerCombat>();
        playerController = GetComponent<PlayerController>();
        staminaSystem    = GetComponent<StaminaSystem>();
        anim             = GetComponent<Animator>();
    }

    void Update()
    {
        wasInAir = !cc.isGrounded;
    }

    public void TryAirAttack()
    {
        if (!wasInAir) return;
        if (isPlunging) return;
        if (!staminaSystem.ConsumeStamina(15f)) return;
        StartCoroutine(PlungeAttack());
    }

    public void TryDashAttack()
    {
        if (!playerController.IsMoving) return;
        if (!staminaSystem.ConsumeStamina(10f)) return;
        StartCoroutine(DashAttackRoutine());
    }

    IEnumerator PlungeAttack()
    {
        isPlunging = true;
        anim?.SetTrigger("AirAttack");

        while (!cc.isGrounded)
        {
            cc.Move(Vector3.down * plungeSpeed * Time.deltaTime);
            yield return null;
        }

        // AoE Shockwave เมื่อลงพื้น
        Collider[] hits = Physics.OverlapSphere(transform.position, plungeAoERadius, bossLayer);
        foreach (var hit in hits)
        {
            var health = hit.GetComponent<HealthSystem>();
            if (health != null)
            {
                float damage = 50f * plungeDamageMultiplier;
                health.TakeDamage(damage, 0);
                Debug.Log($"[AirAttack] Plunge AoE hit {hit.name} for {damage}");
            }
        }

        isPlunging = false;
    }

    IEnumerator DashAttackRoutine()
    {
        anim?.SetTrigger("DashAttack");

        float elapsed = 0f;
        Vector3 dashDir = transform.forward;

        while (elapsed < dashAttackDuration)
        {
            cc.Move(dashDir * dashAttackSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // เช็ค hit ตรงหน้า
        Collider[] hits = Physics.OverlapSphere(
            transform.position + transform.forward * 1.5f, 1.5f, bossLayer);

        foreach (var hit in hits)
        {
            var health = hit.GetComponent<HealthSystem>();
            if (health != null)
            {
                float damage = 40f * dashAttackDamageMultiplier;
                health.TakeDamage(damage, 0);
                Debug.Log($"[DashAttack] Counter Thrust hit {hit.name} for {damage}");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, plungeAoERadius);
    }
}
