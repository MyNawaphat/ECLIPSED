using UnityEngine;
using UnityEngine.UI;
using System.Collections; // 🔴 ต้องเพิ่มบรรทัดนี้บนสุด เพื่อใช้ระบบหน่วงเวลา (Coroutine)

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public Image healthBar; 
    
    [Header("UI Settings")]
    // 🔴 เปลี่ยนจาก GameObject เป็น CanvasGroup
    public CanvasGroup deathUI; 
    public float fadeSpeed = 1f; // ความเร็วในการเฟดจอดำ

    private Animator anim;
    private PlayerController playerMovement; 
    private bool isDead = false; 

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerController>(); 
        
        // 🔴 บังคับให้หน้าจอตั้งค่าโปร่งใสเป็น 0 ตอนเริ่มเกม
        if(deathUI != null) deathUI.alpha = 0f;
        
        UpdateUI();
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return; 

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();

        if (currentHealth <= 0)
        {
            Die(); 
        }
        else
        {
            if(anim != null) anim.Play("TakeHit"); 
        }
    }

    void Die()
    {
        isDead = true; 
        if(anim != null) anim.Play("Death"); 

        if(playerMovement != null) playerMovement.enabled = false;
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero; 

        gameObject.tag = "Untagged"; 

        // 🔴 เรียกใช้ฟังก์ชันเฟดหน้าจอตอนตาย
        if(deathUI != null)
        {
            StartCoroutine(FadeInDeathScreen());
        }
    }

    // 🔴 ฟังก์ชันใหม่: จัดการระบบค่อยๆ เฟดจอดำแบบ Dark Souls
    IEnumerator FadeInDeathScreen()
    {
        // 1. รอให้ตัวละครเล่นท่าตายจบก่อน (ตั้งไว้ที่ 1.5 วินาที ปรับเพิ่มลดได้ครับ)
        yield return new WaitForSeconds(1.5f);

        // 2. ค่อยๆ ปรับค่า Alpha ของหน้าจอจาก 0 (ล่องหน) ไป 1 (ทึบ)
        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * fadeSpeed;
            deathUI.alpha = Mathf.Lerp(0f, 1f, elapsedTime);
            yield return null; // รอให้รันเฟรมถัดไป
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return; 
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (healthBar != null) healthBar.fillAmount = currentHealth / maxHealth;
    }
}