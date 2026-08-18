using UnityEngine;
using System.Collections;

// บังคับว่าฮีโร่ต้องมีสคริปต์ Stamina ติดอยู่ด้วย
[RequireComponent(typeof(Stamina))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 7f; 
    public float jumpForce = 7f; 

    [Header("Attack Settings")]
    public Transform attackPoint;      
    public float attackRange = 0.5f;   
    public LayerMask enemyLayers;      
    public int attackDamage = 40;
    public float attackCooldown = 0.5f; // หน่วงเวลาฟัน (0.5 วินาทีฟันได้ 1 ครั้ง)
    private float nextAttackTime = 0f;   

    [Header("Stamina Costs (ใช้พลังงานเท่าไหร่)")]
    public float jumpStaminaCost = 15f; 
    public float attackStaminaCost = 10f;

    [Header("Status Effects")]
    public bool isStunned = false; 
    public GameObject stunEffect; // ช่องสำหรับใส่รูปดาวหมุน

    private Animator anim; 
    private Rigidbody2D rb; 
    private SpriteRenderer spriteRender; 
    
    // ประกาศตัวแปรเรียกใช้ Stamina
    private Stamina staminaSystem;

    [Header("Ground Check (ระบบเช็กพื้น)")]
    public Transform groundCheck;     // ใส่จุด GroundCheck ที่เท้า
    public float groundCheckRadius = 0.2f; // รัศมีเรดาร์
    public LayerMask groundLayer;     // กำหนดว่าอะไรคือพื้น

    void Start()
    {
        anim = GetComponent<Animator>(); 
        rb = GetComponent<Rigidbody2D>(); 
        spriteRender = GetComponent<SpriteRenderer>(); 
        
        // ดึงคอมโพเนนต์ Stamina มาใช้งาน
        staminaSystem = GetComponent<Stamina>();
    }

    void Update()
    {
        // ==========================================
        // 1. ดักไว้ก่อนเลย: ถ้าเลือดหมด ให้เด้งออกทันทีไม่ต้องทำอะไรต่อ
        // ==========================================
        Health hp = GetComponent<Health>();
        if (hp != null && hp.currentHealth <= 0) 
        {
            return; 
        }

        if (isStunned)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); 
            anim.SetFloat("Speed", 0f); 
            return;  
        }

        float moveInput = Input.GetAxisRaw("Horizontal"); 
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y); 

        if (moveInput != 0) anim.SetFloat("Speed", 1f); 
        else anim.SetFloat("Speed", 0f); 

        if (moveInput > 0) 
        {
            spriteRender.flipX = false; 
            if (attackPoint != null) attackPoint.localPosition = new Vector3(Mathf.Abs(attackPoint.localPosition.x), attackPoint.localPosition.y, attackPoint.localPosition.z); 
        }
        else if (moveInput < 0) 
        {
            spriteRender.flipX = true; 
            if (attackPoint != null) attackPoint.localPosition = new Vector3(-Mathf.Abs(attackPoint.localPosition.x), attackPoint.localPosition.y, attackPoint.localPosition.z); 
        }
        
        bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // --- ระบบโจมตี (บังคับ && isGrounded กลับมาแล้ว) ---
        if (Input.GetKeyDown(KeyCode.K) && isGrounded) 
        {
            if (Time.time >= nextAttackTime && staminaSystem.UseStamina(attackStaminaCost))
            {
                anim.SetTrigger("Attack"); 
                nextAttackTime = Time.time + attackCooldown;
            }
        }

        if (Input.GetKeyDown(KeyCode.J) && isGrounded) 
        {
            if (Time.time >= nextAttackTime && staminaSystem.UseStamina(attackStaminaCost))
            {
                anim.SetTrigger("Attack2"); 
                nextAttackTime = Time.time + attackCooldown;
            }
        }

        // --- ระบบกระโดด (หัก Stamina ก่อนกระโดด) ---
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            if (staminaSystem.UseStamina(jumpStaminaCost))
            {
                rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse); 
            }
        }

        anim.SetBool("isGrounded", isGrounded); 
        anim.SetFloat("Floating", rb.linearVelocity.y); 
    }

    public void AttackEnemy()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers); 
        foreach (Collider2D enemy in hitEnemies) 
        {
            // โจมตีโดนลูกกระจ๊อก
            EnemyController enemyCtrl = enemy.GetComponent<EnemyController>();
            if (enemyCtrl != null) enemyCtrl.TakeDamage(attackDamage);

            BossController bossCtrl = enemy.GetComponent<BossController>();
            if (bossCtrl != null) bossCtrl.TakeDamage(attackDamage);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return; 
        Gizmos.DrawWireSphere(attackPoint.position, attackRange); 
    }

    public void ApplyStun(float stunTime) 
    {
        if (!isStunned) StartCoroutine(StunRoutine(stunTime)); 
    }

    private IEnumerator StunRoutine(float stunTime) 
    {
        isStunned = true; 
        
        // 1. เปิดให้ดาวโชว์ขึ้นมาบนหัว
        if (stunEffect != null) stunEffect.SetActive(true);

        yield return new WaitForSeconds(stunTime); 
        
        isStunned = false; 
        
        // 2. ปิดดาวทิ้งเมื่อหายมึน
        if (stunEffect != null) stunEffect.SetActive(false);
    }
}