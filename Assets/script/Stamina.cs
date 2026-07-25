using UnityEngine;
using UnityEngine.UI;

public class Stamina : MonoBehaviour
{
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaRegenRate = 5f;
    public Image staminaBar;

    void Start()
    {
        currentStamina = maxStamina;
        UpdateUI();
    }

    void Update()
    {
        // ฟื้นฟูพลังงานอัตโนมัติ
        if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
            UpdateUI();
        }
    }

    // ฟังก์ชันสำหรับใช้พลังงาน (เช็คว่าพลังงานพอไหม)
    public bool UseStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            UpdateUI();
            return true; // อนุญาตให้โจมตี/กระโดดได้
        }
        else
        {
            Debug.Log("Stamina ไม่พอ!");
            return false; // ไม่อนุญาตให้ทำแอ็กชัน
        }
    }

    private void UpdateUI()
    {
        if (staminaBar != null)
        {
            staminaBar.fillAmount = currentStamina / maxStamina;
        }
    }
}