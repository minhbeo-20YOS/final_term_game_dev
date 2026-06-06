using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI & Transition References")]
    public GameObject menuUIElements; 
    public VideoPlayer transitionVideo; 
    public CanvasGroup fadePanel; 
    public GameObject exitConfirmationPanel; 
    
    [Header("Tutorial References")]
    public GameObject tutorialPanel; // Kéo TutorialPanel vào đây

    [Header("Options References")]
    public GameObject optionsPanel; // Kéo OptionMenuPanel vào đây
    public Image soundBtnImage; // Kéo component Image của nút SoundBtn vào đây
    public Image fullscreenBtnImage; // Kéo component Image của nút FullscreenBtn vào đây
    public Sprite iconOn; // Kéo file ảnh nút ON vào đây
    public Sprite iconOff; // Kéo file ảnh nút OFF vào đây
    
    [Header("Video References")]
    public VideoPlayer backgroundVideo; // Kéo object BackgroundVid vào đây

    [Header("Settings")]
    public float fadeDuration = 1.5f; 
    public string nextSceneName = "SampleScene";

    // Trạng thái biến
    private bool isSoundOn = true;

    private void Start()
    {
        // Đồng bộ UI ngay khi khởi động game dựa trên trạng thái thực tế
        UpdateOptionsUI();
    }

    // --- PHẦN CODE CHUYỂN CẢNH VÀ EXIT CŨ (Giữ nguyên) ---
    public void StartNewGame()
    {
        menuUIElements.SetActive(false); 
        StartCoroutine(PlayCutsceneAndLoad());
    }

    private IEnumerator PlayCutsceneAndLoad()
    {
        transitionVideo.gameObject.SetActive(true);
        transitionVideo.Play();
        while (!transitionVideo.isPrepared) yield return null;
        yield return new WaitForSeconds((float)transitionVideo.length);
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            yield return null;
        }
        fadePanel.alpha = 1f;
        SceneManager.LoadScene(nextSceneName);
    }

    public void OpenExitPopup()
    {
        if (exitConfirmationPanel != null)
        {
            exitConfirmationPanel.SetActive(true); 
            menuUIElements.SetActive(false); 
        }
    }

    public void CloseExitPopup()
    {
        if (exitConfirmationPanel != null)
        {
            exitConfirmationPanel.SetActive(false); 
            menuUIElements.SetActive(true); 
        }
    }

    public void ExitGame()
    {
        Application.Quit(); 
    }

    public void ContinueGame()
    {
        Debug.Log("Chức năng Continue: Chờ tích hợp.");
    }
    
    public void OpenTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true); // Hiện bảng hướng dẫn
            menuUIElements.SetActive(false); // Ẩn menu chính
        }
    }

    public void CloseTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false); // Ẩn bảng hướng dẫn
            menuUIElements.SetActive(true); // Hiện lại menu chính
        }
    }

    // --- PHẦN CODE MỚI CHO OPTION MENU ---

    public void OpenOptions()
    {
        optionsPanel.SetActive(true);
        menuUIElements.SetActive(false);
        UpdateOptionsUI(); // Cập nhật lại hình ảnh nút trước khi hiển thị
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        menuUIElements.SetActive(true);
    }

    public void ToggleSound()
    {
        isSoundOn = !isSoundOn; 
        
        // 1. Tắt/mở âm thanh của game thông thường (AudioSource)
        AudioListener.pause = !isSoundOn; 

        // 2. Tắt/mở âm thanh của Video Background
        if (backgroundVideo != null)
        {
            // SetDirectAudioMute(trackIndex, isMuted)
            // Track 0 là kênh âm thanh mặc định đầu tiên của video
            backgroundVideo.SetDirectAudioMute(0, !isSoundOn); 
        }

        UpdateOptionsUI();
    }

    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen; // Đảo ngược trạng thái toàn màn hình
        UpdateOptionsUI();
    }

    // Hàm phụ trợ để xử lý việc tráo đổi hình ảnh
    private void UpdateOptionsUI()
    {
        if (soundBtnImage != null)
        {
            soundBtnImage.sprite = isSoundOn ? iconOn : iconOff;
        }

        if (fullscreenBtnImage != null)
        {
            fullscreenBtnImage.sprite = Screen.fullScreen ? iconOn : iconOff;
        }
    }
}