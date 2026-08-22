using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class TutorialFade : MonoBehaviour
{
    [Header("Fade Settings")]
    public float showDuration = 3f; // โชว์ค้างไว้ 3 วินาที
    public float fadeSpeed = 1f;    

    private CanvasGroup cg;

    void Start()
    {
        cg = GetComponent<CanvasGroup>();
        
        // บังคับให้โปร่งใสเป็น 1 (เห็นชัดเจน) ทันทีที่เริ่มเกม
        cg.alpha = 1f; 
        
        // เริ่มนับเวลาเฟดหายทันที โดยไม่ต้องเช็กว่าเคยเล่นหรือยัง
        StartCoroutine(FadeOutRoutine());
    }

    IEnumerator FadeOutRoutine()
    {
        // 1. รอเวลาให้ผู้เล่นอ่าน (3 วินาที)
        yield return new WaitForSeconds(showDuration);

        // 2. ค่อยๆ ปรับค่า Alpha จาก 1 ลดลงไป 0
        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * fadeSpeed;
            cg.alpha = Mathf.Lerp(1f, 0f, elapsedTime);
            yield return null;
        }

        // 3. ปิดตัวเองทิ้ง
        gameObject.SetActive(false);
    }
}