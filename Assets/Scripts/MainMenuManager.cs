using UnityEngine;
using UnityEngine.SceneManagement; // Thư viện quản lý chuyển cảnh

public class MainMenuManager : MonoBehaviour
{
    public void StartNewGame()
    {
        // Hệ thống sẽ tìm và tải scene có tên trùng khớp hoàn toàn với chuỗi này
        SceneManager.LoadScene("SampleScene");
    }

    public void ContinueGame()
    {
        Debug.Log("Chức năng Continue: Chờ tích hợp dữ liệu lưu trữ.");
    }

    public void OpenOptions()
    {
        Debug.Log("Chức năng Options: Chờ khởi tạo Panel Settings.");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}