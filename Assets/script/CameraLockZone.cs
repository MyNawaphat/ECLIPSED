using UnityEngine;

public class CameraZone : MonoBehaviour
{
    [Header("ใส่ตัวเลขขอบเขตกล้องที่พอดีกับด่านนี้")]
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ถ้าคนที่เดินเข้ามาในโซนนี้คือฮีโร่
        if (collision.CompareTag("Player"))
        {
            // ค้นหากล้องหลัก และดึงสคริปต์ CameraFollow มา
            CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
            
            if (camFollow != null)
            {
                // สั่งเปิดใช้งาน Limit (เผื่อด่านก่อนหน้าปิดไว้)
                // หมายเหตุ: ถ้าในสคริปต์คุณตั้งชื่อตัวแปร Use Limit ต่างออกไป ให้แก้บรรทัดนี้ให้ตรงกันนะครับ
                // camFollow.useLimit = true; 

                // อัปเดตตัวเลขขอบเขตใหม่ให้เป็นของด่านนี้
                camFollow.minX = this.minX;
                camFollow.maxX = this.maxX;
                camFollow.minY = this.minY;
                camFollow.maxY = this.maxY;
            }
        }
    }
}