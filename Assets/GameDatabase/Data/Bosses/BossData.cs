using UnityEngine;

[CreateAssetMenu(fileName = "New Boss", menuName = "BossHunter/Boss Data")]
public class BossData : ScriptableObject
{
    public int bossID;
    public string bossName;
    public float baseHP;
    public float baseDamage;
    public float baseDefense;
    public float[] phaseThresholds; // เช่น {0.7f, 0.3f} = เปลี่ยนเฟสที่ 70% และ 30%
    public int lootTableID;
}