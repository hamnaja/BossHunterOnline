using UnityEngine;
using TMPro;

public class DroppedItem : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float pickupRadius = 1.5f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;
    [SerializeField] private float rotateSpeed = 90f;

    // ข้อมูลไอเทม
    private int itemID;
    private int amount;
    private string itemName;

    // State
    private Vector3 startPos;
    private Transform playerTransform;

    // Events
    public static event System.Action<int, int> OnItemPickedUp; // itemID, amount

    void Start()
    {
        startPos = transform.position;
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    public void Initialize(int id, int amt, string name)
    {
        itemID   = id;
        amount   = amt;
        itemName = name;
    }

    void Update()
    {
        // Bob ขึ้นลง
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // หมุนรอบตัวเอง
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);

        // เช็คระยะเก็บ
        if (playerTransform == null) return;
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist <= pickupRadius)
            PickUp();
    }

    void PickUp()
    {
        OnItemPickedUp?.Invoke(itemID, amount);
        Debug.Log($"[DroppedItem] Picked up {itemName} x{amount}");
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
