using System;

[Serializable]
public class InventorySlot
{
    public int itemID;
    public int amount;

    public InventorySlot(int id, int amt)
    {
        itemID = id;
        amount = amt;
    }
}