using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;          // Thêm để quản lý Text/Panel UI truyền thống
using TMPro;                   // Thêm để quản lý TextMesh Pro xịn
using UnityEngine.SceneManagement; // THÊM THƯ VIỆN NÀY ĐỂ CHUYỂN SCENE

public class NewGameplayManager : MonoBehaviour
{
    public static NewGameplayManager instance;

    public enum GameMode { PlayCustomMap, RecordMode, RandomMachine }
    [Header("Chọn Chế Độ Chôi")]
    public GameMode currentMode = GameMode.PlayCustomMap;

    [Header("Danh Sách Nốt (Bản Đồ Nhạc)")]
    public List<NoteData> customBeatmap = new List<NoteData>(); 
    private int currentNoteIndex = 0; 

    [Header("Prefabs & UI")]
    public GameObject notePrefab;       
    public RectTransform[] targetButtons; 

    [Header("Hệ Thống Đếm Nốt & Chuyển Cảnh")]
    [Tooltip("Kéo thả Text hiển thị số lượt bấm trúng vào đây")]
    public TextMeshProUGUI hitCounterText; 
    [Tooltip("Kéo thả Panel thông báo Next Scene vào đây (Mặc định ẩn đi)")]
    public GameObject nextScenePanel;     
    [Tooltip("Tên của Scene tiếp theo muốn chuyển đến (Ví dụ: Day1 hoặc SampleScene)")]
    public string nextSceneName = "SampleScene";
    public string winSceneName = "WinScene";
    [Tooltip("Số lượt bấm đúng yêu cầu để qua màn")]
    public int targetHitCount = 30;

    private int currentHitCount = 0; // Biến tích lũy số lần bấm đúng
    private bool isLevelCompleted = false; // Tránh việc chuyển cảnh liên tục nhiều lần

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
        // Khởi tạo UI ban đầu
        currentHitCount = 0;
        UpdateHitCounterUI();
        if (nextScenePanel != null) nextScenePanel.SetActive(false); // Ẩn bảng chuyển cảnh đi khi đầu game

        if (currentMode == GameMode.PlayCustomMap)
        {
            LoadBeatmapFromFile();
        }

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
        if (Conductor.instance == null || isLevelCompleted) return;

        if (currentMode == GameMode.RecordMode)
        {
            RecordYourOwnBeatmap();
            if (Input.GetKeyDown(KeyCode.F5)) SaveBeatmapToFile();
            return; 
        }

        if (currentMode == GameMode.PlayCustomMap)
        {
            if (Input.GetKeyDown(KeyCode.F6)) LoadBeatmapFromFile();
            CheckAndSpawnMapNotes();
        }

