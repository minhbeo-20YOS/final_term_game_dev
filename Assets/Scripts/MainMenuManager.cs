using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI & Transition References")]
    public GameObject menuUIElements; // Kéo object chứa các nút bấm vào đây
    public VideoPlayer transitionVideo; // Kéo TransitionVideoUI vào đây
    public CanvasGroup fadePanel; // Kéo FadePanel vào đây
    
    [Header("Exit Popup References")]
    public GameObject exitConfirmationPanel;
    
    [Header("Settings")]
    public float fadeDuration = 1.5f; // Thời gian chuyển đen
    public string nextSceneName = "SampleScene";

    public void StartNewGame()
    {
        // Vô hiệu hóa nút bấm ngay lập tức để tránh double-click
        menuUIElements.SetActive(false); 
        
        // Bắt đầu chuỗi sự kiện chuyển cảnh
        StartCoroutine(PlayCutsceneAndLoad());
    }

    private IEnumerator PlayCutsceneAndLoad()
    {
        // 1. Kích hoạt và phát video
        transitionVideo.gameObject.SetActive(true);
        transitionVideo.Play();

        // 2. Chờ video buffer xong
        while (!transitionVideo.isPrepared)
        {
            yield return null;
        }

        // 3. Đợi video chạy hết thời lượng
        // Sử dụng transitionVideo.length để lấy tổng thời gian chính xác của file video
        yield return new WaitForSeconds((float)transitionVideo.length);

        // 4. Bắt đầu hiệu ứng Fade to Black
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            // Tăng dần Alpha từ 0 lên 1
            fadePanel.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            yield return null;
        }

        // Đảm bảo màn hình hoàn toàn đen
        fadePanel.alpha = 1f;

        // 5. Load Scene mới
        SceneManager.LoadScene(nextSceneName);
    }

    public void ContinueGame()
    {
        Debug.Log("Chức năng Continue: Chờ tích hợp dữ liệu lưu trữ.");
    }

    public void OpenOptions()
    {
        Debug.Log("Chức năng Options: Chờ khởi tạo Panel Settings.");
    }
    
    
    
    public void OpenExitPopup()
    {
        if (exitConfirmationPanel != null)
        {
            exitConfirmationPanel.SetActive(true); // Hiện bảng xác nhận lên
            menuUIElements.SetActive(false); // Ẩn các nút bấm chính đi để người chơi không bấm nhầm
        }
    }

    // Hàm này gắn vào nút NO để đóng bảng xác nhận, quay lại Main Menu
    public void CloseExitPopup()
    {
        if (exitConfirmationPanel != null)
        {
            exitConfirmationPanel.SetActive(false); // Ẩn bảng xác nhận đi
            menuUIElements.SetActive(true); // Hiện lại các nút bấm chính ở Main Menu
        }
    }

    // Hàm này gắn vào nút YES để thoát hẳn game
    public void ExitGame()
    {
        Debug.Log("Đang thoát game..."); // Lệnh này để bạn thấy nó hoạt động khi test trong Unity Editor
        Application.Quit(); // Lệnh thoát game thực tế khi build ra máy tính/điện thoại
    }
}