using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    // ตัวแปรพิเศษเพื่อให้สคริปต์อื่นเรียกใช้กล้องตัวนี้ได้ง่ายๆ
    public static CameraShake Instance; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // ฟังก์ชันสั่งกล้องสั่น (รับค่าความนาน และ ความแรง)
    public void TriggerShake(float duration, float magnitude)
    {
        StartCoroutine(Shake(duration, magnitude));
    }

    private IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // สุ่มตำแหน่งให้ขยับไปมาแบบรวดเร็ว
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;

            yield return null; // รอเฟรมถัดไป
        }

        // สั่นเสร็จแล้ว ดึงกล้องกลับมาที่เดิม
        transform.localPosition = originalPos; 
    }
}