using UnityEngine;
using System.Collections.Generic;

public class LootSystem : MonoBehaviour
{
    [Header("Loot Table")]
    [SerializeField] private LootTableData lootTable;

    // Events
    public static event System.Action<List<(int itemID, int amount)>> OnLootGenerated;

    void Start()
    {
        // Subscribe ตอนบอสตาย
        var health = GetComponent<HealthSystem>();
        if (health != null)
            health.OnDeath += RollLoot;
    }

    void RollLoot()
    {
        if (lootTable == null)
        {
            Debug.LogWarning("[LootSystem] No loot table assigned!");
            return;
        }

        var results = new List<(int itemID, int amount)>();

        foreach (var drop in lootTable.drops)
        {
            float roll = Random.Range(0f, 1f);
            if (roll <= drop.dropChance)
            {
                int amount = Random.Range(drop.minAmount, drop.maxAmount + 1);
                results.Add((drop.itemID, amount));
                Debug.Log($"[LootSystem] Dropped ItemID:{drop.itemID} x{amount} (roll:{roll:F2} <= {drop.dropChance:F2})");
            }
        }

        if (results.Count == 0)
            Debug.Log("[LootSystem] No items dropped this run");

        OnLootGenerated?.Invoke(results);
    }
}
