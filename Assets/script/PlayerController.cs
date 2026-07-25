using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 7f; // ความเร็วตอนวิ่ง (ปรับตัวเลขใน Unity ได้เลย)
    public float jumpForce = 7f; // ความแรงในการกระโดด

    [Header("Attack Settings")]
    public Transform attackPoint;      // จุดศูนย์กลางการฟัน (ลาก AttackPoint มาใส่)
    public float attackRange = 0.5f;   // รัศมีความกว้างของดาบ
    public LayerMask enemyLayers;      // กำหนดว่าฟันโดน Layer ไหนได้บ้าง
    public int attackDamage = 40;

    [Header("Status Effects")]
    public bool isStunned = false;

    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRender;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRender = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // 1. รับค่าการกดปุ่มซ้าย-ขวา
        float moveInput = Input.GetAxisRaw("Horizontal");

        // 2. เคลื่อนที่ด้วยความเร็ววิ่งทันที
        // แก้จาก rb.velocity เป็น rb.linearVelocity
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // 3. ควบคุมอนิเมชัน
        if (moveInput != 0)
        {
            // ถ้ามีการเคลื่อนที่ ให้ส่งค่าไปให้ระบบเล่นท่าวิ่ง
            anim.SetFloat("Speed", 1f);
        }
        else
        {
            // ถ้ายืนนิ่ง ให้ส่งค่า 0 เพื่อกลับไปท่า Idel
            anim.SetFloat("Speed", 0f);
        }

        // 4. หันหน้าตัวละครซ้าย-ขวา
        if (moveInput > 0) 
        {
            spriteRender.flipX = false;
            // ย้ายจุดฟันมาฝั่งขวา (ค่าบวก)
            if (attackPoint != null) 
            {
                attackPoint.localPosition = new Vector3(Mathf.Abs(attackPoint.localPosition.x), attackPoint.localPosition.y, attackPoint.localPosition.z);
            }
        }
        else if (moveInput < 0) 
        {
            spriteRender.flipX = true;
            // ย้ายจุดฟันไปฝั่งซ้าย (ค่าลบ)
            if (attackPoint != null) 
            {
                attackPoint.localPosition = new Vector3(-Mathf.Abs(attackPoint.localPosition.x), attackPoint.localPosition.y, attackPoint.localPosition.z);
            }
        }
        // --- เช็คพื้น ---
        // แก้จาก rb.velocity.y เป็น rb.linearVelocity.y
        bool isGrounded = Mathf.Abs(rb.linearVelocity.y) < 0.001f;

        // 5. ระบบโจมตี
        // ท่าฟัน 1 (ปุ่ม K) : บังคับว่าต้องเหยียบพื้นอยู่ถึงจะฟันได้
        if (Input.GetKeyDown(KeyCode.K) && isGrounded) 
        {
            anim.SetTrigger("Attack"); 
            AttackEnemy(); // เรียกใช้คำสั่งลดเลือดศัตรู
        }

        // ท่าฟัน 2 (ปุ่ม J)
        if (Input.GetKeyDown(KeyCode.J)) 
        {
            anim.SetTrigger("Attack2"); 
            AttackEnemy(); // เรียกใช้คำสั่งลดเลือดศัตรู
        }

        // 6. ระบบกระโดดด้วย Spacebar
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            // เพิ่มแรงผลักขึ้นไปด้านบนเพื่อให้ตัวละครกระโดด
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
        }

        if (isStunned)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            anim.SetFloat("Speed", 0f);
            return; 
        }

        // 7. ส่งค่าไปให้ Animator ควบคุมท่ากระโดดและร่วง
        // แก้จาก rb.velocity.y เป็น rb.linearVelocity.y
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("Floating", rb.linearVelocity.y);
    }

    void AttackEnemy()
    {
        // สร้างวงกลมล่องหนเพื่อเช็คว่ามีศัตรูอยู่ในระยะไหม
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        // สั่งลดเลือดศัตรูทุกตัวที่โดนฟัน
        foreach (Collider2D enemy in hitEnemies)
        {
            // เรียกใช้คำสั่ง TakeDamage จากสคริปต์ของเห็ด
            enemy.GetComponent<EnemyController>().TakeDamage(attackDamage);
        }
    }

    // ฟังก์ชันพิเศษสำหรับวาดวงกลมสีแดงในหน้า Scene ให้เราเห็นระยะฟันได้ง่ายๆ
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    public void ApplyStun(float stunTime)
    {
        if (!isStunned)
        {
            StartCoroutine(StunRoutine(stunTime));
        }
    }

    private IEnumerator StunRoutine(float stunTime)
    {
        isStunned = true;
        Debug.Log("ฮีโร่ติดสตันขยับไม่ได้!");
        
        // (ตัวเลือกเสริม) สั่งเปลี่ยนสีฮีโร่เป็นสีเทาหรือเหลืองตอนติดสตันได้ตรงนี้

        yield return new WaitForSeconds(stunTime); // รอเวลาตามที่กำหนด

        isStunned = false;
        Debug.Log("หายสตันแล้ว!");
    }
}