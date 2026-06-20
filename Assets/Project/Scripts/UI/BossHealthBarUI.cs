using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBarUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject panelRoot;   // ทั้งแถบ ซ่อน/โชว์ตาม Aggro
    [SerializeField] private Slider hpBar;
    [SerializeField] private TextMeshProUGUI bossNameText;
    [SerializeField] private RectTransform[] phaseMarkers; // เส้นแบ่งเฟสบนแถบเลือด

    private HealthSystem targetBossHealth;
    private bool isVisible = false;

    void Start()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false); // ซ่อนไว้ก่อนจนกว่าจะ Aggro
    }

    // เรียกจาก BossController ตอนเข้า Chase/Attack state ครั้งแรก
    public void ShowAndBind(HealthSystem bossHealth, string bossName)
    {
        if (isVisible) return;

        targetBossHealth = bossHealth;
        targetBossHealth.OnHealthChanged += UpdateBar;
        targetBossHealth.OnDeath += HideBar;

        if (bossNameText != null)
            bossNameText.text = bossName;

        UpdateBar(bossHealth.CurrentHP, bossHealth.MaxHP);

        if (panelRoot != null)
            panelRoot.SetActive(true);

        isVisible = true;
    }

    void UpdateBar(float current, float max)
    {
        if (hpBar == null) return;
        hpBar.maxValue = max;
        hpBar.value = current;
    }

    void HideBar()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (targetBossHealth != null)
        {
            targetBossHealth.OnHealthChanged -= UpdateBar;
            targetBossHealth.OnDeath -= HideBar;
        }

        isVisible = false;
    }
}
