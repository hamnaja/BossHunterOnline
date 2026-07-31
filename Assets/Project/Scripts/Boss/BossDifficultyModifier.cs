using UnityEngine;

public enum DifficultyMode
{
    Normal,    // HP x1, Damage x1
    Hard,      // HP x2, Damage x1.5
    Nightmare, // HP x4, Damage x2
    Chaos      // HP x10, Damage x5
}

public class BossDifficultyModifier : MonoBehaviour
{
    [Header("Difficulty")]
    [SerializeField] private DifficultyMode difficulty = DifficultyMode.Normal;

    private HealthSystem healthSystem;

    // ค่าสเกลตาม GDD Section 3.4 และ Section 22
    private static readonly float[] hpMultipliers     = { 1f, 2f, 4f, 10f };
    private static readonly float[] damageMultipliers = { 1f, 1.5f, 2f, 5f };

    public float DamageMultiplier { get; private set; } = 1f;

    void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
    }

    void Start()
    {
        ApplyDifficulty();
    }

    void ApplyDifficulty()
    {
        int index = (int)difficulty;
        float hpMult     = hpMultipliers[index];
        float damageMult = damageMultipliers[index];

        // สเกล HP ตามความยาก
        healthSystem?.ScaleMaxHP(hpMult);

        // เก็บค่า damage multiplier ไว้ให้ BossAttackTrigger ดึงไปใช้
        DamageMultiplier = damageMult;

        Debug.Log($"[Difficulty] {difficulty} — HP x{hpMult}, Damage x{damageMult}");
    }

    public DifficultyMode CurrentDifficulty => difficulty;
}
