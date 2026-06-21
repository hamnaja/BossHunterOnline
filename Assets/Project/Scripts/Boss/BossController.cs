using UnityEngine;
using UnityEngine.AI;

public enum BossState
{
    Spawn,
    Idle,
    Chase,
    Attack,
    Enrage,
    Dead
}

[RequireComponent(typeof(NavMeshAgent))]
public class BossController : MonoBehaviour
{
    [Header("Boss Data")]
    [SerializeField] private BossData bossData;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 3f;

    [Header("UI")]
    [SerializeField] private BossHealthBarUI healthBarUI;

    // Components
    private BossMotor motor;
    private BossPhaseController phaseController;
    private HealthSystem healthSystem;
    private Animator anim;

    // FSM State
    private BossState currentState = BossState.Spawn;
    private Transform playerTransform;
    private bool aggroTriggered = false;

    // Attack cooldown
    private float attackCooldown = 0f;
    [SerializeField] private float attackInterval = 2f;

    void Awake()
    {
        motor           = GetComponent<BossMotor>();
        phaseController = GetComponent<BossPhaseController>();
        healthSystem    = GetComponent<HealthSystem>();
        anim            = GetComponent<Animator>();
    }

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        healthSystem.OnDeath += OnBossDeath;
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

        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        switch (currentState)
        {
            case BossState.Spawn:
                ChangeState(BossState.Idle);
                break;

            case BossState.Idle:
                if (distToPlayer <= detectionRange)
                {
                    TriggerAggro();
                    ChangeState(BossState.Chase);
                }
                break;

            case BossState.Chase:
                motor.MoveTo(playerTransform.position);
                if (distToPlayer <= attackRange && attackCooldown <= 0f)
                    ChangeState(BossState.Attack);
                break;

            case BossState.Attack:
                motor.Stop();
                if (attackCooldown <= 0f)
                {
                    PerformAttack();
                    attackCooldown = attackInterval;
                    ChangeState(BossState.Chase);
                }
                break;

            case BossState.Enrage:
                motor.MoveTo(playerTransform.position);
                if (distToPlayer <= attackRange && attackCooldown <= 0f)
                {
                    PerformAttack();
                    attackCooldown = attackInterval * 0.75f;
                }
                break;
        }
    }

    // เรียกครั้งแรกที่บอสเห็นผู้เล่น — โชว์ Boss Health Bar
    void TriggerAggro()
    {
        if (aggroTriggered) return;
        aggroTriggered = true;

        if (healthBarUI != null)
        {
            string name = bossData != null ? bossData.bossName : "Boss";
            healthBarUI.ShowAndBind(healthSystem, name);
        }
    }

    void PerformAttack()
    {
        anim?.SetTrigger("Attack");
        Debug.Log($"[BossController] {bossData?.bossName} attacks!");
        StartCoroutine(SimulateAttackHitbox());
    }

    System.Collections.IEnumerator SimulateAttackHitbox()
    {
        // ชั่วคราว — จำลอง Animation Event จนกว่าจะมี animation จริง
        yield return new WaitForSeconds(0.3f);
        var hitbox = GetComponentInChildren<BossAttackTrigger>();
        hitbox?.EnableHitbox();

        yield return new WaitForSeconds(0.3f);
        hitbox?.DisableHitbox();
    }

    public void ChangeState(BossState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        Debug.Log($"[BossController] State -> {newState}");

        anim?.SetInteger("State", (int)newState);
    }

    public void TriggerEnrage()
    {
        if (currentState == BossState.Dead) return;
        ChangeState(BossState.Enrage);
    }

    void OnBossDeath()
    {
        ChangeState(BossState.Dead);
        motor.Stop();
        Debug.Log($"[BossController] {bossData?.bossName} defeated!");
    }

    public BossState CurrentState => currentState;
}