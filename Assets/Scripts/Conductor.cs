using UnityEngine;

public class Conductor : MonoBehaviour
{
    public static Conductor instance;

    [Header("Audio Settings")]
    public AudioSource songAudio;     
    public float bpm = 88f;          
    
    [Header("Rhythm Calculations")]
    public float crotchet;             
    public float songPosition;        
    public float dspTimeSong;          

    [Header("Beat Tracking")]
    public int totalBeatsCalculated = 0; 
    public int currentBeatInBar = 1;    
    
    public delegate void OnBeatPassed(int beatCount, int beatInBar);
    public static event OnBeatPassed OnBeat;

    private bool isSongPlaying = false;
    private bool isAudioStarted = false;
    private float delayDuration; // Khoảng thời gian hoãn phát nhạc

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        crotchet = 60f / bpm;
        if (songAudio == null) songAudio = GetComponent<AudioSource>();

        // Lấy số nhịp sinh trước từ GameplayManager (Ví dụ là 3 nhịp)
        float spawnOffsetBeats = 3f; 
        if (NewGameplayManager.instance != null)
        {
            spawnOffsetBeats = NewGameplayManager.instance.spawnOffsetBeats;
        }

        // TÍNH TOÁN KHOẢNG DELAY: Thời gian cần thiết để nốt đầu tiên kịp bay lên nút gỗ
        delayDuration = spawnOffsetBeats * crotchet;

        StartSong();
    }

    void StartSong()
    {
        // Ghi lại mốc thời gian thực của máy tính lúc bấm Play
        dspTimeSong = (float)AudioSettings.dspTime;
        isSongPlaying = true;
        isAudioStarted = false;
        
        Debug.LogWarning($"[CONDUCTOR]: Bắt đầu đếm nhịp ảo! Nhạc sẽ phát sau {delayDuration} giây nữa...");
    }

    void Update()
    {
        if (!isSongPlaying) return;
        
        // TÍNH VỊ TRÍ THỜI GIAN CHUẨN (Có trừ đi khoảng thời gian delay dạo đầu)
        // Khi mới vào game, songPosition sẽ mang giá trị ÂM (ví dụ: -2.04 giây)
        songPosition = (float)(AudioSettings.dspTime - dspTimeSong) - delayDuration;
        
        // KHI ĐỒNG HỒ ĐẾM NGƯỢC VỀ ĐÚNG 0 -> TIẾNG NHẠC CẤT LÊN!
        if (!isAudioStarted && songPosition >= 0f)
        {
            if (songAudio != null && songAudio.clip != null)
            {
                songAudio.Play();
                isAudioStarted = true;
                Debug.Log("🎵 [MÁY PHÁT NHẠC]: BÙM! Nhạc phát đúng nhịp với nốt đầu tiên đè lên nút gỗ!");
            }
        }

        // Kiểm tra kết thúc bài nhạc
        if (isAudioStarted && songAudio != null && (songAudio.time >= 122f ||songAudio.time >= songAudio.clip.length))
        {
            isSongPlaying = false;
            songAudio.Stop();
            Debug.Log("Bài hát kết thúc!");
            return;
        }

        // Logic đếm nhịp tự động (Giữ nguyên toán học cũ của bạn nhưng chạy mượt từ thời gian âm)
        if (songPosition > (totalBeatsCalculated + 1) * crotchet - delayDuration)
        {
            totalBeatsCalculated++;
            currentBeatInBar = ((totalBeatsCalculated) % 4) + 1;
            
            if (OnBeat != null)
            {
                OnBeat(totalBeatsCalculated, currentBeatInBar);
            }
        }
    }
}