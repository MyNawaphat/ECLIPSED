using UnityEngine;
using System.Collections;

public class StatueFadeIn1 : MonoBehaviour
{
    [Header("ความเร็วในการปรากฏตัว")]
    public float fadeSpeed = 1f; 

    // ➕ เปลี่ยนจากตัวแปรเดี่ยว เป็นแบบ Array (เก็บได้หลายชิ้น)
    private SpriteRenderer[] allSprites; 

    void Awake()
    {
        // ➕ ใช้ GetComponentsInChildren เพื่อดึงภาพทั้งของรูปปั้นและแสงพื้นหลัง (sun_0) มาทั้งหมด
        allSprites = GetComponentsInChildren<SpriteRenderer>();

        // สั่งให้ทุกชิ้นโปร่งใสเป็น 0 รอไว้ตั้งแต่เริ่มเกม
        foreach (SpriteRenderer sr in allSprites)
        {
            Color c = sr.color;
            c.a = 0f; 
            sr.color = c;
        }
    }

    void Start()
    {
        StartCoroutine(FadeInRoutine());
    }

    IEnumerator FadeInRoutine()
    {
        float currentAlpha = 0f;
        
        while (currentAlpha < 1f)
        {
            currentAlpha += Time.deltaTime * fadeSpeed;

            // สั่งค่อยๆ เพิ่มความสว่างให้ทุกชิ้นพร้อมๆ กัน
            foreach (SpriteRenderer sr in allSprites)
            {
                Color c = sr.color;
                c.a = Mathf.Clamp01(currentAlpha); // บังคับไม่ให้ค่าเกิน 1
                sr.color = c;
            }
            
            yield return null;
        }
    }
}