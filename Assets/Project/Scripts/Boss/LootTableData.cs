using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LootDrop
{
    public int itemID;
    [Range(0f, 1f)] public float dropChance;
    public int minAmount = 1;
    public int maxAmount = 1;
}

[CreateAssetMenu(fileName = "New LootTable", menuName = "BossHunter/Loot Table")]
public class LootTableData : ScriptableObject
{
    public int lootTableID;
    public List<LootDrop> drops = new List<LootDrop>();
}
