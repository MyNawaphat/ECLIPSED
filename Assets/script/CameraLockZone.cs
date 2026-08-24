using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CameraLockZone : MonoBehaviour
{
    [Header("จุดที่จะให้กล้องล็อก (ล็อกไว้กลางห้อง)")]
    public Transform centerPoint;

    private CameraFollow camScript;

    void Start()
    {
        // ค้นหากล้องหลักในฉาก
        camScript = Camera.main.GetComponent<CameraFollow>();
        
        // ตั้งค่าให้โซนนี้เป็น Trigger (เดินทะลุได้)
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    // เมื่อฮีโร่เดินเข้ามาในโซน
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            camScript.isFollowing = false; // สั่งกล้องเลิกตาม
            camScript.lockPosition = centerPoint.position; // ล็อกกล้องไว้ที่จุด Center ที่เราตั้งไว้
        }
    }

    // เมื่อฮีโร่เดินออกจากโซน (หรือตีมอนตายแล้วประตูเปิดให้เดินออก)
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            camScript.isFollowing = true; // สั่งกล้องกลับมาตามฮีโร่เหมือนเดิม
        }
    }
}