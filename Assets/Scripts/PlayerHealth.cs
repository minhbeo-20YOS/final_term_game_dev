using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth instance; // Tạo Singleton để Boss dễ gọi

    public float maxHP = 100f;
    public float currentHP;
    public Slider hpSlider;
    
    [Header("Hiệu Ứng Bị Đòn")]
    public GameObject getHitEffectPrefab;
    [Tooltip("Kéo thả Object 'HitPoint' (con của Player) vào ô này")]
    public Transform hitPointTransform;
    public GameObject gameOverPanel;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        currentHP = GameData.PlayerHP;
        UpdateHPUI();
    }
    
    public void TakeDamage(float damageAmount)
    {
        currentHP -= damageAmount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP); 
        
        GameData.PlayerHP = currentHP;
        
        UpdateHPUI();

        if (getHitEffectPrefab != null && hitPointTransform != null)
        {
            GameObject instantiatedEffect = Instantiate(getHitEffectPrefab, hitPointTransform.position, hitPointTransform.rotation);
            Destroy(instantiatedEffect, 1.0f); 
        }
        else if (getHitEffectPrefab != null && hitPointTransform == null)
        {
            GameObject instantiatedEffect = Instantiate(getHitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(instantiatedEffect, 1.0f);
        }
        
        Animator playerAnim = GetComponentInChildren<Animator>();
        if (playerAnim != null)
        {
            playerAnim.SetTrigger("TakeDamage"); 
        }

        // 🔥 2. KIỂM TRA ĐIỀU KIỆN THUA CUỘC (Hết máu)
        if (currentHP <= 0)
        {
            Debug.Log("Game Over! Người chơi bị sa thải + Tự sad.");
            
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            
            Time.timeScale = 0;
            
            // Giải phóng con trỏ chuột để người chơi tương tác bấm nút "Chơi lại" (Retry)
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // (Tùy chọn) Ông có thể làm chậm thời gian game lại nếu muốn: Time.timeScale = 0f;
        }
    }

    void UpdateHPUI()
    {
        if (hpSlider != null)
        {
            hpSlider.value = currentHP / maxHP;
        }
    }
}