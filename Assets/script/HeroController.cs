using UnityEngine;

public class HeroController : MonoBehaviour
{
    public float moveSpeed = 7f; //[cite: 4]

    private Animator anim; //[cite: 4]
    private Rigidbody2D rb; //[cite: 4]
    private SpriteRenderer spriteRender; //[cite: 4]

    void Start()
    {
        anim = GetComponent<Animator>(); //[cite: 4]
        rb = GetComponent<Rigidbody2D>(); //[cite: 4]
        spriteRender = GetComponent<SpriteRenderer>(); //[cite: 4]
    }

    void Update()
    {
        float moveInput = Input.GetAxisRaw("Horizontal"); //[cite: 4]
        // เปลี่ยนบรรทัดนี้ใน HeroController
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        anim.SetFloat("Speed", Mathf.Abs(moveInput)); //[cite: 4]

        if (moveInput > 0) //[cite: 4]
        {
            spriteRender.flipX = false; //[cite: 4]
        }
        else if (moveInput < 0) //[cite: 4]
        {
            spriteRender.flipX = true;  //[cite: 4]
        }
        
        // แก้ไขตรงนี้: เปลี่ยน KeyCode.k เป็น KeyCode.K (ตัว K พิมพ์ใหญ่)
        if (Input.GetKeyDown(KeyCode.K)) //[cite: 4]
        {
            anim.SetTrigger("Attack"); //[cite: 4]
        }
    }
}