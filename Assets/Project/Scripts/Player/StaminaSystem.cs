using UnityEngine;

public class StaminaSystem : MonoBehaviour
{
    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float recoveryRate = 20f;      // หน่วยต่อวินาที
    [SerializeField] private float recoveryDelay = 1.0f;    // รอกี่วินาทีก่อนฟื้น

    private float currentStamina;
    private float delayTimer = 0f;

    public event System.Action<float, float> OnStaminaChanged; // current, max

    void Awake()
    {
        currentStamina = maxStamina;
    }

    void Update()
    {
        RecoverStamina();
    }

    void RecoverStamina()
    {
        if (delayTimer > 0f)
        {
            delayTimer -= Time.deltaTime;
            return;
        }

        if (currentStamina >= maxStamina) return;

        currentStamina = Mathf.Min(currentStamina + recoveryRate * Time.deltaTime, maxStamina);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    // คืนค่า true ถ้า stamina พอ และหักออก, false ถ้าไม่พอ
    public bool ConsumeStamina(float amount)
    {
        if (currentStamina < amount) return false;

        currentStamina -= amount;
        delayTimer = recoveryDelay;
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        return true;
    }

    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;
}