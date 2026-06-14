using UnityEngine;
using System.Collections.Generic;

public class GameDatabase : MonoBehaviour
{
    public static GameDatabase Instance { get; private set; }

    // ตารางข้อมูลทั้งหมด โหลดครั้งเดียวตอนเปิดเกม
    private Dictionary<int, ItemData> itemTable = new();
    private Dictionary<int, WeaponData> weaponTable = new();
    private Dictionary<int, BossData> bossTable = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAllData();
    }

    void LoadAllData()
    {
        // โหลด ScriptableObjects ทั้งหมดจากโฟลเดอร์ Resources
        var items = Resources.LoadAll<ItemData>("Data/Items");
        foreach (var item in items)
            itemTable[item.itemID] = item;

        var weapons = Resources.LoadAll<WeaponData>("Data/Weapons");
        foreach (var weapon in weapons)
            weaponTable[weapon.itemID] = weapon;

        var bosses = Resources.LoadAll<BossData>("Data/Bosses");
        foreach (var boss in bosses)
            bossTable[boss.bossID] = boss;

        Debug.Log($"[GameDatabase] Loaded: {itemTable.Count} items, {weaponTable.Count} weapons, {bossTable.Count} bosses");
    }

    // Public API — class อื่นเรียกผ่านนี้เท่านั้น ห้าม access table ตรงๆ
    public ItemData GetItemById(int id)
    {
        itemTable.TryGetValue(id, out var data);
        return data;
    }

    public WeaponData GetWeaponById(int id)
    {
        weaponTable.TryGetValue(id, out var data);
        return data;
    }

    public BossData GetBossById(int id)
    {
        bossTable.TryGetValue(id, out var data);
        return data;
    }
}