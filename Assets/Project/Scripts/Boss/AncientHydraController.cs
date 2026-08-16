using UnityEngine;
using System.Collections;
using UnityEngine.AI;

// Boss #4: Ancient Hydra — ตาม GDD Section 20
// บอสขนาดใหญ่มี 3 หัวแยกธาตุ (ไฟ/น้ำแข็ง/พิษ)
// ต้องตัดหัวให้ครบก่อนจึงจะโดน HP หลักได้
public class AncientHydraController : MonoBehaviour
{
    [Header("Boss Data")]
    [SerializeField] private BossData bossData;
    [SerializeField] private BossHealthBarUI healthBarUI;

    [Header("Hydra Settings")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 5f;

    [Header("Head HP — ต้องกำจัดทั้ง 3 หัวก่อน")]
    [SerializeField] private float fireHeadHP   = 50f;
    [SerializeField] private float iceHeadHP    = 50f;
    [SerializeField] private float poisonHeadHP = 50f;

    // State
    private bool fireHeadAlive   = true;
    private bool iceHeadAlive    = true;
    private bool poisonHeadAlive = true;
    private bool coreVulnerable  = false;

    private BossState currentState = BossState.Spawn;
    private HealthSystem healthSystem;
    private NavMeshAgent agent;
    private Animator anim;
    private Transform playerTransform;
    private bool aggroTriggered = false;
    private float attackCooldown = 0f;
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

        // บล็อก HP หลักจนกว่าจะตัดหัวครบ
        healthSystem.isInvulnerable = true;
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
                    StartCoroutine(MultiHeadAttack());
                    attackCooldown = attackInterval;
                    ChangeState(BossState.Chase);
                }
                break;
        }
    }

    // โจมตีจากหัวที่ยังเหลืออยู่
    IEnumerator MultiHeadAttack()
    {
        if (fireHeadAlive)
        {
            Debug.Log("[Hydra] Fire Head Attack!");
            DealDamageToPlayer(25f);
        }
        yield return new WaitForSeconds(0.5f);

        if (iceHeadAlive)
        {
            Debug.Log("[Hydra] Ice Head Attack! (Slow)");
            DealDamageToPlayer(20f);
            // TODO: apply slow debuff
        }
        yield return new WaitForSeconds(0.5f);

        if (poisonHeadAlive)
        {
            Debug.Log("[Hydra] Poison Head Attack! (DoT)");
            DealDamageToPlayer(15f);
            // TODO: apply poison DoT
        }

        if (agent != null) agent.isStopped = false;
    }

    void DealDamageToPlayer(float dmg)
    {
        if (playerTransform == null) return;
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist > attackRange * 1.5f) return;

        var health = playerTransform.GetComponent<HealthSystem>();
        health?.TakeDamage(dmg, bossData?.bossID ?? 4);
    }

    // เรียกจากภายนอกเมื่อโจมตีหัวแต่ละหัว
    public void DamageFireHead(float dmg)
    {
        if (!fireHeadAlive) return;
        fireHeadHP -= dmg;
        Debug.Log($"[Hydra] Fire Head HP: {fireHeadHP}");
        if (fireHeadHP <= 0) { fireHeadAlive = false; Debug.Log("[Hydra] Fire Head destroyed!"); }
        CheckAllHeadsDestroyed();
    }

    public void DamageIceHead(float dmg)
    {
        if (!iceHeadAlive) return;
        iceHeadHP -= dmg;
        Debug.Log($"[Hydra] Ice Head HP: {iceHeadHP}");
        if (iceHeadHP <= 0) { iceHeadAlive = false; Debug.Log("[Hydra] Ice Head destroyed!"); }
        CheckAllHeadsDestroyed();
    }

    public void DamagePoisonHead(float dmg)
    {
        if (!poisonHeadAlive) return;
        poisonHeadHP -= dmg;
        Debug.Log($"[Hydra] Poison Head HP: {poisonHeadHP}");
        if (poisonHeadHP <= 0) { poisonHeadAlive = false; Debug.Log("[Hydra] Poison Head destroyed!"); }
        CheckAllHeadsDestroyed();
    }

    void CheckAllHeadsDestroyed()
    {
        if (!fireHeadAlive && !iceHeadAlive && !poisonHeadAlive)
        {
            coreVulnerable = true;
            healthSystem.isInvulnerable = false;
            Debug.Log("[Hydra] All heads destroyed! Core is now vulnerable!");
        }
    }

    void TriggerAggro()
    {
        if (aggroTriggered) return;
        aggroTriggered = true;
        string name = bossData != null ? bossData.bossName : "Ancient Hydra";
        healthBarUI?.ShowAndBind(healthSystem, name);
    }

    public void ChangeState(BossState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        Debug.Log($"[Hydra] State -> {newState}");
    }

    void OnDeath()
    {
        ChangeState(BossState.Dead);
        if (agent != null) agent.isStopped = true;
        Debug.Log("[Hydra] Defeated!");
        GameManager.Instance?.PlayerWon();
    }
}
