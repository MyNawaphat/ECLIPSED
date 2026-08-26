using UnityEngine;
using UnityEngine.UI;
using System.Collections; 
using UnityEngine.SceneManagement; 

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public Image healthBar; 
    
    [Header("UI Settings")]
    public CanvasGroup deathUI; 
    public float fadeSpeed = 1f; 
    // 🔴 1. เอากล่องใส่ปุ่ม Restart กลับมา
    public CanvasGroup restartButtonUI; 
    public GameObject bossHealthUI;

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
        
        spriteRender = GetComponent<SpriteRenderer>();
        if (spriteRender != null)
        {
            originalMaterial = spriteRender.material;
            whiteMaterial = new Material(Shader.Find("GUI/Text Shader"));
        }

        if(deathUI != null) 
        {
            deathUI.alpha = 0f;
            deathUI.interactable = false; 
            deathUI.blocksRaycasts = false; 
        }

        // 🔴 2. ซ่อนปุ่ม Restart ตอนเริ่มเกม
        if (restartButtonUI != null)
        {
            restartButtonUI.alpha = 0f;
            restartButtonUI.interactable = false;
            restartButtonUI.blocksRaycasts = false;
        }
        
        UpdateUI();
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return; 

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();

        if (spriteRender != null) StartCoroutine(FlashWhiteRoutine());

        if (currentHealth <= 0)
        {
            Die(); 
        }
        else
        {
            if(anim != null) anim.Play("TakeHit"); 
        }
    }

    private IEnumerator FlashWhiteRoutine()
    {
        spriteRender.material = whiteMaterial;
        yield return new WaitForSeconds(flashDuration);
        spriteRender.material = originalMaterial;
    }

    void Die()
    {
        isDead = true; 
        
        if(anim != null) 
        {
            anim.SetFloat("Speed", 0);
            anim.SetFloat("Floating", 0);
            anim.SetBool("isGrounded", true); 
            anim.SetTrigger("Dead"); 
        } 

        if(playerMovement != null) playerMovement.enabled = false;
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero; 

        gameObject.tag = "Untagged"; 

        if (bossHealthUI != null)
        {
            bossHealthUI.SetActive(false);
        }

        if(deathUI != null)
        {
            StartCoroutine(FadeInDeathScreen());
        }
    }

    IEnumerator FadeInDeathScreen()
    {
        yield return new WaitForSeconds(1.5f);

        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * fadeSpeed;
            deathUI.alpha = Mathf.Lerp(0f, 1f, elapsedTime);
            yield return null; 
        }

        if (deathUI != null)
        {
            deathUI.interactable = true;
            deathUI.blocksRaycasts = true;
        }

        // 🔴 3. โค้ดดีเลย์ 3 วิ และเฟดปุ่มกลับมาแล้วครับ!
        yield return new WaitForSeconds(1.5f);

        if (restartButtonUI != null)
        {
            elapsedTime = 0f;
            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime * fadeSpeed;
                restartButtonUI.alpha = Mathf.Lerp(0f, 1f, elapsedTime);
                yield return null; 
            }
            
            // เปิดให้ปุ่มกดได้
            restartButtonUI.interactable = true;
            restartButtonUI.blocksRaycasts = true;
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

    // ฟังก์ชันสำหรับรีสตาร์ท
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}