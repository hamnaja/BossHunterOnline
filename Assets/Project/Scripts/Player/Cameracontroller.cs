using UnityEngine;
using UnityEngine.InputSystem;

// ติดบน Empty GameObject "CameraRig" ที่อยู่นอก Player
// CinemachineCamera เป็น child ของ CameraRig
public class CameraController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float sensitivity = 2f;
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 60f;
    [SerializeField] private float cameraDistance = 5f;
    [SerializeField] private float cameraHeight = 1.5f;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform cameraTransform; // ลาก CinemachineCamera ใส่

    private float yaw = 0f;
    private float pitch = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (player == null || cameraTransform == null) return;

        // อ่านเมาส์
        Vector2 mouse = Mouse.current.delta.ReadValue();
        yaw   += mouse.x * sensitivity;
        pitch -= mouse.y * sensitivity;
        pitch  = Mathf.Clamp(pitch, minPitch, maxPitch);

        // คำนวณตำแหน่งกล้องรอบ Player
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = rotation * new Vector3(0, cameraHeight, -cameraDistance);
        cameraTransform.position = player.position + offset;
        cameraTransform.rotation = rotation;

        // หมุน Player ตาม yaw เท่านั้น
        player.rotation = Quaternion.Euler(0f, yaw, 0f);

        // Escape ปลด cursor
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}