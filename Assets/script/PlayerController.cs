using UnityEngine;
using System.Collections;

// บังคับว่าฮีโร่ต้องมีสคริปต์ Stamina ติดอยู่ด้วย
[RequireComponent(typeof(Stamina))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 7f; // ความเร็วตอนวิ่ง[cite: 5]
    public float jumpForce = 7f; // ความแรงในการกระโดด[cite: 5]

    [Header("Attack Settings")]
    public Transform attackPoint;      // จุดศูนย์กลางการฟัน[cite: 5]
    public float attackRange = 0.5f;   // รัศมีความกว้างของดาบ[cite: 5]
    public LayerMask enemyLayers;      // กำหนดว่าฟันโดน Layer ไหนได้บ้าง[cite: 5]
    public int attackDamage = 40;      //[cite: 5]

    [Header("Stamina Costs (ใช้พลังงานเท่าไหร่)")]
    public float jumpStaminaCost = 15f; 
    public float attackStaminaCost = 10f;

    [Header("Status Effects")]
    public bool isStunned = false; //[cite: 5]

    private Animator anim; //[cite: 5]
    private Rigidbody2D rb; //[cite: 5]
    private SpriteRenderer spriteRender; //[cite: 5]
    
    // ประกาศตัวแปรเรียกใช้ Stamina
    private Stamina staminaSystem;

    void Start()
    {
        anim = GetComponent<Animator>(); //[cite: 5]
        rb = GetComponent<Rigidbody2D>(); //[cite: 5]
        spriteRender = GetComponent<SpriteRenderer>(); //[cite: 5]
        
        // ดึงคอมโพเนนต์ Stamina มาใช้งาน
        staminaSystem = GetComponent<Stamina>();
    }

    void Update()
    {
        float moveInput = Input.GetAxisRaw("Horizontal"); //[cite: 5]
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y); //[cite: 5]

        if (moveInput != 0) anim.SetFloat("Speed", 1f); //[cite: 5]
        else anim.SetFloat("Speed", 0f); //[cite: 5]

        if (moveInput > 0) 
        {
            spriteRender.flipX = false; //[cite: 5]
            if (attackPoint != null) attackPoint.localPosition = new Vector3(Mathf.Abs(attackPoint.localPosition.x), attackPoint.localPosition.y, attackPoint.localPosition.z); //[cite: 5]
        }
        else if (moveInput < 0) 
        {
            spriteRender.flipX = true; //[cite: 5]
            if (attackPoint != null) attackPoint.localPosition = new Vector3(-Mathf.Abs(attackPoint.localPosition.x), attackPoint.localPosition.y, attackPoint.localPosition.z); //[cite: 5]
        }
        
        bool isGrounded = Mathf.Abs(rb.linearVelocity.y) < 0.001f; //[cite: 5]

        // --- แก้ไขระบบโจมตี (หัก Stamina ก่อนฟัน) ---
        if (Input.GetKeyDown(KeyCode.K) && isGrounded) 
        {
            // เช็คว่าพลังงานพอไหม ถ้าพอก็ฟันได้
            if (staminaSystem.UseStamina(attackStaminaCost))
            {
                anim.SetTrigger("Attack"); //[cite: 5]
                AttackEnemy(); //[cite: 5]
            }
        }

        if (Input.GetKeyDown(KeyCode.J)) 
        {
            if (staminaSystem.UseStamina(attackStaminaCost))
            {
                anim.SetTrigger("Attack2"); //[cite: 5]
                AttackEnemy(); //[cite: 5]
            }
        }

        // --- แก้ไขระบบกระโดด (หัก Stamina ก่อนกระโดด) ---
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            if (staminaSystem.UseStamina(jumpStaminaCost))
            {
                rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse); //[cite: 5]
            }
        }

        if (isStunned)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); //[cite: 5]
            anim.SetFloat("Speed", 0f); //[cite: 5]
            return;  //[cite: 5]
        }

        anim.SetBool("isGrounded", isGrounded); //[cite: 5]
        anim.SetFloat("Floating", rb.linearVelocity.y); //[cite: 5]
        Health hp = GetComponent<Health>();
    if (hp != null && hp.currentHealth <= 0) 
    {
        return; // เด้งออกจาก Update ทันที ไม่สั่ง Animator เล่นท่า Idel หรือ Run อีก
    }
    }

    void AttackEnemy()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers); //[cite: 5]
        foreach (Collider2D enemy in hitEnemies) //[cite: 5]
        {
            enemy.GetComponent<EnemyController>().TakeDamage(attackDamage); //[cite: 5]
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return; //[cite: 5]
        Gizmos.DrawWireSphere(attackPoint.position, attackRange); //[cite: 5]
    }

    public void ApplyStun(float stunTime) //[cite: 5]
    {
        if (!isStunned) StartCoroutine(StunRoutine(stunTime)); //[cite: 5]
    }

    private IEnumerator StunRoutine(float stunTime) //[cite: 5]
    {
        isStunned = true; //[cite: 5]
        yield return new WaitForSeconds(stunTime); //[cite: 5]
        isStunned = false; //[cite: 5]
    }
}