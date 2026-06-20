using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BossMotor : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float stoppingDistance = 2.5f;

    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed            = moveSpeed;
        agent.acceleration     = acceleration;
        agent.stoppingDistance = stoppingDistance;
    }

    public void MoveTo(Vector3 destination)
    {
        if (!agent.isActiveAndEnabled) return;
        agent.isStopped = false;
        agent.SetDestination(destination);
    }

    public void Stop()
    {
        if (!agent.isActiveAndEnabled) return;
        agent.isStopped = true;
        agent.velocity  = Vector3.zero;
    }

    public void SetSpeed(float speed)
    {
        agent.speed = speed;
    }

    public float BaseSpeed => moveSpeed;
}
