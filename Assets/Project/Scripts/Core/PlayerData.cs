using System;
using System.Collections.Generic;

[Serializable]
public class PlayerData
{
    public int gold = 0;
    public int gearScore = 0;
    public List<InventorySlot> inventory = new List<InventorySlot>();
    public SerializableDictionary<int, int> weaponMasteryLevels = new();
    public List<int> unlockedAchievements = new List<int>();
    public SerializableDictionary<int, int> codexDonations = new();
}