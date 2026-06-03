using UnityEngine;
using System.Collections.Generic;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager instance;

    [Header("Audition Settings")]
    public int minKeys = 3;            
    public int maxKeys = 6;           
    public float spaceHitWindow = 0.15f; 

    [Header("Current Round State")]
    public List<KeyCode> currentSequence = new List<KeyCode>(); 
    public int currentKeyIndex = 0;   
    
    private bool isSequenceCompleted = false; 
    private bool hasHitSpacePerfect = false;  
    private bool isRoundFailedEarly = false;  
    private float targetSpaceTime = 0f;      

    // Danh sách các phím mũi tên để game bốc ngẫu nhiên
    private KeyCode[] arrowKeys = { KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow };

    void Awake()
    {
        instance = this;
    }

    void OnEnable()
    {
        Conductor.OnBeat += HandleBeatLogic;
    }

    void OnDisable()
    {
        Conductor.OnBeat -= HandleBeatLogic;
    }

    // Hàm này lắng nghe từ Conductor để vận hành vòng chơi
    void HandleBeatLogic(int totalBeats, int beatInBar)
    {
        // 1. NHỊP 1: Sinh chuỗi phím mới cho vòng này
        if (beatInBar == 1)
        {
            GenerateNewSequence();
            
            // Tính toán trước xem nhịp 4 sẽ rơi vào giây thứ mấy của bài nhạc
            // Nhịp 4 cách nhịp 1 đúng 3 khoảng nhịp (crotchet)
            targetSpaceTime = Conductor.instance.songPosition + (Conductor.instance.crotchet * 3f);
        }

        // 2. NHỊP 4: Hết giờ gõ! Đây là lúc Boss sẽ gọi hàm CheckIfPlayerHitSpacePerfect()
        // Sau khi nhịp 4 trôi qua, ta reset trạng thái để chuẩn bị cho vòng sau ở nhịp 1
        if (beatInBar == 4)
        {
            // Nếu đến nhịp 4 rồi mà vẫn chưa gõ xong dãy hoặc chưa bấm Space -> Tự động tính là trượt
            // Đoạn này dùng để bọc lót, chuẩn bị data sẵn cho BossController check
        }
    }

    void GenerateNewSequence()
    {
        currentSequence.Clear();
        currentKeyIndex = 0;
        isSequenceCompleted = false;
        hasHitSpacePerfect = false;
        isRoundFailedEarly = false;

        // Bốc ngẫu nhiên độ dài chuỗi phím
        int sequenceLength = Random.Range(minKeys, maxKeys + 1);

        string debugStr = "Chuỗi phím mới: ";
        for (int i = 0; i < sequenceLength; i++)
        {
            KeyCode randomArrow = arrowKeys[Random.Range(0, arrowKeys.Length)];
            currentSequence.Add(randomArrow);
            debugStr += randomArrow.ToString() + " ";
        }
        
        Debug.Log(debugStr);
        // Tí nữa bạn có thể viết thêm hàm UI để hiển thị dãy mũi tên này lên màn hình ở đây
    }

    void Update()
    {
        // Nếu đã bị tính là thua sớm (gõ sai) hoặc đã gõ xong hết rồi thì không nhận input nữa
        if (isRoundFailedEarly) return;

        // LƯU Ý: Nếu chưa gõ xong dãy mũi tên
        if (!isSequenceCompleted)
        {
            if (Input.anyKeyDown)
            {
                // Kiểm tra xem phím người chơi vừa bấm có phải là phím mũi tên đúng không
                if (Input.GetKeyDown(currentSequence[currentKeyIndex]))
                {
                    Debug.Log("Gõ ĐÚNG nút: " + currentSequence[currentKeyIndex]);
                    currentKeyIndex++;

                    // Nếu đã gõ đến nút cuối cùng của dãy
                    if (currentKeyIndex >= currentSequence.Count)
                    {
                        isSequenceCompleted = true;
                        Debug.Log("Đã gõ xong dãy mũi tên! ĐỢI NHỊP 4 ĐỂ BẤM SPACEBAR...");
                    }
                }
                // Nếu bấm đại một nút khác hoặc gõ sai mũi tên
                else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || 
                         Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
                {
                    Debug.Log("Gõ SAI mũi tên! Tạch lượt này.");
                    isRoundFailedEarly = true;
                    // Phạt nhẹ gõ sai ngay lập tức nếu muốn
                    PlayerHealth.instance.TakeDamage(2f); 
                }
            }
        }
        // Nếu ĐA GÕ XONG dãy mũi tên và đang canh nhịp bấm SPACE
        else if (isSequenceCompleted && !hasHitSpacePerfect)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // Lấy thời gian thực của bài nhạc ngay lúc bấm Space
                float inputTime = Conductor.instance.songPosition;
                
                // Tính khoảng cách lệch so với nhịp 4 chuẩn thực tế
                float timeDifference = Mathf.Abs(inputTime - targetSpaceTime);

                if (timeDifference <= spaceHitWindow)
                {
                    Debug.Log("SPACEBAR PERFECT!!! Khớp nhịp: " + timeDifference);
                    hasHitSpacePerfect = true;
                    // Thêm điểm hoặc tăng progress tại đây
                }
                else
                {
                    Debug.Log("SPACEBAR TRƯỢT NHỊP! Bấm quá sớm hoặc quá muộn. Lệch: " + timeDifference);
                    isRoundFailedEarly = true; // Tính là trượt luôn
                }
            }
        }
    }

    // ĐÂY LÀ HÀM QUAN TRỌNG NHẤT: Để BossController gọi sang hỏi kết quả
    public bool CheckIfPlayerHitSpacePerfect()
    {
        // Trả về kết quả: True nếu gõ đúng + Space chuẩn. False nếu gõ sai/hụt.
        return hasHitSpacePerfect;
    }
}