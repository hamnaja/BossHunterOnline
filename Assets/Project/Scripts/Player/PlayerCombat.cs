using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combo Settings")]
    [SerializeField] private float comboResetTime = 1.2f;
    [SerializeField] private int maxComboSteps = 3;

    [Header("Stamina Cost")]
    [SerializeField] private float lightAttackStaminaCost = 10f;
    [SerializeField] private float heavyAttackStaminaCost = 25f;

    // Components
    private PlayerAnimController animController;
    private StaminaSystem staminaSystem;

    // Combo state
    private int currentComboStep = 0;
    private float comboResetTimer = 0f;
    private bool isAttacking = false;
    private bool comboWindowOpen = false;

    // Events — ส่งสัญญาณให้ Hitbox เปิด/ปิด แทนการเรียกตรงๆ
    public static event System.Action<int> OnAttackStart;   // int = comboStep
    public static event System.Action OnAttackEnd;

    void Awake()
    {
        animController  = GetComponent<PlayerAnimController>();
        staminaSystem   = GetComponent<StaminaSystem>();
    }

    void Update()
    {
        TickComboTimer();
    }

    void TickComboTimer()
    {
        if (currentComboStep == 0) return;

        comboResetTimer -= Time.deltaTime;
        if (comboResetTimer <= 0f)
            ResetCombo();
    }

    // PlayerController จะเรียก method นี้เมื่อรับ input โจมตี
    public void OnLightAttack()
    {
        if (isAttacking && !comboWindowOpen) return;
        if (!staminaSystem.ConsumeStamina(lightAttackStaminaCost)) return;

        currentComboStep++;
        if (currentComboStep > maxComboSteps)
            currentComboStep = 1;

        comboResetTimer = comboResetTime;
        StartCoroutine(ExecuteAttack(currentComboStep));
    }

    public void OnHeavyAttack()
    {
        if (isAttacking) return;
        if (!staminaSystem.ConsumeStamina(heavyAttackStaminaCost)) return;

        ResetCombo();
        StartCoroutine(ExecuteAttack(-1)); // -1 = heavy attack
    }

    IEnumerator ExecuteAttack(int comboStep)
    {
        isAttacking = true;
        comboWindowOpen = false;

        // Startup frames — แจ้ง Animator
        animController?.TriggerAttack(comboStep);
        OnAttackStart?.Invoke(comboStep);

        // Startup duration (ปรับได้ตาม animation จริง)
        yield return new WaitForSeconds(0.1f);

        // Active hitbox frames — เปิด combo window ให้กดต่อได้
        comboWindowOpen = true;
        yield return new WaitForSeconds(0.3f);

        // Recovery frames
        comboWindowOpen = false;
        OnAttackEnd?.Invoke();

        yield return new WaitForSeconds(0.2f);
        isAttacking = false;
    }

    void ResetCombo()
    {
        currentComboStep = 0;
        comboResetTimer  = 0f;
    }

    public bool IsAttacking => isAttacking;
}