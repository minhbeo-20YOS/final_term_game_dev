using UnityEngine;

public class BossController : MonoBehaviour
{
    // Bỏ biến toàn cục bossAnimator cũ đi, mình sẽ tìm trực tiếp khi tung chiêu

    [Header("--- Damage Settings ---")]
    public float baseDamagePerMiss = 5f; // Sát thương gốc ở Day 1

    void Start()
    {
        // XÓA SẠCH CODE Ở ĐÂY: Không cache cứng ở Start nữa để tránh bị Unity đánh lừa
    }

    void OnEnable()
    {
        Conductor.OnBeat += CheckPlayerRhythm;
    }

    void OnDisable()
    {
        Conductor.OnBeat -= CheckPlayerRhythm;
    }

    void CheckPlayerRhythm(int totalBeats, int beatInBar)
    {
        // Để trống sạch sẽ cho cơ chế mũi tên mới
    }
    
    public float GetCurrentDayDamage()
    {
        return baseDamagePerMiss + (GameData.CurrentDay - 1) * 5f;
    }

    // Hàm này được NewGameplayManager gọi trực tiếp khi người chơi lỡ nốt
    public void AttackAndInsult()
    {
        Debug.Log($"Boss ngày {GameData.CurrentDay} tung chiêu: SỈ NHỤC / CHỬI BỚI!");
        
        // 🔥 ĐÒN QUYẾT ĐỊNH: Quét tìm đúng con Animator của đứa con đang BẬT (Active) lúc này
        Animator activeAnimator = GetComponentInChildren<Animator>();
        
        if (activeAnimator != null)
        {
            // Bắn hoạt ảnh chuẩn đét vào đứa của ngày hôm đó
            activeAnimator.SetTrigger("InsultAttack"); 
        }
        else
        {
            Debug.LogError("❌ Lỗi ngang ngược: Không tìm thấy con Animator nào đang bật ở các Object con cả ông ơi!");
        }
        
        if (PlayerHealth.instance != null)
        {
            float finalDamage = GetCurrentDayDamage();
            PlayerHealth.instance.TakeDamage(finalDamage);
        }
    }
}