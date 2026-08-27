using UnityEngine;
using System.Collections;

public class BringerEnemy : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    private bool isDead = false;

    // 🔴 1. เพิ่มตัวแปรระยะมองเห็นตรงนี้
    [Header("ระยะมองเห็นและโจมตี")]
    public float detectionRange = 15f; // ระยะที่จะเริ่มเดินตาม (ปรับได้ใน Inspector)
    public float spellRange = 8f;   
    public float meleeRange = 2f;   
    public float moveSpeed = 3f;

    public float attackCooldown = 2.5f;
    private float lastAttackTime;

    private Transform player;
    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRender;

    [Header("Damage Settings")]
    public float meleeDamage = 20f; 
    public float spellDamage = 30f;

    [Header("เวทมนตร์ (Spell)")]
    public GameObject spellPrefab;
    private GameObject currentActiveSpell;

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

        if (currentActiveSpell != null)
        {
            lastAttackTime = Time.time;
        }

        float dist = Vector2.Distance(transform.position, player.position);
        bool canAttack = Time.time >= lastAttackTime + attackCooldown;

        // โจมตีระยะใกล้ (ฟันดาบ)
        if (dist <= meleeRange)
        {
            spriteRender.flipX = player.position.x > transform.position.x; // หันหน้า
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // หยุดเดิน
            if (canAttack)
            {
                anim.SetTrigger("isAttack");
                lastAttackTime = Time.time;
            }
        }
        // โจมตีระยะไกล (ร่ายเวท)
        else if (dist <= spellRange)
        {
            spriteRender.flipX = player.position.x > transform.position.x; // หันหน้า
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // หยุดเดิน
            
            if (canAttack && currentActiveSpell == null) 
            {
                anim.SetTrigger("isCast"); 
                lastAttackTime = Time.time;
            }
        }
        // 🔴 2. ถ้าไม่ได้โจมตี ให้เช็กก่อนว่า "อยู่ในระยะมองเห็นไหม?"
        else if (dist <= detectionRange) 
        {
            spriteRender.flipX = player.position.x > transform.position.x; // หันหน้า
            float direction = player.position.x > transform.position.x ? 1 : -1;
            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        }
        // 🔴 3. ถ้าอยู่นอกระยะมองเห็น (เช่น ผู้เล่นอยู่ด่านอื่น) -> ให้ยืนนิ่งๆ
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

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

    public void Event_SpawnSpell()
    {
        if (player != null && spellPrefab != null)
        {
            Vector3 spawnPos = player.position + new Vector3(0, 2.9f, 0); 
            currentActiveSpell = Instantiate(spellPrefab, spawnPos, Quaternion.identity);
            StartCoroutine(SpellDamageDelay(spawnPos)); 
        }
    }

    public void Event_MeleeHit()
    {
        if (player != null && Vector2.Distance(transform.position, player.position) <= meleeRange)
        {
            Health hp = player.GetComponent<Health>();
            if (hp != null) hp.TakeDamage(meleeDamage); 
        }
    }

    IEnumerator SpellDamageDelay(Vector3 targetPos)
    {
        yield return new WaitForSeconds(1.0f);

        if (player != null)
        {
            Vector3 playerCenter = player.position + new Vector3(0, 2.5f, 0);
            float distance = Vector3.Distance(targetPos, playerCenter);

            if (distance <= 2.0f) 
            {
                Health hp = player.GetComponent<Health>();
                if (hp != null) hp.TakeDamage(spellDamage); 
            }
        }
    }

    // 🔴 4. เพิ่มเส้นวาดระยะมองเห็นใน Scene (ช่วยให้คุณกะระยะง่ายขึ้นเยอะครับ)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange); // วาดเส้นสีเหลืองบอกระยะมองเห็น
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spellRange);     // วาดเส้นสีแดงบอกระยะร่ายเวท
    }
}