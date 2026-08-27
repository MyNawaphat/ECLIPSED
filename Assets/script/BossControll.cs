using UnityEngine;
using System.Collections; 
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
    public GameObject bossHealthUIParent; 
    public GameObject winUIObject;      
    
    public CanvasGroup winUIcanvasGroup; 
    public float fadeSpeed = 1f; 
    public float winUIDelay = 1.5f; 
    public CanvasGroup winRestartButtonUI;

    // 🌟 1. สิ่งที่เพิ่มเข้ามา: ระบบปลุกบอสเมื่อเดินเข้าใกล้
    [Header("Wake Up Settings")]
    public float wakeUpRange = 12f; // ระยะที่ฮีโร่เดินมาใกล้แล้วบอสจะตื่น (ปรับเลขได้)
    private bool isAwake = false;   // สถานะว่าบอสตื่นหรือยัง

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

        // 🌟 2. บังคับซ่อนหลอดเลือดบอสไว้ก่อนตั้งแต่เริ่มเกม
        if (bossHealthUIParent != null)
        {
            bossHealthUIParent.SetActive(false); 
        }

        if (winUIcanvasGroup != null)
        {
            winUIcanvasGroup.alpha = 0f;
            winUIcanvasGroup.interactable = false;
            winUIcanvasGroup.blocksRaycasts = false;
        }

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

        float distanceToPlayer = Mathf.Abs(transform.position.x - player.position.x);

        // 🌟 3. ถ้ายืนอยู่นอกระยะปลุกบอส ให้บอสยืนนิ่งๆ ไม่ทำอะไรเลย
        if (!isAwake)
        {
            if (distanceToPlayer <= wakeUpRange)
            {
                isAwake = true; // บอสตื่นแล้ว!
                if (bossHealthUIParent != null) bossHealthUIParent.SetActive(true); // โชว์หลอดเลือดบอส
            }
            else
            {
                return; // หยุดการทำงาน Update ไม่ให้เดินตามฮีโร่
            }
        }

        FacePlayer(); 

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
        
        if (bossHealthUIParent != null)
        {
            bossHealthUIParent.SetActive(false);
        }

        if (winUIcanvasGroup != null)
        {
            if (winUIObject != null) winUIObject.SetActive(true); 
            StartCoroutine(FadeInWinScreen());
        }

        this.enabled = false;
    }

    IEnumerator FadeInWinScreen()
    {
        yield return new WaitForSeconds(winUIDelay); 

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

        yield return new WaitForSeconds(1.5f);

        if (winRestartButtonUI != null)
        {
            elapsedTime = 0f;
            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime * fadeSpeed;
                winRestartButtonUI.alpha = Mathf.Lerp(0f, 1f, elapsedTime);
                yield return null; 
            }
            
            winRestartButtonUI.interactable = true;
            winRestartButtonUI.blocksRaycasts = true;
        }
    }

    [Header("Attack Impact")]
    public int attackDamage = 30; 
    public float shakeDuration = 0.2f; 
    public float shakeMagnitude = 0.4f; 

    public void Event_BossAttackHit()
    {
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.TriggerShake(shakeDuration, shakeMagnitude);
        }

        if (player != null && Mathf.Abs(transform.position.x - player.position.x) <= attackRange + 0.5f)
        {
            Health hp = player.GetComponent<Health>(); 
            if (hp != null)
            {
                hp.TakeDamage(attackDamage);
            }
        }
    }

    // 🌟 4. วาดเส้นวงกลมสีชมพูใน Scene เพื่อให้คุณกะระยะการตื่นของบอสได้ง่ายๆ
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, wakeUpRange); 
    }
}