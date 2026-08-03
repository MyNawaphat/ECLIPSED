using UnityEngine;
using UnityEngine.UI;

public class Stamina : MonoBehaviour
{
    [Header("Stamina Settings")]
    public float maxStamina = 100f; //[cite: 6]
    public float currentStamina; //[cite: 6]
    public float staminaRegenRate = 5f; //[cite: 6]
    public Image staminaBar; //[cite: 6]

    void Start()
    {
        currentStamina = maxStamina; //[cite: 6]
        UpdateUI(); //[cite: 6]
    }

    void Update()
    {
        if (currentStamina < maxStamina) //[cite: 6]
        {
            currentStamina += staminaRegenRate * Time.deltaTime; //[cite: 6]
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina); //[cite: 6]
            UpdateUI(); //[cite: 6]
        }
    }

    public bool UseStamina(float amount) //[cite: 6]
    {
        if (currentStamina >= amount) //[cite: 6]
        {
            currentStamina -= amount; //[cite: 6]
            UpdateUI(); //[cite: 6]
            return true; //[cite: 6]
        }
        else //[cite: 6]
        {
            Debug.Log("Stamina ไม่พอ!"); //[cite: 6]
            return false; //[cite: 6]
        }
    }

    // === [ส่วนที่เพิ่มเข้ามาใหม่] ฟังก์ชันสำหรับรับพลังงานจากถ้วยชา ===
    public void RestoreStamina(float amount)
    {
        currentStamina += amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina); // ป้องกันพลังงานล้นหลอด
        UpdateUI();
    }
    // =======================================================

    private void UpdateUI() //[cite: 6]
    {
        if (staminaBar != null) //[cite: 6]
        {
            staminaBar.fillAmount = currentStamina / maxStamina; //[cite: 6]
        }
    }
}