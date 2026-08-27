using UnityEngine;
using System.Collections;

public class StatueFadeIn : MonoBehaviour
{
    [Header("ความเร็วในการปรากฏตัว")]
    public float fadeSpeed = 1f; 

    // 1. 🔴 เปลี่ยนจากตัวแปรเดี่ยว เป็นแบบ Array (เติม [])
    private SpriteRenderer[] allSprites; 

    void Awake()
    {
        // 2. 🔴 ใช้ GetComponentsInChildren เพื่อกวาดภาพทั้งรูปปั้นและออร่ามาทั้งหมด
        allSprites = GetComponentsInChildren<SpriteRenderer>();

        // สั่งให้ทุกชิ้นโปร่งใสเป็น 0 รอไว้ตั้งแต่เริ่มเกม
        if (allSprites != null)
        {
            foreach (SpriteRenderer sr in allSprites)
            {
                Color c = sr.color;
                c.a = 0f; 
                sr.color = c;
            }
        }
    }

    // ฟังก์ชันนี้จะทำงานอัตโนมัติเมื่อ RoomClearManager สั่งเปิดรูปปั้น
    void OnEnable()
    {
        if (allSprites != null && allSprites.Length > 0)
        {
            StartCoroutine(FadeInRoutine());
        }
    }

    // กระบวนการค่อยๆ เพิ่มสีให้เข้มขึ้น
    IEnumerator FadeInRoutine()
    {
        float currentAlpha = 0f;
        
        while (currentAlpha < 1f)
        {
            currentAlpha += Time.deltaTime * fadeSpeed;

            // 3. 🔴 ใช้ foreach สั่งค่อยๆ เพิ่มความสว่างให้ทุกชิ้นพร้อมๆ กัน
            foreach (SpriteRenderer sr in allSprites)
            {
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = Mathf.Clamp01(currentAlpha); // บังคับไม่ให้ค่าเกิน 1
                    sr.color = c;
                }
            }
            
            yield return null;
        }
    }
}