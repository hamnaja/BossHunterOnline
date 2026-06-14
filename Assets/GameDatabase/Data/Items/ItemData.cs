using UnityEngine;

public enum ItemRarity { Common, Rare, Epic, Legendary, Mythic }

[CreateAssetMenu(fileName = "New Item", menuName = "BossHunter/Item Data")]
public class ItemData : ScriptableObject
{
    public int itemID;
    public string itemName;
    [TextArea] public string description;
    public ItemRarity rarity;
    public int maxStack = 99;
    public Sprite icon;
}