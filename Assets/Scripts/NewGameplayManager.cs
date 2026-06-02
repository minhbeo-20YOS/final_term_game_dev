using UnityEngine;
using System.Collections.Generic;

public class NewGameplayManager : MonoBehaviour
{
    public static NewGameplayManager instance;

    public enum GameMode { PlayCustomMap, RecordMode, RandomMachine }
    [Header("Chọn Chế Độ Chơi")]
    public GameMode currentMode = GameMode.PlayCustomMap;

    [Header("Danh Sách Nốt (Bản Đồ Nhạc)")]
    // Bạn có thể tự tay thêm nốt hoặc bấm Record để game tự điền vào danh sách này!
    public List<NoteData> customBeatmap = new List<NoteData>(); 
    private int currentNoteIndex = 0; // Để theo dõi đang chơi đến nốt nào

    [Header("Prefabs & UI")]
    public GameObject notePrefab;       
    public RectTransform[] targetButtons; 

    [Header("Timing")]
    public float hitWindow = 0.12f;     
    public float spawnOffsetBeats = 3f; 

    private List<GameObject> activeNotes = new List<GameObject>();
    private KeyCode[] keys = { KeyCode.LeftArrow, KeyCode.DownArrow, KeyCode.UpArrow, KeyCode.RightArrow };
    private string[] arrowChars = { "←", "↓", "↑", "→" };
    
    [Header("Cấu hình File Map Nhạc")]
    public string mapFileName = "Sweden";
    public GameObject hitEffectPrefab;
    
    public Material[] laneMaterials;

    void Awake() { instance = this; }

    void Start()
    {
        // TỰ ĐỘNG NẠP MAP: Nếu đang chọn chế độ chơi Map, tự đọc file text luôn khi vừa bấm Play
        if (currentMode == GameMode.PlayCustomMap)
        {
            LoadBeatmapFromFile();
        }

        // Chế độ chơi Random máy đếm nhịp cũ
        if (currentMode == GameMode.RandomMachine)
        {
            Conductor.OnBeat += SpawnNoteOnBeat_Random;
        }
    }

    void OnDestroy()
    {
        Conductor.OnBeat -= SpawnNoteOnBeat_Random;
    }

    void Update()
    {
        if (Conductor.instance == null) return;

        // CHẾ ĐỘ 1: TỰ LÀM MAP BẰNG TAI (RECORD MODE)
        if (currentMode == GameMode.RecordMode)
        {
            RecordYourOwnBeatmap();
            
            // NHẤN F5 ĐỂ LƯU THÀNH FILE TEXT THEO TÊN ĐÃ ĐẶT
            if (Input.GetKeyDown(KeyCode.F5))
            {
                SaveBeatmapToFile();
            }
            return; 
        }

        // CHẾ ĐỘ 2: CHƠI THEO MAP ĐÃ LÀM
        if (currentMode == GameMode.PlayCustomMap)
        {
            // MẸO: Nhấn F6 lúc đang ở menu hoặc đầu game để tự động đọc file text vào chơi
            if (Input.GetKeyDown(KeyCode.F6))
            {
                LoadBeatmapFromFile();
            }

            CheckAndSpawnMapNotes();
        }

        // Người chơi bấm nút để ăn nốt
        for (int i = 0; i < keys.Length; i++)
        {
            if (Input.GetKeyDown(keys[i])) CheckHit(keys[i]);
        }
    }

    // Cấu trúc phụ dùng để đóng gói riêng danh sách nốt khi chuyển sang chữ JSON
    [System.Serializable]
    private class BeatmapContainer
    {
        public List<NoteData> customBeatmap;
    }

    // --- HÀM TỰ ĐỘNG GHI FILE TEXT (Chỉ lưu danh sách nốt) ---
    void SaveBeatmapToFile()
    {
        if (customBeatmap.Count == 0) return;

        // ĐÓNG GÓI RIÊNG: Chỉ bỏ danh sách nốt vào gói, không bỏ "currentMode" vào
        BeatmapContainer container = new BeatmapContainer();
        container.customBeatmap = this.customBeatmap;

        string jsonString = JsonUtility.ToJson(container, true);
        string filePath = Application.dataPath + "/DataNote/" + mapFileName + ".txt";
        
        System.IO.File.WriteAllText(filePath, jsonString);
        
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
        Debug.LogWarning($"💾 [ĐÃ LƯU]: Bản map [{mapFileName}.txt] đã lưu an toàn tại Assets/DataNote/!");
    }

