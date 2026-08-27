using UnityEngine;
using System.Collections; // ➕ ต้องมีบรรทัดนี้เพื่อใช้ระบบหน่วงเวลา (Coroutine)

public class StatueInteract : MonoBehaviour
{
    [Header("จุดที่จะวาร์ปไป")]
    public Transform destination;

    [Header("UI ข้อความบนหัวรูปปั้น (ตัว E)")]
    public CanvasGroup interactUI; 
    public float textFadeSpeed = 5f;   

    [Header("UI จอดำสำหรับเปลี่ยนฉาก")]
    public CanvasGroup blackScreenFade; // ➕ ช่องใส่จอดำ
    public float sceneFadeSpeed = 2f;   // ➕ ความเร็วในการเฟดจอมืด

    private bool isPlayerNear = false;
    private float targetAlpha = 0f; 
    private GameObject playerObj;
    private bool isWarping = false; // ➕ ตัวล็อกป้องกันการกด E เบิ้ลรัวๆ

    void Start()
    {
        if (interactUI != null) interactUI.alpha = 0f; 
    }

    void Update()
    {
        // เฟดตัวอักษร E บนหัว
        if (interactUI != null)
        {
            interactUI.alpha = Mathf.MoveTowards(interactUI.alpha, targetAlpha, textFadeSpeed * Time.deltaTime);
        }

        // ถ้ายืนใกล้ + กด E + และ "ยังไม่ได้กำลังวาร์ปอยู่"
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E) && !isWarping)
        {
            if (playerObj != null && destination != null)
            {
                // 🚀 สั่งเริ่มกระบวนการเฟดและวาร์ป
                StartCoroutine(FadeAndTeleportWarp());
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isWarping)
        {
            isPlayerNear = true;
            targetAlpha = 1f; 
            playerObj = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNear = false;
            targetAlpha = 0f; 
            playerObj = null;
        }
    }

    // 🎬 ฟังก์ชันกระบวนการเปลี่ยนฉาก
    IEnumerator FadeAndTeleportWarp()
    {
        isWarping = true; // ล็อกปุ่มไว้ก่อน

        if (blackScreenFade != null)
        {
            // 1. ค่อยๆ เฟดจอมืดลง (Alpha วิ่งไปหา 1)
            while (blackScreenFade.alpha < 1f)
            {
                blackScreenFade.alpha += Time.deltaTime * sceneFadeSpeed;
                yield return null;
            }

            // 2. เมื่อจอมืดสนิทแล้ว ให้ย้ายตำแหน่งฮีโร่
            playerObj.transform.position = destination.position;

            // 3. รอให้กล้องวิ่งตามไปที่ฮีโร่แป๊บนึง (จะได้ไม่เห็นกล้องเลื่อนตอนจอสว่าง)
            yield return new WaitForSeconds(0.5f);

            // 4. ค่อยๆ เฟดจอสว่างขึ้น (Alpha วิ่งกลับไปหา 0)
            while (blackScreenFade.alpha > 0f)
            {
                blackScreenFade.alpha -= Time.deltaTime * sceneFadeSpeed;
                yield return null;
            }
        }

        isWarping = false; // ปลดล็อกปุ่ม
    }
}