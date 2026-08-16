using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private int maxSlots = 100;
    [SerializeField] private int maxStack = 99;

    private List<InventorySlot> slots = new List<InventorySlot>();

    public static event System.Action OnInventoryChanged;
    public static event System.Action OnInventoryFull;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        DroppedItem.OnItemPickedUp += (id, amount) => AddItem(id, amount);
    }

    public bool AddItem(int itemID, int amount)
    {
        foreach (var slot in slots)
        {
            if (slot.itemID == itemID && slot.amount < maxStack)
            {
                int canAdd = maxStack - slot.amount;
                int adding = Mathf.Min(canAdd, amount);
                slot.amount += adding;
                amount -= adding;
                if (amount <= 0) { OnInventoryChanged?.Invoke(); return true; }
            }
        }

        while (amount > 0)
        {
            if (slots.Count >= maxSlots) { OnInventoryFull?.Invoke(); return false; }
            int adding = Mathf.Min(maxStack, amount);
            slots.Add(new InventorySlot(itemID, adding));
            amount -= adding;
            Debug.Log($"[Inventory] New slot: ID={itemID} x{adding}");
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool RemoveItem(int itemID, int amount)
    {
        if (!HasItem(itemID, amount)) return false;
        int remaining = amount;
        for (int i = slots.Count - 1; i >= 0 && remaining > 0; i--)
        {
            if (slots[i].itemID != itemID) continue;
            int removing = Mathf.Min(slots[i].amount, remaining);
            slots[i].amount -= removing;
            remaining -= removing;
            if (slots[i].amount <= 0) slots.RemoveAt(i);
        }
        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool HasItem(int itemID, int amount = 1)
    {
        int total = 0;
        foreach (var slot in slots)
            if (slot.itemID == itemID) total += slot.amount;
        return total >= amount;
    }

    public int GetItemCount(int itemID)
    {
        int total = 0;
        foreach (var slot in slots)
            if (slot.itemID == itemID) total += slot.amount;
        return total;
    }

    public List<InventorySlot> GetAllSlots() => slots;

    public void LoadFromPlayerData(PlayerData data)
    {
        slots = new List<InventorySlot>(data.inventory);
        OnInventoryChanged?.Invoke();
    }

    public void SaveToPlayerData(PlayerData data)
    {
        data.inventory = new List<InventorySlot>(slots);
    }
}
