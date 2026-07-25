using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// บังคับว่าตัวละครที่ใส่สคริปต์นี้ ต้องมี HealthSystem อยู่ด้วย
[RequireComponent(typeof(Health))] 
public class Heal : MonoBehaviour
{
    [Header("Heal Settings")]
    public float healAmount = 20f;
    public float teacupCooldown = 5f;
    
    [Header("UI Settings")]
    public Image teacupUI;
    public Sprite fullTeacup;
    public Sprite emptyTeacup;

    private bool canUseTeacup = true;
    
    // แก้ไขชนิดคลาสตรงนี้เป็น Health (ตัว H พิมพ์ใหญ่)
    private Health health; 

    void Start()
    {
        // แก้ไขชนิดคลาสตรงนี้เป็น Health (ตัว H พิมพ์ใหญ่)
        health = GetComponent<Health>(); 
        
        // เซ็ตภาพเริ่มต้นให้เป็นถ้วยเต็ม
        if (teacupUI != null) teacupUI.sprite = fullTeacup;
    }

    void Update()
    {
        // กด H เพื่อดื่มชา
        if (Input.GetKeyDown(KeyCode.H))
        {
            TryUseTeacup();
        }
    }

    public void TryUseTeacup()
    {
        // เช็คว่าคูลดาวน์เสร็จแล้ว และเลือดยังไม่เต็ม
        if (canUseTeacup && health.currentHealth < health.maxHealth)
        {
            health.Heal(healAmount); // สั่งฮีลไปที่ระบบเลือด
            StartCoroutine(TeacupCooldownRoutine());
        }
    }

    private IEnumerator TeacupCooldownRoutine()
    {
        canUseTeacup = false;
        if (teacupUI != null) teacupUI.sprite = emptyTeacup; // เปลี่ยนเป็นถ้วยเปล่า

        yield return new WaitForSeconds(teacupCooldown); // รอเวลาคูลดาวน์

        canUseTeacup = true;
        if (teacupUI != null) teacupUI.sprite = fullTeacup; // เปลี่ยนกลับเป็นถ้วยเต็ม
    }
}