using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenuPanel; // Kéo bảng Pause (ảnh nền + các nút) vào đây

    [Header("Scene Settings")]
    public string mainMenuSceneName = "MainMenu"; // Điền đúng tên Scene màn hình chính của bạn

    // Biến này để theo dõi xem game đang dừng hay đang chạy
    private bool isPaused = false; 

    void Start()
    {
        // Đảm bảo khi mới vào game, bảng Pause luôn ẩn và thời gian chạy bình thường
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    void Update()
    {
        // Lắng nghe sự kiện người chơi bấm nút Esc (Escape) trên bàn phím
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame(); // Nếu đang dừng thì tiếp tục
            }
            else
            {
                PauseGame(); // Nếu đang chơi thì dừng lại
            }
        }
    }

    // Gắn vào nút "Tiếp tục chơi"
    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false); // Ẩn UI
        Time.timeScale = 1f; // Khôi phục thời gian về bình thường
        isPaused = false; // Cập nhật trạng thái
    }

    // Hàm này tự động gọi khi bấm Esc, không cần gắn vào nút nào
    private void PauseGame()
    {
        pauseMenuPanel.SetActive(true); // Hiện UI Pause lên
        Time.timeScale = 0f; // Đóng băng thời gian trong game
        isPaused = true; // Cập nhật trạng thái
    }

    // Gắn vào nút "Chơi lại"
    // Gắn vào nút "Chơi lại"
    public void RestartGame()
    {
        // Rã đông thời gian
        Time.timeScale = 1f; 
        
        // Thay vì lấy Scene hiện hành, ép hệ thống load trực tiếp tên Scene bạn muốn
        SceneManager.LoadScene("SampleScene"); 
    }

    // Gắn vào nút "Thoát ra màn hình chính"
    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // Rã đông thời gian
        SceneManager.LoadScene(mainMenuSceneName); // Load về Menu
    }
}