using UnityEngine;

public class ColorPaletteUI : MonoBehaviour
{
    public void ChonMauDo()    { IslandsClickBoom.MauDangChon = Color.red; }
    public void ChonMauXanhLa() { IslandsClickBoom.MauDangChon = Color.green; }
    public void ChonMauXanhDuong() { IslandsClickBoom.MauDangChon = Color.blue; }
    public void ChonMauVang()  { IslandsClickBoom.MauDangChon = Color.yellow; }
    // Thêm các màu khác nếu muốn
}