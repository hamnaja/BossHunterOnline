using UnityEngine;
using System.Collections.Generic;

public class ItemDropSpawner : MonoBehaviour
{
    [Header("Drop Settings")]
    [SerializeField] private GameObject itemDropPrefab; // Cube สีทองชั่วคราว
    [SerializeField] private float dropRadius = 2f;
    [SerializeField] private float dropHeight = 1.5f;

    void OnEnable()
    {
        LootSystem.OnLootGenerated += SpawnDrops;
    }

    void OnDisable()
    {
        LootSystem.OnLootGenerated -= SpawnDrops;
    }

    void SpawnDrops(List<(int itemID, int amount)> loot)
    {
        if (itemDropPrefab == null)
        {
            Debug.LogWarning("[ItemDropSpawner] No prefab assigned!");
            return;
        }

        foreach (var (itemID, amount) in loot)
        {
            // สุ่มตำแหน่งรอบจุดที่บอสตาย
            Vector2 randomCircle = Random.insideUnitCircle * dropRadius;
            Vector3 spawnPos = transform.position + new Vector3(
                randomCircle.x, dropHeight, randomCircle.y);

            // Spawn
            var obj = Instantiate(itemDropPrefab, spawnPos, Quaternion.identity);

            // ดึงชื่อไอเทม
            var itemData = GameDatabase.Instance?.GetItemById(itemID);
            string name  = itemData != null ? itemData.itemName : $"Item #{itemID}";

            // ตั้งค่า DroppedItem component
            var dropped = obj.GetComponent<DroppedItem>();
            if (dropped != null)
                dropped.Initialize(itemID, amount, name);
        }
    }
}
