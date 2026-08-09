using UnityEngine;
using System.Collections;
using UnityEngine.AI;

// Boss #2: Fire Dragon — ตาม GDD Section 20
// เน้นการต่อสู้กลางอากาศ บินโฉบพ่นไฟเป็นแนวยาว
public class FireDragonController : MonoBehaviour
{
    [Header("Boss Data")]
    [SerializeField] private BossData bossData;
    [SerializeField] private BossHealthBarUI healthBarUI;

    [Header("Fire Dragon Settings")]
    [SerializeField] private float detectionRange = 20f;
    [SerializeField] private float attackRange = 8f;
    [SerializeField] private float flyHeight = 5f;
    [SerializeField] private float swoopSpeed = 12f;

    [Header("AoE Burn Zone")]
    [SerializeField] private float burnZoneRadius = 4f;
    [SerializeField] private float burnDamagePerSecond = 10f;
    [SerializeField] private float burnDuration = 3f;

    // Components
    private HealthSystem healthSystem;
    private BossPhaseController phaseController;
    private NavMeshAgent agent;
    private Animator anim;

    // State
    private BossState currentState = BossState.Spawn;
    private Transform playerTransform;
    private bool aggroTriggered = false;
    private float attackCooldown = 0f;
    [SerializeField] private float attackInterval = 4f;

    void Awake()
    {
        healthSystem    = GetComponent<HealthSystem>();
        phaseController = GetComponent<BossPhaseController>();
        agent           = GetComponent<NavMeshAgent>();
        anim            = GetComponent<Animator>();
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
                agent?.SetDestination(playerTransform.position);
                if (dist <= attackRange && attackCooldown <= 0f)
                    ChangeState(BossState.Attack);
                break;

            case BossState.Attack:
                if (agent != null) agent.isStopped = true;
                if (attackCooldown <= 0f)
                {
                    StartCoroutine(FireBreathAttack());
                    attackCooldown = attackInterval;
                    ChangeState(BossState.Chase);
                }
                break;

            case BossState.Enrage:
                // Phase 2: โจมตีเร็วขึ้น เพิ่ม Swoop attack
                agent?.SetDestination(playerTransform.position);
                if (dist <= attackRange && attackCooldown <= 0f)
                {
                    StartCoroutine(SwoopAttack());
                    attackCooldown = attackInterval * 0.6f;
                }
                break;
        }
    }

    // ท่าโจมตีหลัก — พ่นไฟเป็นแนวยาว AoE Burn Zone
    IEnumerator FireBreathAttack()
    {
        anim?.SetTrigger("FireBreath");
        Debug.Log("[FireDragon] Fire Breath!");

        // สร้าง Burn Zone ตรงหน้าบอส
        Vector3 burnPos = transform.position + transform.forward * 3f;
        StartCoroutine(BurnZone(burnPos));

        yield return new WaitForSeconds(1f);
        agent.isStopped = false;
    }

    // ท่า Phase 2 — บินโฉบลงพุ่งเข้าหาผู้เล่น
    IEnumerator SwoopAttack()
    {
        anim?.SetTrigger("Swoop");
        Debug.Log("[FireDragon] Swoop Attack!");

        Vector3 targetPos = playerTransform.position;
        float elapsed = 0f;
        float duration = 0.5f;
        Vector3 startPos = transform.position;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // ตรวจจับผู้เล่นเมื่อพุ่งถึง
        Collider[] hits = Physics.OverlapSphere(transform.position, 2f);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;
            hit.GetComponent<HealthSystem>()?.TakeDamage(40f, bossData?.bossID ?? 2);
            Debug.Log("[FireDragon] Swoop hit player!");
        }
    }

    // Burn Zone — พื้นที่ไฟลุกต่อเนื่อง
    IEnumerator BurnZone(Vector3 position)
    {
        float elapsed = 0f;
        Debug.Log($"[FireDragon] Burn Zone active at {position}");

        while (elapsed < burnDuration)
        {
            // ตรวจจับผู้เล่นในพื้นที่
            Collider[] hits = Physics.OverlapSphere(position, burnZoneRadius);
            foreach (var hit in hits)
            {
                if (!hit.CompareTag("Player")) continue;
                float dmg = burnDamagePerSecond * Time.deltaTime;
                hit.GetComponent<HealthSystem>()?.TakeDamage(dmg, bossData?.bossID ?? 2);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Debug.Log("[FireDragon] Burn Zone expired");
    }

    void TriggerAggro()
    {
        if (aggroTriggered) return;
        aggroTriggered = true;
        string name = bossData != null ? bossData.bossName : "Fire Dragon";
        healthBarUI?.ShowAndBind(healthSystem, name);
    }

    public void ChangeState(BossState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        Debug.Log($"[FireDragon] State -> {newState}");
    }

    public void TriggerEnrage() => ChangeState(BossState.Enrage);

    void OnDeath()
    {
        ChangeState(BossState.Dead);
        if (agent != null) agent.isStopped = true;
        Debug.Log("[FireDragon] Defeated!");
        GameManager.Instance?.PlayerWon();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * 3f, burnZoneRadius);
    }
}