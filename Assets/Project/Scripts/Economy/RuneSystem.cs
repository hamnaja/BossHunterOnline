using UnityEngine;
using System.Collections.Generic;

// ตาม GDD Section 19 — ระบบฝังรูนเพิ่มสเตตัส
public enum RuneType { Crit, Attack, HP, Stamina, Fire, Ice, Lightning }
public enum StatModifierType { Flat, Percentage }

[CreateAssetMenu(fileName = "New Rune", menuName = "BossHunter/Rune Data")]
public class RuneData : ItemData
{
    public RuneType runeType;
    public StatModifierType modifierType;
    public float modifierValue;
    public int tier = 1; // 1-5
}

// ระบบ Socket — จัดการรูนที่ฝังในอุปกรณ์
[System.Serializable]
public class RuneSocket
{
    public int runeItemID = -1; // -1 = ว่าง
    public bool isEmpty => runeItemID == -1;
}

public class RuneSystem : MonoBehaviour
{
    public static RuneSystem Instance { get; private set; }

    // Rune Fusion ตาม GDD Section 8.1 — อัตราส่วน 3:1
    // Rune Tier N x3 + Gold → Rune Tier N+1
    private static readonly int[] fusionGoldCost = { 0, 5000, 25000, 100000, 500000 };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ฟิวส์รูน 3 ชิ้น → รูน tier สูงกว่า
    public bool TryFuseRunes(int runeItemID, int currentTier)
    {
        if (currentTier >= 5)
        {
            Debug.Log("[RuneSystem] Already max tier!");
            return false;
        }

        if (InventoryManager.Instance == null) return false;
        var inventory = InventoryManager.Instance;

        int goldCost = fusionGoldCost[currentTier];

        // เช็คว่ามีรูน 3 ชิ้นไหม
        if (!inventory.HasItem(runeItemID, 3))
        {
            Debug.Log($"[RuneSystem] Need 3x RuneID:{runeItemID} to fuse");
            return false;
        }

        // เช็คว่ามีทองพอไหม
        if (SaveSystem.Instance == null) return false;
        var saveSystem = SaveSystem.Instance;
        if (saveSystem.CurrentData.gold < goldCost)
        {
            Debug.Log($"[RuneSystem] Need {goldCost} gold to fuse");
            return false;
        }

        // หักวัตถุดิบ
        inventory.RemoveItem(runeItemID, 3);
        saveSystem.CurrentData.gold -= goldCost;

        // TODO: เพิ่มรูน tier สูงกว่า (ต้องมี ID ของรูน tier ถัดไป)
        Debug.Log($"[RuneSystem] Fused 3x RuneID:{runeItemID} → Tier {currentTier + 1} (cost {goldCost} gold)");
        return true;
    }

    // คำนวณ stat bonus จากรูนทั้งหมดที่ฝังอยู่
    public float CalculateTotalBonus(RuneType type, StatModifierType modType, List<RuneSocket> sockets)
    {
        float total = 0f;
        foreach (var socket in sockets)
        {
            if (socket.isEmpty) continue;
            var itemData = GameDatabase.Instance?.GetItemById(socket.runeItemID);
            if (itemData is RuneData rune && rune.runeType == type && rune.modifierType == modType)
                total += rune.modifierValue;
        }
        return total;
    }
}