        for (int i = 0; i < keys.Length; i++)
        {
            if (Input.GetKeyDown(keys[i])) CheckHit(keys[i]);
        }
    }

    // --- HÀM CẬP NHẬT CHỮ HIỂN THỊ SỐ LƯỢT BẤM ---
    void UpdateHitCounterUI()
    {
        if (hitCounterText != null)
        {
            hitCounterText.text = $"Progress: {currentHitCount} / {targetHitCount}";
        }
    }

    // --- HÀM XỬ LÝ QUA MÀN KHI ĐỦ 30 LẦN BẤM ĐÚNG ---
    void CompleteLevel()
    {
        isLevelCompleted = true;
        Debug.LogWarning($"🎉 XUẤT SẮC! Đã bấm đúng {targetHitCount} nốt. Đang hiển thị bảng văn phòng...");

        // 1. Hiện cái Panel thông báo lên màn hình (Canvas Overlay nằm trên cùng)
        if (nextScenePanel != null)
        {
            nextScenePanel.SetActive(true);
        }

        // Tắt nhạc nền đi cho đỡ ồn ào khi chuyển sang màn hình tan ca
        if (Conductor.instance != null && Conductor.instance.GetComponent<AudioSource>() != null)
        {
            Conductor.instance.GetComponent<AudioSource>().Stop();
        }

        // 🔥 XỬ LÝ TRIỆT ĐỂ: Dọn sạch tất cả nốt nhạc đang bay lơ lửng phía sau để màn hình gọn gàng
        ClearAllActiveNotes();
    }

    void ClearAllActiveNotes()
    {
        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            if (activeNotes[i] != null)
            {
                Destroy(activeNotes[i]);
            }
        }
        activeNotes.Clear();
    }

    // 🔥 HÀM MỚI: GẮN VÀO SỰ KIỆN CLICK CỦA NÚT NEXT TRÊN PANEL VĂN PHÒNG
    public void OnNextButtonClick()
    {
        Debug.LogWarning("🚀 Người chơi bấm nút Next! Đang xử lý chuyển ngày...");

        // 1. TĂNG SỐ NGÀY LÊN 1 (Nhân vật ngủ dậy)
        GameData.CurrentDay++;
        Debug.Log($"[Hệ thống]: Trời đã sáng! Hôm nay là Day: {GameData.CurrentDay}");

        // 2. KIỂM TRA ĐIỀU KIỆN THẮNG (Sống sót qua 3 ngày. Sáng Day 4 = Thắng)
        if (GameData.CurrentDay > 3)
        {
            Debug.LogWarning("🏆 CHÚC MỪNG! Bạn đã sống sót qua 3 ngày địa ngục công sở. Bạn thắng!");
            
            // Tắt Panel Gameplay cũ
            if (nextScenePanel != null) nextScenePanel.SetActive(false);

            // Chuyển đến màn hình thắng cuộc (Tạo Scene tên là 'WinScene' và add vào Build Settings nhé)
            SceneManager.LoadScene(winSceneName); 
            
            return; // Ngăn không cho code chạy xuống dòng LoadNextScene() mặc định bên dưới
        }
        
        LoadNextScene();
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    [System.Serializable]
    private class BeatmapContainer
    {
        public List<NoteData> customBeatmap;
    }

    void SaveBeatmapToFile()
    {
        if (customBeatmap.Count == 0) return;
        BeatmapContainer container = new BeatmapContainer();
        container.customBeatmap = this.customBeatmap;

        string jsonString = JsonUtility.ToJson(container, true);
        string filePath = Application.dataPath + "/DataNote/" + mapFileName + ".txt";
        System.IO.File.WriteAllText(filePath, jsonString);
        
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    public void LoadBeatmapFromFile()
    {
        string filePath = Application.dataPath + "/DataNote/" + mapFileName + ".txt";
        if (System.IO.File.Exists(filePath))
        {
            string jsonString = System.IO.File.ReadAllText(filePath);
            BeatmapContainer container = JsonUtility.FromJson<BeatmapContainer>(jsonString);
            if (container != null && container.customBeatmap != null)
            {
                this.customBeatmap = container.customBeatmap;
            }
            currentNoteIndex = 0;
        }
    }

    void CheckAndSpawnMapNotes()
    {
        if (currentNoteIndex >= customBeatmap.Count) return;
        float timeToSpawn = customBeatmap[currentNoteIndex].hitTime - (spawnOffsetBeats * Conductor.instance.crotchet);
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
            if (laneMaterials != null && laneMaterials.Length > laneIndex && moveScript.noteText != null)
            {
                moveScript.noteText.fontSharedMaterial = laneMaterials[laneIndex];
            }
        }
        activeNotes.Add(newNote);
    }
    
    void RecordYourOwnBeatmap()
    {
        if (Conductor.instance.songPosition < 0f) return;
        for (int i = 0; i < keys.Length; i++)
        {
            if (Input.GetKeyDown(keys[i]))
            {
                NoteData newRecordedNote = new NoteData();
                newRecordedNote.hitTime = Conductor.instance.songPosition; 
                newRecordedNote.laneIndex = i;
                customBeatmap.Add(newRecordedNote);
            }
        }
    }

    void SpawnNoteOnBeat_Random(int totalBeats, int beatInBar)
    {
        int randomIndex = Random.Range(0, 4);
        float targetHitTime = Conductor.instance.songPosition + (spawnOffsetBeats * Conductor.instance.crotchet);
        SpawnSpecificNote(randomIndex, targetHitTime);
    }

    void CheckHit(KeyCode pressedKey)
    {
        if (Conductor.instance == null || isLevelCompleted) return;

        GameObject closestNote = null;
        float smallestTimeDiff = float.MaxValue;

        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            GameObject note = activeNotes[i];
            if (note == null) { activeNotes.RemoveAt(i); continue; }

            NoteMovement move = note.GetComponent<NoteMovement>();
            if (move == null) continue;

            if (move.requiredKey == pressedKey)
            {
                float diff = Mathf.Abs(Conductor.instance.songPosition - move.beatTime);
                if (diff < smallestTimeDiff)
                {
                    smallestTimeDiff = diff;
                    closestNote = note;
                }
            }
        }

        if (closestNote != null && smallestTimeDiff <= hitWindow)
        {
            currentHitCount++; 
            UpdateHitCounterUI(); 

            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, closestNote.transform.position, Quaternion.identity, closestNote.transform.parent);
                effect.transform.localScale = Vector3.one;
            }

            activeNotes.Remove(closestNote);
            Destroy(closestNote);

            if (currentHitCount >= targetHitCount)
            {
                CompleteLevel();
            }
        }
    }

    public void NoteMissed(GameObject note)
    {
        if (activeNotes.Contains(note))
        {
            activeNotes.Remove(note);
            Destroy(note);

            BossController boss = GameObject.FindFirstObjectByType<BossController>();
            if (boss != null) boss.AttackAndInsult();
        }
    }
}