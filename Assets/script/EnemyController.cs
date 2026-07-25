using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Enemy Health")]
    public int maxHealth = 100;
    private int currentHealth;
    private bool isDead = false;

    [Header("AI Settings & Speeds")]
    public float chaseSpeed = 4f;     
    public float retreatSpeed = 2f;   
    public float detectionRange = 7f; 
    public float retreatRange = 3f;   
    public int runAwayHealth = 20;    

    [Header("Combat Options")]
    public float attackRange = 1.5f;  // ระยะที่จะหยุดเดินแล้วง้างตี
    public float attackDamage = 15f; 
    public float attackCooldown = 2f; // หน่วงเวลาก่อนตีรอบถัดไป
    public float stunChance = 0.3f;   // โอกาสตีติดสตัน (0.3 = 30%)
    public float stunDuration = 1.5f; // ระยะเวลาที่ฮีโร่จะขยับไม่ได้
    private float lastAttackTime;

    [Header("Idle Settings (ยืนหันซ้ายขวา)")]
    public float minIdleTime = 1f; // สุ่มเวลาน้อยสุดที่จะหัน
    public float maxIdleTime = 3f; // สุ่มเวลามากสุดที่จะหัน
    private float idleTimer;

    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRender;
    private Transform player;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRender = GetComponent<SpriteRenderer>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool isCooldown = Time.time < lastAttackTime + attackCooldown;
        bool isLowHealth = currentHealth <= runAwayHealth;

        // --- ระบบ AI โจมตีใหม่ ---
        if (distanceToPlayer <= attackRange)
        {
            Idle(); // หยุดเดินเพื่อเตรียมตี

            bool playerIsRight = player.position.x > transform.position.x;
            FlipSprite(playerIsRight);

            if (!isCooldown) 
            {
                AttackPlayer(); // โจมตี
            }
        }
        else if (isLowHealth || (isCooldown && distanceToPlayer < retreatRange))
        {
            RetreatFromPlayer();
        }
        else if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer();
        }
        else
        {
            Idle();
        }

        if (anim != null) anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
    }

    void AttackPlayer()
    {
        lastAttackTime = Time.time;

        // สุ่มตัวเลขตั้งแต่ 0.0 ถึง 1.0
        float randomVal = Random.value; 

        if (randomVal <= stunChance)
        {
            // --- โจมตีติดสตัน ---
            if (anim != null) anim.SetTrigger("isAttackStun"); // เล่นท่าสตัน
            
            Health hp = player.GetComponent<Health>();
            if (hp != null) hp.TakeDamage(attackDamage);

            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null) pc.ApplyStun(stunDuration); // สั่งสตันฮีโร่
            
            Debug.Log("มอนสเตอร์ใช้ท่าสตัน!");
        }
        else
        {
            // --- โจมตีธรรมดา ---
            if (anim != null) anim.SetTrigger("isAttack"); // เล่นท่าตีธรรมดา
            
            Health hp = player.GetComponent<Health>();
            if (hp != null) hp.TakeDamage(attackDamage);
            
            Debug.Log("มอนสเตอร์ตีธรรมดา");
        }
    }

    void Idle() 
    { 
        // สั่งให้หยุดเดิน
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); 

        // ระบบนับเวลาถอยหลังเพื่อหันหน้า
        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0)
        {
            // พลิกหน้าไปอีกฝั่ง
            if (spriteRender != null) spriteRender.flipX = !spriteRender.flipX; 
            
            // สุ่มเวลาขึ้นมาใหม่ว่าจะยืนแช่อีกกี่วินาที
            idleTimer = Random.Range(minIdleTime, maxIdleTime); 
        }
    }

    void ChasePlayer()
    {
        bool playerIsRight = player.position.x > transform.position.x;
        rb.linearVelocity = new Vector2(playerIsRight ? chaseSpeed : -chaseSpeed, rb.linearVelocity.y);
        FlipSprite(playerIsRight);
    }

    void RetreatFromPlayer()
    {
        bool playerIsRight = player.position.x > transform.position.x;
        rb.linearVelocity = new Vector2(playerIsRight ? -retreatSpeed : retreatSpeed, rb.linearVelocity.y);
        FlipSprite(!playerIsRight); 
    }

    void FlipSprite(bool faceRight)
    {
        if (spriteRender != null)
        {
            // === [ส่วนที่ต้องแก้] เปลี่ยนจาก !faceRight เป็น faceRight เฉยๆ ===
            spriteRender.flipX = faceRight; 
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;
        currentHealth -= damageAmount;
        lastAttackTime = 0f; 

        if (currentHealth <= 0) Die();
        else if (anim != null) anim.SetTrigger("isHit");
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero; 
        if (anim != null) anim.SetTrigger("isDead");
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange); 
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, retreatRange); 
        
        // เพิ่มเส้นสีแดง เพื่อดูระยะโจมตีใน Scene
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange); 
    }
}