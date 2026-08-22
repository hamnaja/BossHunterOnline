using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    [SerializeField] private float baseDamage = 30f;
    [SerializeField] private float attackRadius = 2.5f;
    [SerializeField] private LayerMask bossLayer;

    private bool isActive = false;
    private Transform playerTransform;

    void Awake()
    {
        playerTransform = transform.root;
    }

    public void EnableHitbox()
    {
        isActive = true;
        Debug.Log("[PlayerAttackHitbox] Hitbox ENABLED");
        CheckHit(); // เช็คทันทีที่เปิด
    }

    public void DisableHitbox()
    {
        isActive = false;
        Debug.Log("[PlayerAttackHitbox] Hitbox DISABLED");
    }

    void CheckHit()
    {
        // ตรวจจับบอสในรัศมีรอบผู้เล่น
        Vector3 origin = playerTransform.position + playerTransform.forward * 1.5f;
        Collider[] hits = Physics.OverlapSphere(origin, attackRadius);

        Debug.Log($"[PlayerAttackHitbox] Checking hits at {origin}, found {hits.Length} colliders");

        foreach (var hit in hits)
        {
            Debug.Log($"[PlayerAttackHitbox] Found: {hit.name} tag:{hit.tag}");

            if (hit.transform.root == playerTransform) continue; // ข้ามตัวเอง

            var health = hit.GetComponent<HealthSystem>();
            if (health == null)
                health = hit.transform.root.GetComponent<HealthSystem>();

            if (health != null)
            {
                health.TakeDamage(baseDamage, 0);
                Debug.Log($"[PlayerAttack] Hit {hit.name} for {baseDamage}");
                return;
                FloatingDamagePool.Instance?.SpawnDamageNumber(
                hit.transform.position + Vector3.up * 2f,
                baseDamage,
                isCrit: false,
                isElement: false);
            }
            
        }
    }

    void OnDrawGizmosSelected()
    {
        if (playerTransform == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            playerTransform.position + playerTransform.forward * 1.5f, attackRadius);
    }
}