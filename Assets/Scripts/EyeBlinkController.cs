using System.Collections;
using UnityEngine;

public class EyeBlinkController : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform topLid;
    public RectTransform bottomLid;

    [Header("Timing")]
    public float wakeUpDuration = 2.5f; // Tổng thời lượng mở mắt

    private float _topTargetY;
    private float _bottomTargetY;

    void Start()
    {
        // Tính toán khoảng cách cần dịch chuyển dựa trên chiều cao thực tế của mí mắt
        _topTargetY = topLid.rect.height; 
        _bottomTargetY = -bottomLid.rect.height;

        StartCoroutine(ExecuteEyeOpening());
    }

    private IEnumerator ExecuteEyeOpening()
    {
        // Ghi nhận vị trí ban đầu
        Vector2 topStartPos = topLid.anchoredPosition;
        Vector2 bottomStartPos = bottomLid.anchoredPosition;

        // Xác định vị trí đích (vượt ra ngoài viền màn hình)
        Vector2 topEndPos = new Vector2(topStartPos.x, _topTargetY);
        Vector2 bottomEndPos = new Vector2(bottomStartPos.x, _bottomTargetY);

        float elapsedTime = 0f;

        while (elapsedTime < wakeUpDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / wakeUpDuration;
            
            // Áp dụng đường cong Smoothstep (tương đương với Ease In-Out)
            // t = t * t * (3f - 2f * t);

            // Hoặc dùng đường cong Ease Out (nhanh ở đầu, chậm dần ở cuối cho tự nhiên hơn)
            t = 1f - Mathf.Pow(1f - t, 3f); 

            // Cập nhật vị trí mỗi frame
            topLid.anchoredPosition = Vector2.Lerp(topStartPos, topEndPos, t);
            bottomLid.anchoredPosition = Vector2.Lerp(bottomStartPos, bottomEndPos, t);

            yield return null;
        }

        // Đảm bảo mí mắt mở hoàn toàn và tắt object để giải phóng tài nguyên tính toán
        topLid.anchoredPosition = topEndPos;
        bottomLid.anchoredPosition = bottomEndPos;
        gameObject.SetActive(false);
    }
}