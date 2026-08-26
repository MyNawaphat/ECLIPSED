using UnityEngine;
using System.Collections;

public class FlyingEnemy : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    private bool isDead = false;
    private bool isSwooping = false; // กำลังโฉบอยู่หรือไม่?

    [Header("ระบบบินลาดตระเวน (Patrol)")]
    public float patrolSpeed = 2f;    // ความเร็วตอนบินวน
    public float patrolDistance = 3f; // ระยะทางที่จะบินวนซ้ายขวา
    private float leftEdge;
    private float rightEdge;
    private bool movingRight = true;

    [Header("ระยะและการพุ่งโจมตี")]
    public float detectionRange = 8f; // ระยะที่จะเห็นฮีโร่
    public float swoopSpeed = 8f;     // ความเร็วตอนพุ่งโฉบ
    public float returnSpeed = 4f;    // ความเร็วตอนบินกลับขึ้นฟ้า
    public float attackCooldown = 3f; // รอคูลดาวน์ก่อนพุ่งรอบต่อไป
    public int attackDamage = 15;     // พลังโจมตี

    private float originalY;
    private Transform player;
    private Animator anim;
    private SpriteRenderer spriteRender;
    private float lastAttackTime;

    void Start()
    {
        currentHealth = maxHealth;
        
        // จำความสูงและขอบเขตซ้ายขวาตอนเริ่มเกม
        originalY = transform.position.y; 
        leftEdge = transform.position.x - patrolDistance;
        rightEdge = transform.position.x + patrolDistance;

        anim = GetComponent<Animator>();
        spriteRender = GetComponent<SpriteRenderer>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (isDead || player == null || isSwooping) return;

        float dist = Vector2.Distance(transform.position, player.position);
        
        // 1. ถ้าฮีโร่เข้ามาในระยะ ให้พุ่งโฉบ!
        if (dist <= detectionRange)
        {
            // หันหน้าจ้องฮีโร่
            spriteRender.flipX = player.position.x > transform.position.x;

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                StartCoroutine(SwoopAttack());
            }
        }
        // 2. ถ้าฮีโร่อยู่ไกล ให้บินลาดตระเวนไปมา
        else
        {
            Patrol(); 
        }
    }

    void Patrol()
    {
        if (movingRight)
        {
            transform.position += Vector3.right * patrolSpeed * Time.deltaTime;
            spriteRender.flipX = true;
            if (transform.position.x >= rightEdge) movingRight = false; // ถึงขอบขวาให้หันกลับ
        }
        else
        {
            transform.position += Vector3.left * patrolSpeed * Time.deltaTime;
            spriteRender.flipX = false;
            if (transform.position.x <= leftEdge) movingRight = true; // ถึงขอบซ้ายให้หันกลับ
        }
    }

    IEnumerator SwoopAttack()
    {
        isSwooping = true;
        Vector2 targetPos = player.position; // เล็งเป้าตำแหน่งฮีโร่

        if (anim != null) anim.SetTrigger("isSmash"); 
        
        bool hasDealtDamage = false; // ตัวล็อก: ป้องกันไม่ให้ตีเลือดฮีโร่ลดรัวๆ ในการโฉบ 1 ครั้ง

        // พุ่งลงมาหาฮีโร่ (ตั้งระยะให้ทะลุเข้าไปใกล้ๆ เป็น 0.1f จะได้ชนชัวร์ๆ)
        while (Vector2.Distance(transform.position, targetPos) > 0.1f && !isDead)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, swoopSpeed * Time.deltaTime);
            
            // 💡 ไม้งัดไม้ตาย: ใช้การวัดระยะทางแทนกล่องชน!
            // ถ้านกระยะห่างจากฮีโร่น้อยกว่า 1.5 หน่วย (แปลว่าตัวแทบจะทับกันแล้ว) ให้สั่งลดเลือดเลย!
            if (!hasDealtDamage && Vector2.Distance(transform.position, player.position) <= 1.5f)
            {
                Health hp = player.GetComponent<Health>();
                if (hp != null) 
                {
                    hp.TakeDamage(attackDamage); // ฮีโร่เลือดลด!
                    hasDealtDamage = true;       // จำไว้ว่าตีโดนแล้ว จะได้ไม่ตีซ้ำจนเลือดหมดหลอด
                }
            }
            
            yield return null;
        }
        
        yield return new WaitForSeconds(0.5f); // ค้างอยู่พื้นแป๊บนึง

        // บินกลับขึ้นไปความสูงเดิม
        Vector2 returnPos = new Vector2(transform.position.x, originalY);
        
        // อัปเดตขอบเขตซ้ายขวาใหม่ 
        leftEdge = transform.position.x - patrolDistance;
        rightEdge = transform.position.x + patrolDistance;

        while (Vector2.Distance(transform.position, returnPos) > 0.1f && !isDead)
        {
            transform.position = Vector2.MoveTowards(transform.position, returnPos, returnSpeed * Time.deltaTime);
            yield return null;
        }

        lastAttackTime = Time.time;
        isSwooping = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return; // ถ้าตายแล้วไม่ต้องรับดาเมจซ้ำ
        
        currentHealth -= damage; // เลือดลดตามพลังโจมตีของดาบ
        
        if (currentHealth <= 0) 
        {
            Die(); // ถ้าเลือดหมดหลอดให้ตาย
        }
        else if (anim != null) 
        {
            anim.SetTrigger("isHit"); // ถ้าเลือดยังไม่หมด ให้กระตุกเจ็บ
        }
    }

    void Die()
    {
        isDead = true;
        
        // 1. แก้บั๊กศพจมดิน: สั่งหยุดการเคลื่อนที่ทั้งหมด และปิดแรงโน้มถ่วงทันทีที่ตาย
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // สั่งเบรกหยุดนิ่ง
            rb.gravityScale = 0f;             // ปิดแรงโน้มถ่วงไม่ให้ร่วงทะลุพื้น
        }

        // เล่นแอนิเมชันตาย
        if (anim != null) anim.SetTrigger("isDead");
        
        // ปิดกล่องชน ฮีโร่จะได้เดินผ่านศพไปได้
        GetComponent<Collider2D>().enabled = false;
        
        // ทำลายศพทิ้งใน 2 วินาที
        Destroy(gameObject, 2f);
    }

    // ฟังก์ชันนี้เรียกใช้ในหมุด Animation Event ได้เลย เพื่อให้ทำดาเมจใส่ฮีโร่ตอนโฉบถึงตัว
    public void Event_DealNormalDamage()
    {
        if (player == null) return;
        if (Vector2.Distance(transform.position, player.position) <= 2.5f)
        {
            Health hp = player.GetComponent<Health>();
            if (hp != null) hp.TakeDamage(attackDamage);
        }
    }
}