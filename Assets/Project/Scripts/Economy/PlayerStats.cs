using UnityEngine;
using System.Collections.Generic;

// ระบบสเตตัสผู้เล่นที่รวม base stats + rune bonuses
// ตาม GDD Section 16.2 — Combat Statistics & Damage Formula
public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("Base Stats")]
    [SerializeField] private float baseAttack = 100f;
    [SerializeField] private float baseDefense = 50f;
    [SerializeField] private float baseCritRate = 5f;      // %
    [SerializeField] private float baseCritDamage = 150f;  // % ตาม GDD
    [SerializeField] private float baseBossDamageModifier = 0f; // %

    // Rune Sockets — อาวุธ 3, เกราะ 2, หมวก 1 ตาม GDD Section 19.1
    [Header("Rune Sockets")]
    public List<RuneSocket> weaponSockets = new List<RuneSocket> { new(), new(), new() };
    public List<RuneSocket> armorSockets  = new List<RuneSocket> { new(), new() };
    public List<RuneSocket> helmetSockets = new List<RuneSocket> { new() };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ---- สูตรคำนวณดาเมจตาม GDD Section 16.2 ----

    // Base Damage = (Attack × Skill Multiplier) - Enemy Defense
    public float CalculateBaseDamage(float skillMultiplier, float enemyDefense)
    {
        float atk = GetTotalAttack();
        float damage = (atk * skillMultiplier) - enemyDefense;
        return Mathf.Max(damage, 1f); // ดาเมจขั้นต่ำ = 1
    }

    // Final Damage = Base Damage × (Critical Multiplier / 100)
    public float CalculateCritDamage(float baseDamage)
    {
        float critMult = GetTotalCritDamage();
        return baseDamage * (critMult / 100f);
    }

    // Total Damage = [Final Damage + Element Damage] × (1 + Boss Damage Modifier)
    public float CalculateTotalDamage(float finalDamage, float elementDamage)
    {
        float bossMod = 1f + (baseBossDamageModifier / 100f);
        return (finalDamage + elementDamage) * bossMod;
    }

    // เช็คว่า crit ไหม
    public bool RollCrit()
    {
        float critRate = GetTotalCritRate();
        return Random.Range(0f, 100f) <= critRate;
    }

    // ---- Getters รวม base + rune bonus ----

    public float GetTotalAttack()
    {
        var runeSystem = RuneSystem.Instance;
        if (runeSystem == null) return baseAttack;

        float runeBonus = runeSystem.CalculateTotalBonus(
            RuneType.Attack, StatModifierType.Flat,
            GetAllSockets());

        return baseAttack + runeBonus;
    }

    public float GetTotalCritRate()
    {
        var runeSystem = RuneSystem.Instance;
        if (runeSystem == null) return baseCritRate;

        float runeBonus = runeSystem.CalculateTotalBonus(
            RuneType.Crit, StatModifierType.Percentage,
            GetAllSockets());

        return baseCritRate + runeBonus;
    }

    public float GetTotalCritDamage()
    {
        var runeSystem = RuneSystem.Instance;
        if (runeSystem == null) return baseCritDamage;

        float runeBonus = runeSystem.CalculateTotalBonus(
            RuneType.Crit, StatModifierType.Flat,
            GetAllSockets());

        return baseCritDamage + runeBonus;
    }

    List<RuneSocket> GetAllSockets()
    {
        var all = new List<RuneSocket>();
        all.AddRange(weaponSockets);
        all.AddRange(armorSockets);
        all.AddRange(helmetSockets);
        return all;
    }

    void OnGUI()
    {
        // Debug display
        if (!Application.isEditor) return;
        GUI.Label(new Rect(10, 10, 300, 20), $"ATK: {GetTotalAttack():F0} | Crit: {GetTotalCritRate():F1}% | CritDmg: {GetTotalCritDamage():F0}%");
    }
}
