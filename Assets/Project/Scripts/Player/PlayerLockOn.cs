using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class PlayerLockOn : MonoBehaviour
{
    [Header("Lock-On Settings")]
    [SerializeField] private float detectionRadius = 20f;
    [SerializeField] private LayerMask bossLayer;
    [SerializeField] private CinemachineCamera virtualCamera;

    // State
    private Transform currentTarget;
    private bool isLockedOn = false;

    // Events
    public static event System.Action<Transform> OnLockOnTarget;
    public static event System.Action OnLockOnReleased;

    void Update()
    {
        if (isLockedOn && currentTarget != null)
            FaceTarget();
    }

    // Player Input เรียกเมื่อกดปุ่ม Middle Mouse
    public void OnLockOn(InputValue value)
    {
        if (!value.isPressed) return;

        if (isLockedOn)
        {
            ReleaseLockOn();
            return;
        }

        TryLockOn();
    }

    void TryLockOn()
    {
        // ค้นหา Boss ในรัศมี
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, bossLayer);
        if (hits.Length == 0) return;

        // เลือกตัวที่ใกล้ที่สุด
        Transform closest = null;
        float minDist = Mathf.Infinity;
        foreach (var hit in hits)
        {
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = hit.transform;
            }
        }

        if (closest == null) return;

        // ล็อกเป้า
        currentTarget = closest;
        isLockedOn = true;

        // ผูกกล้อง Cinemachine ให้หันหาบอส
        if (virtualCamera != null)
            virtualCamera.LookAt = currentTarget;

        OnLockOnTarget?.Invoke(currentTarget);
        Debug.Log($"[LockOn] Locked onto: {currentTarget.name}");
    }

    void ReleaseLockOn()
    {
        currentTarget = null;
        isLockedOn = false;

        if (virtualCamera != null)
            virtualCamera.LookAt = null;

        OnLockOnReleased?.Invoke();
        Debug.Log("[LockOn] Released");
    }

    void FaceTarget()
    {
        // หมุนหน้าเข้าหาบอสเสมอขณะ Lock-On
        Vector3 dir = currentTarget.position - transform.position;
        dir.y = 0f;
        if (dir == Vector3.zero) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, targetRot, 10f * Time.deltaTime);
    }

    void OnDrawGizmosSelected()
    {
        // แสดงรัศมีตรวจจับใน Scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    public bool IsLockedOn => isLockedOn;
    public Transform CurrentTarget => currentTarget;
}