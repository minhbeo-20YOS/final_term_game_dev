public static class GameData
{
    public static float PlayerHP = 100f; 
    public static int CurrentDay = 1;    

    // HÀM THÊM VÀO: Gọi phát là đưa mọi thứ về vạch xuất phát
    public static void ResetGame()
    {
        PlayerHP = 100f;  // Trả máu về 100 hoàn hảo
        CurrentDay = 1;   // Reset lại từ Ngày 1
        
        // Sau này có thêm điểm số hay vật phẩm gì thì cứ nhét vào đây để reset luôn
    }
}