    // --- HÀM TỰ ĐỘNG ĐỌC FILE TEXT (Chỉ nạp nốt, không đè Mode) ---
    public void LoadBeatmapFromFile()
    {
        string filePath = Application.dataPath + "/DataNote/" + mapFileName + ".txt";

        if (System.IO.File.Exists(filePath))
        {
            string jsonString = System.IO.File.ReadAllText(filePath);
            
            // ĐỌC RIÊNG: Chỉ giải nén danh sách nốt ra
            BeatmapContainer container = JsonUtility.FromJson<BeatmapContainer>(jsonString);
            
            if (container != null && container.customBeatmap != null)
            {
                this.customBeatmap = container.customBeatmap;
            }
            
            currentNoteIndex = 0; // Reset chỉ số nốt về đầu bài
            
            Debug.LogWarning($"🎵 [ĐÃ NẠP MAP]: Đã nạp thành công [{mapFileName}.txt]. Số nốt: {customBeatmap.Count}");
        }
        else
        {
            Debug.LogError($"❌ KHÔNG TÌM THẤY FILE: [{mapFileName}.txt] trong thư mục Assets/DataNote/!");
        }
    }

    // --- LOGIC CHẾ ĐỘ CHƠI THEO MAP ---
    void CheckAndSpawnMapNotes()
    {
        if (currentNoteIndex >= customBeatmap.Count) return;

        // Tính thời gian cần sinh nốt ra trước để nó kịp bay lên
        float timeToSpawn = customBeatmap[currentNoteIndex].hitTime - (spawnOffsetBeats * Conductor.instance.crotchet);

        // Nếu thời gian bài nhạc chạm đến mốc cần sinh nốt
        if (Conductor.instance.songPosition >= timeToSpawn)
        {
            SpawnSpecificNote(customBeatmap[currentNoteIndex].laneIndex, customBeatmap[currentNoteIndex].hitTime);
            currentNoteIndex++;
        }
    }

    void SpawnSpecificNote(int laneIndex, float exactHitTime)
    {
        GameObject newNote = Instantiate(notePrefab, targetButtons[laneIndex]);
        newNote.transform.localScale = Vector3.one;

        NoteMovement moveScript = newNote.GetComponent<NoteMovement>();
        if (moveScript != null)
        {
            moveScript.SetupNote(exactHitTime, keys[laneIndex], arrowChars[laneIndex]);
        
            // ĐOẠN ĐỘ NEON XỊN: Đổi nguyên quả cầu Material (Thay cả màu ruột lẫn màu viền phát sáng Glow)
            if (laneMaterials != null && laneMaterials.Length > laneIndex && moveScript.noteText != null)
            {
                // Ép TextMesh Pro sài đúng Material Neon riêng biệt của làn đường đó
                moveScript.noteText.fontSharedMaterial = laneMaterials[laneIndex];
            }
        }
        activeNotes.Add(newNote);
    }
    
    void RecordYourOwnBeatmap()
    {
        // ĐOẠN THÊM VÀO: Nếu nhạc chưa chính thức phát (đang trong thời gian delay âm), chặn không cho Record
        if (Conductor.instance.songPosition < 0f) return;

        for (int i = 0; i < keys.Length; i++)
        {
            if (Input.GetKeyDown(keys[i]))
            {
                NoteData newRecordedNote = new NoteData();
                
                // Ghi lại chính xác giây hiện tại của bài hát khi bạn bấm phím
                newRecordedNote.hitTime = Conductor.instance.songPosition; 
                newRecordedNote.laneIndex = i;

                customBeatmap.Add(newRecordedNote);
                
                // In ra màn hình số giây đẹp đẽ để bạn theo dõi
                Debug.LogWarning($"[ĐÃ GHI NỐT]: Hướng {arrowChars[i]} tại giây thứ: {newRecordedNote.hitTime:F2}s");
            }
        }
    }

