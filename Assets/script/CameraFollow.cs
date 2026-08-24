using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("ลากตัวละครมาใส่ช่องนี้")]
    public Transform target;

    [Header("ความสมูท")]
    public float smoothSpeed = 5f;

    [Header("ระยะห่างกล้อง")]
    public Vector3 offset = new Vector3(0f, 1.5f, -10f);

    [Header("--- ขอบเขตกล้อง (กันกล้องหลุดฉาก) ---")]
    public bool useLimit = true; // ติ๊กถูกเพื่อเปิดระบบล็อกขอบ
    public float minX = -5f; // ขอบซ้ายสุด
    public float maxX = 50f; // ขอบขวาสุด
    public float minY = 0f;  // ขอบล่างสุด
    public float maxY = 10f; // ขอบบนสุด

    [HideInInspector] public bool isFollowing = true; 
    [HideInInspector] public Vector3 lockPosition; 

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition;

        // ถ้าให้ตามตัวละคร
        if (isFollowing)
        {
            desiredPosition = target.position + offset;

            // ล็อกกล้องไม่ให้เลื่อนทะลุขอบเขตที่ตั้งไว้ (ถ้าเปิด useLimit)
            if (useLimit)
            {
                desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
                desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
            }
        }
        // ถ้าเข้าโซนล็อกกล้อง (ห้องสู้บอส)
        else
        {
            desiredPosition = lockPosition;
        }

        // บังคับให้แกน Z เป็น -10 เสมอ ภาพจะได้ไม่หาย
        desiredPosition.z = -10f;

        // เลื่อนกล้อง
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}