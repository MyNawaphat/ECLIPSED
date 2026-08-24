using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Enemy Health")]
    public int maxHealth = 100;
    private int currentHealth;
    private bool isDead = false;

    [Header("AI Settings & Speeds")]
    public float chaseSpeed = 4f;     
    public float detectionRange = 7f; 

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

    [Header("ติ๊กถูกถ้ามอนสเตอร์หันผิดข้าง")]
    public bool invertFlip = false;

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

        // ทำให้มอนสเตอร์พร้อมโจมตีทันทีตั้งแต่เริ่มเกม
        lastAttackTime = -attackCooldown; 
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distanceToPlayer = Mathf.Abs(transform.position.x - player.position.x);
        bool isCooldown = Time.time < lastAttackTime + attackCooldown;

        // --- ระบบ AI โจมตีใหม่ (ดุดัน ไม่เดินหนี) ---
        if (distanceToPlayer <= attackRange)
        {
            // สั่งหยุดเดินตรงๆ แทนการเรียก Idle() เพื่อป้องกันบั๊กหันหน้ามั่ว
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            
            // หันหน้าจ้องฮีโร่เสมอ
            bool playerIsRight = player.position.x > transform.position.x;
            FlipSprite(playerIsRight);

            // โจมตีถ้าไม่ติดคูลดาวน์
            if (!isCooldown) 
            {
                AttackPlayer(); 
            }
        }
        else if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer(); // วิ่งไล่ตาม
        }
        else
        {
            Idle(); // ยืนเฝ้ายามหันซ้ายขวา
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
            // --- แค่สั่งง้างท่าสตัน (ยังไม่ลดเลือด) ---
            if (anim != null) anim.SetTrigger("isAttackStun"); 
            Debug.Log("มอนสเตอร์ง้างท่าสตัน!");
        }
        else
        {
            // --- แค่สั่งง้างตีธรรมดา (ยังไม่ลดเลือด) ---
            if (anim != null) anim.SetTrigger("isAttack"); 
            Debug.Log("มอนสเตอร์ง้างตีธรรมดา");
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

    void FlipSprite(bool faceRight)
{
    if (spriteRender != null)
    {
        if (invertFlip)
            spriteRender.flipX = !faceRight; 
        else
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
        
        // 1. หยุดการเคลื่อนไหวและปิดแรงโน้มถ่วง ศพจะได้ไม่ร่วงทะลุพื้น!
        rb.linearVelocity = Vector2.zero; 
        rb.gravityScale = 0f; 

        // 2. เล่นแอนิเมชันตาย
        if (anim != null) anim.SetTrigger("isDead");
        
        // 3. ปิดกล่องชน ฮีโร่จะได้เดินเหยียบผ่านศพไปได้
        GetComponent<Collider2D>().enabled = false;
        
        // 4. สั่งทำลายวัตถุนี้ทิ้ง (ให้ศพหายไป) ในอีก 3 วินาทีข้างหน้า (เปลี่ยนตัวเลข 3f ได้ตามต้องการ)
        Destroy(gameObject, 3f); 
        
        this.enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange); 
        
        // เพิ่มเส้นสีแดง เพื่อดูระยะโจมตีใน Scene
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange); 
    }

    public void Event_DealNormalDamage()
    {
        if (player == null) return;
        
        // ระยะโจมตียังถึงอยู่ไหม (ป้องกันบั๊กผู้เล่นวิ่งหนีพ้นแล้วแต่ยังโดนดาเมจ)
        if (Vector2.Distance(transform.position, player.position) <= attackRange + 0.5f)
        {
            Health hp = player.GetComponent<Health>();
            if (hp != null) hp.TakeDamage(attackDamage);
            Debug.Log("ดาเมจตีธรรมดาเข้าแล้ว!");
        }
    }

    // 2. ฟังก์ชันนี้เอาไว้เลือกในหมุดของคลิปอนิเมชัน "ตีสตัน"
    public void Event_DealStunDamage()
    {
        if (player == null) return;

        if (Vector2.Distance(transform.position, player.position) <= attackRange + 0.5f)
        {
            Health hp = player.GetComponent<Health>();
            if (hp != null) hp.TakeDamage(attackDamage);

            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null) pc.ApplyStun(stunDuration); // สั่งสตันฮีโร่
            
            Debug.Log("ดาเมจสตันเข้าแล้ว!");
        }
    }
}