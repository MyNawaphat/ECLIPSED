using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public Image healthBar; // ลาก UI หลอดสีแดงมาใส่

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    // ฟังก์ชันสำหรับรับดาเมจ (ให้มอนสเตอร์เรียกใช้ฟังก์ชันนี้)
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();

        if (currentHealth <= 0)
        {
            Debug.Log("Player ตายแล้ว!");
            // ใส่โค้ดตอนตายที่นี่
        }
    }

    // ฟังก์ชันสำหรับเพิ่มเลือด (ให้ระบบถ้วยน้ำชาเรียกใช้)
    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = currentHealth / maxHealth;
        }
    }
}