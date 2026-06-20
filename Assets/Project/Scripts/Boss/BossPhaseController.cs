using UnityEngine;

public class BossPhaseController : MonoBehaviour
{
    [Header("Phase Thresholds")]
    [SerializeField] private float phase2Threshold = 0.7f; // 70% HP
    [SerializeField] private float phase3Threshold = 0.3f; // 30% HP

    private BossController bossController;
    private HealthSystem healthSystem;
    private BossMotor motor;

    private int currentPhase = 1;
    private bool phase2Triggered = false;
    private bool phase3Triggered = false;

    void Awake()
    {
        bossController = GetComponent<BossController>();
        healthSystem   = GetComponent<HealthSystem>();
        motor          = GetComponent<BossMotor>();
    }

    void Start()
    {
        healthSystem.OnHealthChanged += CheckPhaseTransition;
    }

    void CheckPhaseTransition(float current, float max)
    {
        float ratio = current / max;

        // เฟส 2 — HP ลดเหลือ 70%
        if (!phase2Triggered && ratio <= phase2Threshold)
        {
            phase2Triggered = true;
            currentPhase = 2;
            OnEnterPhase2();
        }

        // เฟส 3 — HP ลดเหลือ 30% (RAGE MODE)
        if (!phase3Triggered && ratio <= phase3Threshold)
        {
            phase3Triggered = true;
            currentPhase = 3;
            OnEnterPhase3();
        }
    }

    void OnEnterPhase2()
    {
        Debug.Log("[BossPhase] Phase 2! Double Slam + Ground Shockwave unlocked");
        // เพิ่มความเร็ว 10%
        motor.SetSpeed(motor.BaseSpeed * 1.1f);
    }

    void OnEnterPhase3()
    {
        Debug.Log("[BossPhase] Phase 3! RAGE MODE — speed x1.25, Cataclysm unlocked");
        // Enrage — ความเร็วเพิ่ม 25% ตาม GDD Section 20
        motor.SetSpeed(motor.BaseSpeed * 1.25f);
        bossController.TriggerEnrage();
    }

    public int CurrentPhase => currentPhase;
}
