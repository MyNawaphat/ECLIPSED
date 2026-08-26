using UnityEngine;
using System.Collections;

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

    [Header("Damage Settings")]
    public float meleeDamage = 20f; // ดาเมจตอนฟันดาบ
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

        // 1. เช็กคาถาเก่าก่อน: ถ้าคาถายังอยู่ ให้รีเซ็ตเวลาคูลดาวน์ไว้
        if (currentActiveSpell != null)
        {
            lastAttackTime = Time.time;
        }

        // 2. ประกาศตัวแปรแค่ "ครั้งเดียว" (เอามาไว้หลังเช็กคาถา จะได้คำนวณคูลดาวน์แม่นๆ)
        float dist = Vector2.Distance(transform.position, player.position);
        bool canAttack = Time.time >= lastAttackTime + attackCooldown;

        // 3. หันหน้า
        spriteRender.flipX = player.position.x > transform.position.x;

        // 4. โจมตีระยะใกล้ (ฟันดาบ)
        if (dist <= meleeRange)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // หยุดเดิน
            if (canAttack)
            {
                anim.SetTrigger("isAttack");
                lastAttackTime = Time.time;
            }
        }
        // 5. โจมตีระยะไกล (ร่ายเวท)
        else if (dist <= spellRange)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // หยุดเดิน
            
            if (canAttack && currentActiveSpell == null) 
            {
                anim.SetTrigger("isCast"); 
                lastAttackTime = Time.time;
            }
        }
        // 6. ถ้าอยู่นอกระยะโจมตี -> ให้เดินตาม (ในโค้ดที่คุณส่งมาตรงนี้หายไปครับ ผมเติมกลับมาให้แล้ว)
        else 
        {
            float direction = player.position.x > transform.position.x ? 1 : -1;
            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        }

        // 7. ส่งค่าความเร็วเดินไปให้ Animator
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
            // 1. คำนวณตำแหน่งและเสกคาถาออกมาให้เห็นก่อน (สมมติว่าปรับความสูงไว้ที่ 2.5f)
            Vector3 spawnPos = player.position + new Vector3(0, 2.9f, 0); 
            currentActiveSpell = Instantiate(spellPrefab, spawnPos, Quaternion.identity);
            
            // 2. 🔴 ลบโค้ดหักเลือดเดิมออก แล้วสั่งเริ่มระบบจับเวลาทำดาเมจ โดยส่งตำแหน่งที่คาถาลงไปให้ระบบจำไว้
            StartCoroutine(SpellDamageDelay(spawnPos)); 
        }
    }
    // ➕ ฟังก์ชันนี้เอาไว้เรียกตอนจังหวะดาบฟันโดนตัว
    public void Event_MeleeHit()
    {
        // เช็กว่าฮีโร่อยู่ในระยะฟันหรือไม่ (ป้องกันบั๊กฮีโร่เดินหนีพ้นแล้วแต่ยังโดนดาเมจ)
        if (player != null && Vector2.Distance(transform.position, player.position) <= meleeRange)
        {
            Health hp = player.GetComponent<Health>();
            if (hp != null) hp.TakeDamage(meleeDamage); // หักเลือดฮีโร่
        }
    }

    IEnumerator SpellDamageDelay(Vector3 targetPos)
    {
        // 1. ⏱️ หน่วงเวลาก่อนทำดาเมจ (ปรับเลขตรงนี้ให้ตรงกับจังหวะที่มือสีดำบีบลงมา เช่น 0.8f หรือ 1.0f วินาที)
        yield return new WaitForSeconds(1.0f);

        if (player != null)
        {
            // 2. คำนวณจุดกึ่งกลางของฮีโร่ปัจจุบัน เพื่อเอามาวัดระยะ
            Vector3 playerCenter = player.position + new Vector3(0, 2.5f, 0);
            
            // 3. วัดระยะห่างระหว่าง "จุดที่คาถาลง" กับ "ตำแหน่งฮีโร่ปัจจุบัน"
            float distance = Vector3.Distance(targetPos, playerCenter);

            // 4. 🏃‍♂️ ถ้าระยะห่างน้อยกว่า 2.5 หน่วย แปลว่าฮีโร่เดินหนีไม่พ้นรัศมีวงเวท! (ปรับความกว้างรัศมีได้ที่เลข 2.5f)
            if (distance <= 2.0f) 
            {
                Health hp = player.GetComponent<Health>();
                if (hp != null) hp.TakeDamage(spellDamage); 
            }
        }
}
}