    // --- CÁC HÀM CŨ GIỮ NGUYÊN (CheckHit, NoteMissed, SpawnNoteOnBeat_Random...) ---
    void SpawnNoteOnBeat_Random(int totalBeats, int beatInBar)
    {
        int randomIndex = Random.Range(0, 4);
        float targetHitTime = Conductor.instance.songPosition + (spawnOffsetBeats * Conductor.instance.crotchet);
        SpawnSpecificNote(randomIndex, targetHitTime);
    }

    void CheckHit(KeyCode pressedKey)
    {
        if (Conductor.instance == null) return;

        GameObject closestNote = null;
        float smallestTimeDiff = float.MaxValue;

        // Vòng lặp ngược bảo vệ
        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            GameObject note = activeNotes[i];
        
            if (note == null)
            {
                activeNotes.RemoveAt(i);
                continue;
            }

            NoteMovement move = note.GetComponent<NoteMovement>();
            if (move == null) continue;

            // Kiểm tra nếu nốt khớp với phím người chơi vừa ấn
            if (move.requiredKey == pressedKey)
            {
                // Tính khoảng độ lệch thời gian tuyệt đối giữa mốc bấm và mốc chuẩn của nốt
                float diff = Mathf.Abs(Conductor.instance.songPosition - move.beatTime);
                
                if (diff < smallestTimeDiff)
                {
                    smallestTimeDiff = diff;
                    closestNote = note;
                }
            }
        }

        // Nếu tìm thấy nốt khớp phím và độ lệch nằm TRONG khoảng thời gian cho phép (hitWindow = 0.12s)
        if (closestNote != null && smallestTimeDiff <= hitWindow)
        {
            Debug.LogWarning($"🎯 [HIT PERFECT]! Độ lệch: {smallestTimeDiff:F4} giây.");
            
            // 🔍 DEBUG TRƯỚC KHI SPAWN: Kiểm tra xem biến Prefab đã được kéo thả vào chưa
            if (hitEffectPrefab == null)
            {
                Debug.LogError("❌ LỖI CHÍ MẠNG: Bạn chưa kéo thả file Particle Prefab vào ô 'hitEffectPrefab' trong bảng Inspector của GameplayManager_Object kìa!");
            }
            else
            {
                Debug.Log($"🚀 [DEBUG]: Đang tiến hành tạo Particle tại vị trí của nốt: {closestNote.transform.position}. Tên nốt bị xóa: {closestNote.name}");
                
                // Sinh ra làm con của nút gỗ để nó thừa hưởng phân cấp UI Canvas
                GameObject effect = Instantiate(hitEffectPrefab, closestNote.transform.position, Quaternion.identity, closestNote.transform.parent);
                
                // 🔍 DEBUG SAU KHI SPAWN: Kiểm tra xem Object có được tạo ra thật không
                if (effect != null)
                {
                    effect.transform.localScale = Vector3.one;
                    Debug.LogWarning($"✅ [DEBUG]: Đã khởi tạo thành công Object Particle: {effect.name}. Vị trí cục bộ: {effect.transform.localPosition}");
                }
                else
                {
                    Debug.LogError("❌ LỖI KỲ LẠ: Lệnh Instantiate trả về null, không thể tạo Object!");
                }
            }

            activeNotes.Remove(closestNote);
            Destroy(closestNote); // Hủy nốt ngay lập tức trên màn hình
        }
        else
        {
            // Nếu tìm thấy nốt cùng hướng nhưng khoảng cách xa quá (bấm quá sớm hoặc quá trễ)
            if (closestNote != null)
            {
                Debug.Log($"❌ BẤM TRƯỢT! Bạn gõ phím đúng hướng nhưng bị lệch quá xa: {smallestTimeDiff:F4} giây (Yêu cầu <= {hitWindow}s)");
            }
            else
            {
                Debug.Log("❌ BẤM BẬY! Hướng này hiện tại không có nốt nào đang bay tới cả.");
            }
        }
    }

    public void NoteMissed(GameObject note)
    {
        if (activeNotes.Contains(note))
        {
            Debug.Log("MISS! Người chơi để nốt bay mất.");
            activeNotes.Remove(note);
            Destroy(note);

            // Gọi Boss tìm thấy trong màn chơi để tung chiêu chửi bới và trừ máu
            BossController boss = GameObject.FindFirstObjectByType<BossController>();
            if (boss != null)
            {
                boss.AttackAndInsult();
            }
        }
    }
}