using UnityEngine;
using UnityEngine.UI;

public class InGameHUD : MonoBehaviour
{
    [Header("Player Bars")]
    [SerializeField] private Slider hpBar;
    [SerializeField] private Slider staminaBar;

    [Header("References")]
    [SerializeField] private HealthSystem playerHealth;
    [SerializeField] private StaminaSystem playerStamina;

    void Start()
    {
        // Subscribe to events แทนการเช็คค่าทุกเฟรม (Section 2.1 Data Isolation)
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateHPBar;
            UpdateHPBar(playerHealth.CurrentHP, playerHealth.MaxHP);
        }

        if (playerStamina != null)
        {
            playerStamina.OnStaminaChanged += UpdateStaminaBar;
            UpdateStaminaBar(playerStamina.CurrentStamina, playerStamina.MaxStamina);
        }
    }

    void UpdateHPBar(float current, float max)
    {
        if (hpBar == null) return;
        hpBar.maxValue = max;
        hpBar.value = current;
    }

    void UpdateStaminaBar(float current, float max)
    {
        if (staminaBar == null) return;
        staminaBar.maxValue = max;
        staminaBar.value = current;
    }

    void OnDestroy()
    {
        // Unsubscribe ป้องกัน memory leak
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHPBar;
        if (playerStamina != null)
            playerStamina.OnStaminaChanged -= UpdateStaminaBar;
    }
}
