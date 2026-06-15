using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHP = 100f;

    private float currentHP;
    public bool isInvulnerable = false;

    // Events — ส่งสัญญาณออกไป ห้าม class นี้เรียก UI หรือ VFX ตรงๆ
    public event System.Action<float, float> OnHealthChanged; // current, max
    public event System.Action OnDeath;

    void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(float damage, int attackerID = 0)
    {
        if (isInvulnerable) return;
        if (currentHP <= 0f) return;

        currentHP = Mathf.Max(currentHP - damage, 0f);
        OnHealthChanged?.Invoke(currentHP, maxHP);

        if (currentHP <= 0f)
            OnDeath?.Invoke();
    }

    public void Heal(float amount)
    {
        if (currentHP <= 0f) return;

        currentHP = Mathf.Min(currentHP + amount, maxHP);
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    public float CurrentHP => currentHP;
    public float MaxHP => maxHP;
    public bool IsDead => currentHP <= 0f;
}
