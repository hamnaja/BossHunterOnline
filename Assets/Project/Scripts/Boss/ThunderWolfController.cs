using UnityEngine;
using System.Collections;
using UnityEngine.AI;

// Boss #3: Thunder Wolf — ตาม GDD Section 20
// ความเร็วสูงมาก วิ่งวนล่อตาก่อนพุ่งเข้ากัด
// มีระบบสะสมพลังสายฟ้า เมื่อชาร์จเต็มจะระเบิดรอบตัว
public class ThunderWolfController : MonoBehaviour
{
    [Header("Boss Data")]
    [SerializeField] private BossData bossData;
    [SerializeField] private BossHealthBarUI healthBarUI;

    [Header("Thunder Wolf Settings")]
    [SerializeField] private float detectionRange = 18f;
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float circleRadius = 8f;
    [SerializeField] private float circleSpeed = 6f;
    [SerializeField] private float dashSpeed = 20f;

    [Header("Thunder Charge")]
    [SerializeField] private float maxThunderCharge = 100f;
    [SerializeField] private float chargePerHit = 25f;
    [SerializeField] private float thunderExplosionRadius = 5f;
    [SerializeField] private float thunderExplosionDamage = 60f;

    // Components
    private HealthSystem healthSystem;
    private NavMeshAgent agent;
    private Animator anim;

    // State
    private BossState currentState = BossState.Spawn;
    private Transform playerTransform;
    private bool aggroTriggered = false;
    private float attackCooldown = 0f;
    private float thunderCharge = 0f;
    private float circleAngle = 0f;
    private bool isCircling = false;

    [SerializeField] private float attackInterval = 3f;

    void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        agent        = GetComponent<NavMeshAgent>();
        anim         = GetComponent<Animator>();
    }

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        healthSystem.OnDeath += OnDeath;
        ChangeState(BossState.Spawn);
    }

    void Update()
    {
        if (currentState == BossState.Dead) return;
        attackCooldown -= Time.deltaTime;
        RunFSM();
    }

    void RunFSM()
    {
        if (playerTransform == null) return;
        float dist = Vector3.Distance(transform.position, playerTransform.position);

        switch (currentState)
        {
            case BossState.Spawn:
                ChangeState(BossState.Idle);
                break;

            case BossState.Idle:
                if (dist <= detectionRange)
                {
                    TriggerAggro();
                    ChangeState(BossState.Chase);
                }
                break;

            case BossState.Chase:
                // วิ่งวนรอบผู้เล่นก่อน (ล่อตา)
                CircleAroundPlayer();
                if (attackCooldown <= 0f)
                    ChangeState(BossState.Attack);
                break;

            case BossState.Attack:
                StartCoroutine(DashAttack());
                attackCooldown = attackInterval;
                ChangeState(BossState.Chase);
                break;

            case BossState.Enrage:
                // Phase 2: วิ่งเร็วขึ้น โจมตีถี่ขึ้น
                agent.speed = circleSpeed * 1.5f;
                CircleAroundPlayer();
                if (attackCooldown <= 0f)
                {
                    StartCoroutine(DashAttack());
                    attackCooldown = attackInterval * 0.5f;
                }
                break;
        }
    }

    // วิ่งวนรอบผู้เล่นเพื่อล่อตา ตาม GDD Section 20
    void CircleAroundPlayer()
    {
        circleAngle += circleSpeed * Time.deltaTime;
        float x = Mathf.Cos(circleAngle) * circleRadius;
        float z = Mathf.Sin(circleAngle) * circleRadius;
        Vector3 targetPos = playerTransform.position + new Vector3(x, 0, z);
        agent?.SetDestination(targetPos);

        // หันหน้าเข้าหาผู้เล่นเสมอ
        Vector3 dir = (playerTransform.position - transform.position).normalized;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    // พุ่งเข้ากัดอย่างรวดเร็ว
    IEnumerator DashAttack()
    {
        anim?.SetTrigger("Dash");
        Debug.Log("[ThunderWolf] Dash Attack!");

        agent.isStopped = true;
        Vector3 dashDir = (playerTransform.position - transform.position).normalized;
        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            transform.position += dashDir * dashSpeed * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // ตรวจจับผู้เล่น
        Collider[] hits = Physics.OverlapSphere(transform.position, 1.5f);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;
            hit.GetComponent<HealthSystem>()?.TakeDamage(35f, bossData?.bossID ?? 3);
            Debug.Log("[ThunderWolf] Dash hit player!");

            // สะสมพลังสายฟ้าเมื่อโจมตีโดน
            thunderCharge += chargePerHit;
            Debug.Log($"[ThunderWolf] Thunder Charge: {thunderCharge}/{maxThunderCharge}");

            if (thunderCharge >= maxThunderCharge)
                StartCoroutine(ThunderExplosion());
        }

        agent.isStopped = false;
    }

    // ระเบิดสายฟ้ารอบตัวเมื่อชาร์จเต็ม ตาม GDD Section 20
    IEnumerator ThunderExplosion()
    {
        thunderCharge = 0f;
        anim?.SetTrigger("Thunder");
        Debug.Log("[ThunderWolf] THUNDER EXPLOSION!");

        Collider[] hits = Physics.OverlapSphere(transform.position, thunderExplosionRadius);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;
            hit.GetComponent<HealthSystem>()?.TakeDamage(thunderExplosionDamage, bossData?.bossID ?? 3);
            Debug.Log("[ThunderWolf] Thunder explosion hit player!");
        }

        yield return new WaitForSeconds(1f);
    }

    void TriggerAggro()
    {
        if (aggroTriggered) return;
        aggroTriggered = true;
        string name = bossData != null ? bossData.bossName : "Thunder Wolf";
        healthBarUI?.ShowAndBind(healthSystem, name);
    }

    public void ChangeState(BossState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        Debug.Log($"[ThunderWolf] State -> {newState}");
    }

    public void TriggerEnrage() => ChangeState(BossState.Enrage);

    void OnDeath()
    {
        ChangeState(BossState.Dead);
        if (agent != null) agent.isStopped = true;
        Debug.Log("[ThunderWolf] Defeated!");
        GameManager.Instance?.PlayerWon();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, thunderExplosionRadius);
        Gizmos.color = Color.cyan;
        if (playerTransform != null)
            Gizmos.DrawWireSphere(playerTransform.position, circleRadius);
    }
}