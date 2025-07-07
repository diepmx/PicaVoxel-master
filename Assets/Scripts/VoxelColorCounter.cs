using UnityEngine;
using TMPro;
using PicaVoxel;

public class VoxelColorCounter : MonoBehaviour
{
    public Volume volume;
    public Color32 color1; // xanh lá
    public Color32 color2; // đỏ
    public Color32 color3; // xanh dương

    public int inkColor1 = 100;
    public int inkColor2 = 100;
    public int inkColor3 = 100;

    public TMP_Text textColor1;
    public TMP_Text textColor2;
    public TMP_Text textColor3;

    public void UpdateInkUI()
    {
        if (textColor1 != null) textColor1.text = inkColor1.ToString();
        if (textColor2 != null) textColor2.text = inkColor2.ToString();
        if (textColor3 != null) textColor3.text = inkColor3.ToString();
    }

    public bool TryUseInk(Color32 color)
    {
        if (color.Equals(color1) && inkColor1 > 0) { inkColor1--; UpdateInkUI(); return true; }
        if (color.Equals(color2) && inkColor2 > 0) { inkColor2--; UpdateInkUI(); return true; }
        if (color.Equals(color3) && inkColor3 > 0) { inkColor3--; UpdateInkUI(); return true; }
        return false;
    }

    public bool HasInk(Color32 color)
    {
        if (color.Equals(color1)) return inkColor1 > 0;
        if (color.Equals(color2)) return inkColor2 > 0;
        if (color.Equals(color3)) return inkColor3 > 0;
        return false;
    }
}
