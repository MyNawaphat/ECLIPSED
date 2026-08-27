using UnityEngine;

public class RoomClearManager : MonoBehaviour
{
    [Header("ลากมอนสเตอร์ในด่านนี้มาใส่ให้ครบ")]
    public GameObject[] monstersInRoom;

    [Header("แท่นวาร์ปที่จะให้โผล่มา")]
    public GameObject warpStatue;

    private bool isCleared = false;

    void Start()
    {
        // เริ่มเกมมา บังคับซ่อนแท่นวาร์ปไว้ก่อนเลย
        if (warpStatue != null) warpStatue.SetActive(false);
    }

    void Update()
    {
        if (isCleared) return;

        bool allDead = true;
        
        // เช็กว่ามอนสเตอร์ทุกตัวถูกทำลาย (กลายเป็น null) หรือยัง
        foreach (GameObject monster in monstersInRoom)
        {
            if (monster != null)
            {
                allDead = false; // ถ้ายังมีมอนสเตอร์เหลืออยู่ แปลว่ายังไม่เคลียร์
                break;
            }
        }

        // ถ้าศพมอนสเตอร์ตัวสุดท้ายหายไปแล้ว -> เปิดรูปปั้น!
        if (allDead)
        {
            isCleared = true;
            if (warpStatue != null)
            {
                warpStatue.SetActive(true); 
            }
        }
    }
}