using UnityEngine;

public class SpellController : MonoBehaviour
{
    // ฟังก์ชันนี้จะรอรับคำสั่งจาก Animation
    public void Event_DestroyMe()
    {
        Destroy(gameObject);
    }
}