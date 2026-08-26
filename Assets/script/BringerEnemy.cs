using UnityEngine;

public class BringerEnemy : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    private bool isDead = false;

    [Header("ระยะโจมตี")]
    public float spellRange = 8f;   // ระยะร่ายคาถา (ไกล)
    public float meleeRange = 2f;   // ระยะฟันดาบ (ใกล้)
    public float moveSpeed = 3f;

    public float attackCooldown = 2.5f;
    private float lastAttackTime;

    private Transform player;
    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRender;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRender = GetComponent<SpriteRenderer>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        bool canAttack = Time.time >= lastAttackTime + attackCooldown;

        // หันหน้า
        spriteRender.flipX = player.position.x > transform.position.x;

        // 1. ถ้าอยู่ใกล้มาก -> ฟันดาบ (Melee)
        if (dist <= meleeRange)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // หยุดเดิน
            if (canAttack)
            {
                anim.SetTrigger("isAttack");
                lastAttackTime = Time.time;
            }
        }
        // 2. ถ้าอยู่ไกลแต่อยู่ในระยะเวทมนตร์ -> ร่ายคาถา (Spell)
        else if (dist <= spellRange)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // หยุดเดิน
            if (canAttack)
            {
                anim.SetTrigger("isSpell"); 
                lastAttackTime = Time.time;
            }
        }
        // 3. ถ้าอยู่นอกระยะเวทมนตร์ -> เดินตาม
        else 
        {
            float direction = player.position.x > transform.position.x ? 1 : -1;
            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        }

        // ส่งค่าความเร็วเดินไปให้อนิเมชัน
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        if (currentHealth <= 0) Die();
        else anim.SetTrigger("isHit");
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        anim.SetTrigger("isDead");
        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 3f);
    }
}