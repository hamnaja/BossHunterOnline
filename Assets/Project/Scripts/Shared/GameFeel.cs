using UnityEngine;

// ศูนย์รวม Game Feel — เรียกจากทุก class ที่ต้องการ feedback
public class GameFeel : MonoBehaviour
{
    public static GameFeel Instance { get; private set; }

    [Header("Hit Stop Settings")]
    [SerializeField] private float hitStopDuration = 0.05f;
    [SerializeField] private float hitStopTimeScale = 0.1f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // Hit Stop — หยุดเวลาชั่วครู่เมื่อโจมตีโดน (ทำให้รู้สึกหนัก)
    public void TriggerHitStop()
    {
        StartCoroutine(HitStopRoutine());
    }

    System.Collections.IEnumerator HitStopRoutine()
    {
        Time.timeScale = hitStopTimeScale;
        yield return new WaitForSecondsRealtime(hitStopDuration);
        Time.timeScale = 1f;
    }

    // Perfect Dodge Flash — กระพริบหน้าจอขาวชั่วครู่
    public void TriggerPerfectDodgeEffect()
    {
        ScreenShake.Instance?.Shake(0.5f, 0.1f);
        Debug.Log("[GameFeel] Perfect Dodge Effect!");
    }

    // Boss Phase Change — เขย่าจอแรงเมื่อบอสเปลี่ยนเฟส
    public void TriggerPhaseChangeEffect()
    {
        ScreenShake.Instance?.Shake(3f, 0.5f);
        Debug.Log("[GameFeel] Phase Change Effect!");
    }

    // Boss Death — เขย่าจอแรงมากเมื่อบอสตาย
    public void TriggerBossDeathEffect()
    {
        ScreenShake.Instance?.Shake(5f, 1f);
        Debug.Log("[GameFeel] Boss Death Effect!");
    }
}
