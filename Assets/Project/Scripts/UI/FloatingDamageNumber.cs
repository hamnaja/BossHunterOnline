using UnityEngine;
using TMPro;

// ตาม GDD Section 3.6 — FloatingDamage
// ใช้ Object Pooling ป้องกัน Memory Overhead
public class FloatingDamageNumber : MonoBehaviour
{
    [SerializeField] private TextMeshPro text;
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private float riseSpeed = 2f;
    [SerializeField] private float fadeSpeed = 2f;

    private float elapsed = 0f;
    private Color startColor;

    public void Initialize(float damage, bool isCrit, bool isElement)
    {
        elapsed = 0f;

        // สีตาม GDD: ขาว=ธรรมดา, เหลืองหนา=คริติคอล, แดง=ธาตุ
        if (isElement)
        {
            text.color = Color.red;
            text.fontSize = 4f;
            text.text = $"{damage:F0}";
        }
        else if (isCrit)
        {
            text.color = Color.yellow;
            text.fontSize = 5f;
            text.text = $"CRIT! {damage:F0}";
        }
        else
        {
            text.color = Color.white;
            text.fontSize = 3.5f;
            text.text = $"{damage:F0}";
        }

        startColor = text.color;
    }

    void Update()
    {
        elapsed += Time.deltaTime;

        // ลอยขึ้น
        transform.position += Vector3.up * riseSpeed * Time.deltaTime;

        // Fade out
        float alpha = Mathf.Lerp(1f, 0f, elapsed / lifetime);
        text.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

        // หันหน้าเข้ากล้องเสมอ
        if (Camera.main != null)
            transform.forward = Camera.main.transform.forward;

        if (elapsed >= lifetime)
            gameObject.SetActive(false); // คืน pool แทนการ Destroy
    }
}
