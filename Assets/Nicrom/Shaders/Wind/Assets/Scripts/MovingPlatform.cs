using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("ตั้งค่าการเคลื่อนที่")]
    public float speed = 2f;      // ความเร็วในการเลื่อน
    public float distance = 3f;   // ระยะทางที่จะเลื่อนขึ้นลง (ยิ่งเยอะยิ่งขึ้นสูง)
    
    private float startY;         // เก็บค่าตำแหน่ง Y เริ่มต้น

    void Start()
    {
        // จำตำแหน่งเริ่มต้นของหินไว้ตอนเริ่มเกม
        startY = transform.position.y;
    }

    void Update()
    {
        // คำนวณตำแหน่ง Y ใหม่โดยใช้สมการคลื่น (Mathf.Sin) ให้มันเลื่อนขึ้นลงสมูทๆ
        float newY = startY + Mathf.Sin(Time.time * speed) * distance;
        
        // อัปเดตตำแหน่งของหิน
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}