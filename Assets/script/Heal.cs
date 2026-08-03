using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Health))] //
[RequireComponent(typeof(Stamina))] // บังคับว่าต้องมี Stamina ด้วย
public class Heal : MonoBehaviour
{
    [Header("Heal Settings")]
    public float healAmount = 20f; //[cite: 2]
    
    // === [ส่วนที่เพิ่มเข้ามาใหม่] ตั้งค่าว่าจะให้ถ้วยชาเพิ่มพลังงานเท่าไหร่ ===
    public float staminaRestoreAmount = 30f; 
    
    public float teacupCooldown = 5f; //[cite: 2]
    
    [Header("UI Settings")]
    public Image teacupUI; //[cite: 2]
    public Sprite fullTeacup; //[cite: 2]
    public Sprite emptyTeacup; //[cite: 2]

    private bool canUseTeacup = true; //[cite: 2]
    private Health health; //[cite: 2]
    
    // ประกาศตัวแปร Stamina
    private Stamina staminaSystem; 

    void Start()
    {
        health = GetComponent<Health>(); //[cite: 2]
        
        // ดึงสคริปต์ Stamina ในตัวละครมาใช้งาน
        staminaSystem = GetComponent<Stamina>(); 
        
        if (teacupUI != null) teacupUI.sprite = fullTeacup; //[cite: 2]
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) //[cite: 2]
        {
            TryUseTeacup(); //[cite: 2]
        }
    }

    public void TryUseTeacup()
    {
        // แก้ไขเงื่อนไข: จะดื่มชาได้ก็ต่อเมื่อ "เลือดไม่เต็ม" หรือ "พลังงานไม่เต็ม" อย่างใดอย่างหนึ่ง
        if (canUseTeacup && (health.currentHealth < health.maxHealth || staminaSystem.currentStamina < staminaSystem.maxStamina))
        {
            health.Heal(healAmount); //[cite: 2]
            
            // สั่งเพิ่มพลังงาน
            staminaSystem.RestoreStamina(staminaRestoreAmount); 
            
            StartCoroutine(TeacupCooldownRoutine()); //[cite: 2]
        }
    }

    private IEnumerator TeacupCooldownRoutine() //[cite: 2]
    {
        canUseTeacup = false; //[cite: 2]
        if (teacupUI != null) teacupUI.sprite = emptyTeacup; //[cite: 2]

        yield return new WaitForSeconds(teacupCooldown); //[cite: 2]

        canUseTeacup = true; //[cite: 2]
        if (teacupUI != null) teacupUI.sprite = fullTeacup; //[cite: 2]
    }
}