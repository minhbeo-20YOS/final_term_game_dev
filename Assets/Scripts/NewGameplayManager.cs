using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;          
using TMPro;                   
using UnityEngine.SceneManagement; 

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
    public TextMeshProUGUI hitCounterText; 
    public GameObject nextScenePanel;     
    
    // 🔥 THÊM ĐÚNG Ô NÀY ĐỂ KÉO THẢ PANEL VICTORY
    public GameObject victoryPanel;

    public string nextSceneName = "SampleScene";
    public int targetHitCount = 30;

    private int currentHitCount = 0; 
    private bool isLevelCompleted = false; 

    [Header("Timing")]
    public float hitWindow = 0.12f;     
    public float spawnOffsetBeats = 3f;
    
    [Header("--- Coworker Settings ---")]
    public GameObject[] coworkerList;

    private List<GameObject> activeNotes = new List<GameObject>();
    private KeyCode[] keys = { KeyCode.LeftArrow, KeyCode.DownArrow, KeyCode.UpArrow, KeyCode.RightArrow };
    private string[] arrowChars = { "O", "O", "O", "O" };
    
    [Header("Cấu hình File Map Nhạc")]
    public string mapFileName = "Sweden";
    public GameObject hitEffectPrefab;
    
    public Material[] laneMaterials;

    void Awake() { instance = this; }

    void Start()
    {
        int coworkerIndex = GameData.CurrentDay - 1;
        
        for (int i = 0; i < coworkerList.Length; i++)
        {
            if (coworkerList[i] != null)
            {
                coworkerList[i].SetActive(i == coworkerIndex);
            }
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        currentHitCount = 0;
        UpdateHitCounterUI();
        if (nextScenePanel != null) nextScenePanel.SetActive(false); 

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

    void UpdateHitCounterUI()
    {
        if (hitCounterText != null)
        {
            hitCounterText.text = $"Progress: {currentHitCount} / {targetHitCount}";
        }
    }

    void CompleteLevel()
    {
        isLevelCompleted = true;
        Debug.LogWarning($"🎉 XUẤT SẮC! Đã bấm đúng {targetHitCount} nốt. Đang hiển thị bảng văn phòng...");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (nextScenePanel != null) nextScenePanel.SetActive(true);

        if (Conductor.instance != null && Conductor.instance.GetComponent<AudioSource>() != null)
        {
            Conductor.instance.GetComponent<AudioSource>().Stop();
        }

        ClearAllActiveNotes();
    }

    void ClearAllActiveNotes()
    {
        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            if (activeNotes[i] != null) Destroy(activeNotes[i]);
        }
        activeNotes.Clear();
    }

    // 🔥 HÀM CLICK NÚT GOHOME ĐÃ ĐƯỢC ĐỔI THEO Ý ÔNG
    public void OnNextButtonClick()
    {
        Debug.LogWarning("🚀 Người chơi bấm nút Next! Đang xử lý chuyển ngày...");

        // 1. TĂNG SỐ NGÀY LÊN 1
        GameData.CurrentDay++;
        Debug.Log($"[Hệ thống]: Trời đã sáng! Hôm nay là Day: {GameData.CurrentDay}");

        // 2. KIỂM TRA ĐIỀU KIỆN THẮNG (Nếu > 3 thì bật panel Victory lên thôi, không chuyển Scene)
        if (GameData.CurrentDay > 3)
        {
            Debug.LogWarning("🏆 CHÚC MỪNG! Bạn đã thắng!");

            // Chỉ cần mở active Victory lên thôi đúng chuẩn ý ông luôn:
            if (victoryPanel != null) 
            {
                victoryPanel.SetActive(true);
                Time.timeScale = 0;
            }
            
            return; // Chặn không cho chạy xuống hàm LoadNextScene ở dưới
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
            
            if (boss != null)
            {
                boss.AttackAndInsult(); // Gọi thg cha tung chiêu
            }
        }
    }
    
    private void ResetGameProgress()
    {
        GameData.CurrentDay = 1;
        GameData.PlayerHP = 100f; 
        
        Time.timeScale = 1f;

        Debug.LogWarning("[Hệ thống] Đã reset toàn bộ dữ liệu tĩnh! Sẵn sàng cho lượt chơi mới.");
    }
    
    public void OnPlayAgainButtonClick()
    {
        ResetGameProgress();
        
        SceneManager.LoadScene("SampleScene");
    }
    
    public void OnReturnToMainMenuButtonClick()
    {
        ResetGameProgress();
        
        SceneManager.LoadScene("MainMenu");
    }
}