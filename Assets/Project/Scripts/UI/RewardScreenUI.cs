using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RewardScreenUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI clearTimeText;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private Transform itemListContainer;
    [SerializeField] private GameObject itemRewardPrefab;

    private float combatStartTime;

    void OnEnable()
    {
        GameManager.OnGameStateChanged += OnStateChanged;
        LootSystem.OnLootGenerated += ShowRewards;
    }

    void OnDisable()
    {
        GameManager.OnGameStateChanged -= OnStateChanged;
        LootSystem.OnLootGenerated -= ShowRewards;
    }

    void OnStateChanged(GameState state)
    {
        if (state == GameState.ActiveCombat)
            combatStartTime = Time.time;
        else if (state == GameState.VictoryRound)
            ShowVictoryScreen();
    }

    void ShowVictoryScreen()
    {
        if (panelRoot != null)
            panelRoot.SetActive(true);

        float elapsed = Time.time - combatStartTime;
        int minutes = Mathf.FloorToInt(elapsed / 60f);
        int seconds = Mathf.FloorToInt(elapsed % 60f);

        if (clearTimeText != null)
            clearTimeText.text = $"Clear Time: {minutes:00}:{seconds:00}";

        string rank = elapsed < 60f ? "S" : elapsed < 120f ? "A" : "B";
        if (rankText != null)
        {
            rankText.text = $"Rank: {rank}";
            rankText.color = rank == "S" ? Color.yellow : rank == "A" ? Color.cyan : Color.white;
        }

        Debug.Log($"[RewardScreen] Clear Time: {minutes:00}:{seconds:00} | Rank: {rank}");
    }

    void ShowRewards(List<(int itemID, int amount)> loot)
    {
        foreach (var (itemID, amount) in loot)
        {
            var itemData = GameDatabase.Instance?.GetItemById(itemID);
            string itemName = itemData != null ? itemData.itemName : $"Item #{itemID}";
            Debug.Log($"[RewardScreen] + {itemName} x{amount}");

            if (itemRewardPrefab != null && itemListContainer != null)
            {
                var obj = Instantiate(itemRewardPrefab, itemListContainer);
                var text = obj.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                    text.text = $"{itemName} x{amount}";
            }
        }
    }
}
