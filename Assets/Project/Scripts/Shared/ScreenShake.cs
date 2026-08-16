using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

// ระบบเขย่าจอเมื่อโดนตีหรือบอสเข้าเฟสใหม่
public class ScreenShake : MonoBehaviour
{
    public static ScreenShake Instance { get; private set; }

    [SerializeField] private CinemachineCamera virtualCamera;

    private CinemachineBasicMultiChannelPerlin noise;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (virtualCamera != null)
            noise = virtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void Shake(float intensity = 1f, float duration = 0.2f)
    {
        StartCoroutine(ShakeRoutine(intensity, duration));
    }

    IEnumerator ShakeRoutine(float intensity, float duration)
    {
        if (noise != null)
            noise.AmplitudeGain = intensity;

        yield return new WaitForSeconds(duration);

        if (noise != null)
            noise.AmplitudeGain = 0f;
    }
}
