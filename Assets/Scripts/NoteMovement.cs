using UnityEngine;
using TMPro;

public class NoteMovement : MonoBehaviour
{
    // Để public để bạn có thể nhìn thấy nó thay đổi số trong bảng Inspector khi bấm Play
    public float beatTime;      
    public KeyCode requiredKey; 
    public float targetY = 0f;  
    public float scrollSpeed = 400f; 

    [Header("Kéo ô Text của Prefab vào đây")]
    public TextMeshProUGUI noteText; 

    private RectTransform rectTransform;
    private bool isInitialized = false;

    public void SetupNote(float hitTime, KeyCode key, string arrowChar)
    {
        beatTime = hitTime;
        requiredKey = key;
        
        // Cố gắng lấy Text nếu bạn quên chưa kéo dây ở Inspector
        if (noteText == null) noteText = GetComponentInChildren<TextMeshProUGUI>();
        
        if (noteText != null)
        {
            noteText.text = arrowChar;
        }

        rectTransform = GetComponent<RectTransform>();
        isInitialized = true; // Đánh dấu đã nhận dữ liệu thành công!
    }

    void Update()
    {
        // Nếu chưa được gọi hàm SetupNote thì đứng im chờ đợi
        if (!isInitialized || Conductor.instance == null) return;

        // Tính vị trí bay dựa trên thời gian nhạc
        float timeToHit = beatTime - Conductor.instance.songPosition;
        float currentY = targetY - (timeToHit * scrollSpeed);

        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        
        // Ép vị trí X luôn bằng 0, Y thay đổi để bay lên
        rectTransform.anchoredPosition = new Vector2(0f, currentY);

        // NẾU BAY QUÁ NÚT MÀ KHÔNG BẤM -> TỰ ĐỘNG MISS VÀ BIẾN MẤT
        if (Conductor.instance.songPosition > beatTime + 0.15f)
        {
            if (NewGameplayManager.instance != null)
            {
                NewGameplayManager.instance.NoteMissed(this.gameObject);
            }
            else
            {
                // Bọc lót nếu không tìm thấy Manager thì tự hủy để không bị rác màn hình
                Destroy(gameObject);
            }
        }
    }
}