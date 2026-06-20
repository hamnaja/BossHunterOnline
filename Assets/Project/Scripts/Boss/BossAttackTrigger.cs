using UnityEngine;

// ติดตั้งบน Collider ที่อาวุธหรืออุ้งเท้าบอส
// เปิด/ปิดผ่าน Animation Event เท่านั้น ตาม GDD Section 3.4
public class BossAttackTrigger : MonoBehaviour
{
    [SerializeField] private float damageAmount = 20f;
    [SerializeField] private int bossID = 1;

    private Collider col;

    void Awake()
    {
        col = GetComponent<Collider>();
        col.enabled = false; // ปิดไว้ก่อนเสมอ
        col.isTrigger = true;
    }

    // Animation Event เรียกตอนเริ่ม Active Hitbox Frame
    public void EnableHitbox()
    {
        col.enabled = true;
        DodgeRoll.BossIsAttacking = true;
    }

    // Animation Event เรียกตอนจบ Active Hitbox Frame
    public void DisableHitbox()
    {
        col.enabled = false;
        DodgeRoll.BossIsAttacking = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var health = other.GetComponent<HealthSystem>();
        if (health == null) return;

        health.TakeDamage(damageAmount, bossID);
        Debug.Log($"[BossAttack] Hit Player for {damageAmount} damage");

        // ปิด hitbox ทันทีหลังโดน ป้องกันโดนซ้ำ
        DisableHitbox();
    }
}
