using UnityEngine;
using UnityEngine.UI;
using System.Collections; 

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public Image healthBar; 
    
    [Header("UI Settings")]
    public CanvasGroup deathUI; 
    public float fadeSpeed = 1f; 

    // 🔴 เพิ่มตัวแปรสำหรับทำตัวกระพริบขาว
    [Header("Hit Flash Effect")]
    public float flashDuration = 0.1f;
    private SpriteRenderer spriteRender;
    private Material originalMaterial;
    private Material whiteMaterial;

    private Animator anim;
    private PlayerController playerMovement; 
    private bool isDead = false; 

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerController>(); 
        
        // 🔴 ตั้งค่า Material สำหรับกระพริบขาว
        spriteRender = GetComponent<SpriteRenderer>();
        if (spriteRender != null)
        {
            originalMaterial = spriteRender.material;
            whiteMaterial = new Material(Shader.Find("GUI/Text Shader"));
        }

        // บังคับให้หน้าจอตั้งค่าโปร่งใสเป็น 0 ตอนเริ่มเกม
        if(deathUI != null) deathUI.alpha = 0f;
        
        UpdateUI();
    }

    public void TakeDamage(float damage)
    {
        // ถ้าตายไปแล้ว (isDead = true) ให้เด้งออกทันที ไม่ต้องทำอะไรต่อ
        if (isDead) return; 

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();

        // 🔴 สั่งให้ตัวละครกระพริบสีขาว 1 แว้บ (ทำงานได้แม้ลอยอยู่กลางอากาศ)
        if (spriteRender != null) StartCoroutine(FlashWhiteRoutine());

        // เช็กว่าเลือดหมดหรือยัง
        if (currentHealth <= 0)
        {
            Die(); 
        }
        else
        {
            if(anim != null) anim.Play("TakeHit"); 
        }
    }

    // 🔴 ฟังก์ชันสลับสีตัวละครให้เป็นสีขาว
    private IEnumerator FlashWhiteRoutine()
    {
        spriteRender.material = whiteMaterial;
        yield return new WaitForSeconds(flashDuration);
        spriteRender.material = originalMaterial;
    }

    void Die()
    {
        // ล็อกกุญแจทันที! เพื่อป้องกันไม่ให้คำสั่งนี้โดนเรียกซ้ำรัวๆ
        isDead = true; 
        
        // สั่งให้ Animator เล่นท่าตาย
        if(anim != null) 
        {
            anim.SetFloat("Speed", 0);
            anim.SetFloat("Floating", 0);
            anim.SetBool("isGrounded", true); // หลอกมันว่าอยู่บนพื้นแล้ว
            
            anim.SetTrigger("Dead"); // ค่อยสั่งให้ตาย
        } 

        // ปิดการเดินและหยุดความเร็วตัวละคร
        if(playerMovement != null) playerMovement.enabled = false;
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero; 

        gameObject.tag = "Untagged"; 

        // เรียกใช้ฟังก์ชันเฟดหน้าจอตอนตาย
        if(deathUI != null)
        {
            StartCoroutine(FadeInDeathScreen());
        }
    }

    // จัดการระบบค่อยๆ เฟดจอดำ
    IEnumerator FadeInDeathScreen()
    {
        // 1. รอให้ตัวละครเล่นท่าตายจบก่อน 1.5 วินาที
        yield return new WaitForSeconds(1.5f);

        // 2. ค่อยๆ ปรับค่า Alpha ของหน้าจอจาก 0 ไป 1
        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * fadeSpeed;
            deathUI.alpha = Mathf.Lerp(0f, 1f, elapsedTime);
            yield return null; 
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