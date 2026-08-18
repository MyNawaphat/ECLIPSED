using UnityEngine;
using System.Collections; // 🔴 สำคัญ: ต้องมีบรรทัดนี้เพื่อใช้ระบบหน่วงเวลากระพริบสี

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

        // 🔴 ตั้งค่าระบบเตรียมกระพริบขาว
        spriteRender = GetComponent<SpriteRenderer>();
        if (spriteRender != null)
        {
            originalMaterial = spriteRender.material;
            whiteMaterial = new Material(Shader.Find("GUI/Text Shader"));
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
        
        // 🔴 สั่งให้บอสกระพริบสีขาว!
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
        anim.SetFloat("Speed", 0);
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
        Destroy(gameObject, 3f);
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