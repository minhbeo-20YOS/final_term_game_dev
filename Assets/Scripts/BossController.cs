using UnityEngine;

public class BossController : MonoBehaviour
{
    public Animator bossAnimator;
    public float damagePerMiss = 1f; // Sát thương mỗi lần gõ sai/hụt

    void Start()
    {
        bossAnimator = GetComponentInChildren<Animator>();
        if (bossAnimator == null) 
            bossAnimator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        // Lắng nghe nhịp từ Conductor
        Conductor.OnBeat += CheckPlayerRhythm;
    }

    void OnDisable()
    {
        Conductor.OnBeat -= CheckPlayerRhythm;
    }

    // Hàm này tự động chạy mỗi khi nhạc đập vào nhịp mới
    void CheckPlayerRhythm(int totalBeats, int beatInBar)
    {
        // Ở cơ chế mới (Mũi tên bay), chúng ta xử lý trượt/trúng trực tiếp khi bấm phím
        // nên hàm check nhịp 4 cũ ở đây được để trống hoàn toàn một cách sạch sẽ.
    }

    // Hàm này sẽ được gọi trực tiếp từ NewGameplayManager khi người chơi lỡ nốt
    public void AttackAndInsult()
    {
        Debug.Log("Boss tung chiêu: SỈ NHỤC / CHỬI BỚI!");
        
        if (bossAnimator != null)
        {
            bossAnimator.SetTrigger("InsultAttack"); 
        }
        
        if (PlayerHealth.instance != null)
        {
            PlayerHealth.instance.TakeDamage(damagePerMiss);
        }
    }
}