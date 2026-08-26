using UnityEngine;
using System.Collections; // 🔴 สำคัญ: ต้องมีบรรทัดนี้เพื่อใช้ระบบหน่วงเวลากระพริบสี
using UnityEngine.UI;

public class BossController : MonoBehaviour
{
    [Header("Boss Stats")]
    public int maxHealth = 200;
    private int currentHealth;
    public float moveSpeed = 2f;
    
    [Header("Attack Settings")]
    public float attackRange = 2f; 
    public float attackCooldown = 2f; 
    private float nextAttackTime = 0f;

    [Header("Attack Chances")]
    [Tooltip("โอกาสออกท่าตีหนัก (0.0 ถึง 1.0) เช่น 0.3 คือ 30%")]
    public float heavyAttackChance = 0.3f; 
    // 🔴 เพิ่มตัวแปรสำหรับกระพริบสีขาว
    [Header("Hit Flash Effect")]
    public float flashDuration = 0.1f;
    private SpriteRenderer spriteRender;
    private Material originalMaterial;
    private Material whiteMaterial;
    private Transform player;
    private Animator anim;
    private Rigidbody2D rb;

    [Header("UI หลอดเลือดและข้อความชนะ")]
    public Image healthBarFill; 
    public GameObject bossHealthUIParent; // ➕ ช่องลากกรอบหลอดเลือดทั้งหมด (BossHealt_BG) มาใส่
    public GameObject winUIObject;      
    
    public CanvasGroup winUIcanvasGroup; 
    public float fadeSpeed = 1f; 
    public float winUIDelay = 1.5f;  // ➕ ช่องลาก WinUI_Container มาใส่
    public CanvasGroup winRestartButtonUI;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        spriteRender = GetComponent<SpriteRenderer>();
        if (spriteRender != null)
        {
            originalMaterial = spriteRender.material;
            whiteMaterial = new Material(Shader.Find("GUI/Text Shader"));
        }

        // ➕ เพิ่มตรงนี้: ซ่อนหน้าจอชนะตอนเริ่มเกม (Alpha = 0)
       if (winUIcanvasGroup != null)
        {
            winUIcanvasGroup.alpha = 0f;
            winUIcanvasGroup.interactable = false;
            winUIcanvasGroup.blocksRaycasts = false;
        }

        // ➕ สั่งซ่อนปุ่ม Restart ของหน้าจอชนะด้วย
        if (winRestartButtonUI != null)
        {
            winRestartButtonUI.alpha = 0f;
            winRestartButtonUI.interactable = false;
            winRestartButtonUI.blocksRaycasts = false;
        }
    }

    void Update()
    {
        if (player == null || currentHealth <= 0) return;

        // เช็กระยะห่างแนวนอน (แกน X) ป้องกันบั๊กฮีโร่กระโดดข้ามหัว
        float distanceToPlayer = Mathf.Abs(transform.position.x - player.position.x);

        FacePlayer(); // หันหน้ามองฮีโร่ตลอดเวลา

        if (distanceToPlayer > attackRange)
        {
            ChasePlayer(); 
        }
        else
        {
            AttackPlayer(); 
        }
    }
    void FacePlayer()
    {
        Vector3 scale = transform.localScale;
        if (player.position.x > transform.position.x)
            scale.x = -Mathf.Abs(scale.x); 
        else
            scale.x = Mathf.Abs(scale.x); 

        transform.localScale = scale;
    }

    void ChasePlayer()
    {
        anim.SetFloat("Speed", moveSpeed);
        Vector2 targetPos = new Vector2(player.position.x, transform.position.y);
        transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
    }

    void AttackPlayer()
    {
        anim.SetFloat("Speed", 0);

        if (Time.time >= nextAttackTime)
        {
            // สุ่มท่าโจมตี
            float randomChance = Random.value; 
            if (randomChance <= heavyAttackChance)
            {
                anim.SetTrigger("Boss_atk2"); 
            }
            else
            {
                anim.SetTrigger("Boss_atk"); 
            }
            
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        // ➕ สั่งให้หลอดเลือด UI ลดลงตามเปอร์เซ็นต์เลือดที่เหลือ (เอาเลือดปัจจุบัน หาร เลือดสูงสุด)
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
        
        if (spriteRender != null) StartCoroutine(FlashWhiteRoutine());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 🔴 ฟังก์ชันสลับสีบอสเป็นสีขาว
    private IEnumerator FlashWhiteRoutine()
    {
        spriteRender.material = whiteMaterial;
        yield return new WaitForSeconds(flashDuration);
        spriteRender.material = originalMaterial;
    }

    void Die()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; 
            rb.gravityScale = 0f;             
        }

        anim.SetFloat("Speed", 0);
        anim.SetTrigger("isDead"); 

        GetComponent<Collider2D>().enabled = false;
        
        // 1. ซ่อนกรอบหลอดเลือดบอสทันที
        if (bossHealthUIParent != null)
        {
            bossHealthUIParent.SetActive(false);
        }

        // 2. ➕ สั่งรันระบบเฟดหน้าจอชนะขึ้นมาอย่างนุ่มนวล
        if (winUIcanvasGroup != null)
        {
            if (winUIObject != null) winUIObject.SetActive(true); // เปิด Object รอไว้
            StartCoroutine(FadeInWinScreen());
        }

        this.enabled = false;
    }

    // ➕ 3. โค้ดสำหรับทำ Fade In หน้าจอชนะ (ถอดแบบมาจากไฟล์ Health.cs เป๊ะๆ)
    IEnumerator FadeInWinScreen()
    {
        yield return new WaitForSeconds(1.5f); 

        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * fadeSpeed;
            winUIcanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime);
            yield return null; 
        }

        if (winUIcanvasGroup != null)
        {
            winUIcanvasGroup.interactable = true;
            winUIcanvasGroup.blocksRaycasts = true;
        }

        // ➕ 1. รอเวลาอีก 1.5 วินาที ให้คนเล่นชื่นชมคำว่า VICTORY ก่อน
        yield return new WaitForSeconds(1.5f);

        // ➕ 2. สั่งเฟดปุ่ม Restart ให้ค่อยๆ โผล่ขึ้นมา
        if (winRestartButtonUI != null)
        {
            elapsedTime = 0f;
            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime * fadeSpeed;
                winRestartButtonUI.alpha = Mathf.Lerp(0f, 1f, elapsedTime);
                yield return null; 
            }
            
            // เปิดให้ปุ่มสามารถกดได้
            winRestartButtonUI.interactable = true;
            winRestartButtonUI.blocksRaycasts = true;
        }
    }

    [Header("Attack Impact")]
    public int attackDamage = 30; 
    public float shakeDuration = 0.2f; 
    public float shakeMagnitude = 0.4f; 

    // ฟังก์ชันทำงานตอนขวานสับลงพื้น
    public void Event_BossAttackHit()
    {
        // สั่นกล้องตอนบอสสับ
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.TriggerShake(shakeDuration, shakeMagnitude);
        }

        // 🔴 เช็กดาเมจแนวแกน X (ถึงฮีโร่กระโดดลอยอยู่ แต่ถ้าไม่พ้นขวานก็โดนดาเมจ)
        if (player != null && Mathf.Abs(transform.position.x - player.position.x) <= attackRange + 0.5f)
        {
            Health hp = player.GetComponent<Health>(); 
            if (hp != null)
            {
                hp.TakeDamage(attackDamage);
            }
        }
    }
}