using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using PicaVoxel;
using TMPro;

public class VoxelColorCounter : MonoBehaviour
{
    public Volume volume;
    public Color32 color1; // VD: xanh lá
    public Color32 color2; // VD: đỏ
    public Color32 color3; // VD: xanh dương

    public TMP_Text textColor1;
    public TMP_Text textColor2;
    public TMP_Text textColor3;

    public void UpdateColorCount()
    {
        Debug.Log("UpdateColorCount() called");
        int count1 = 0, count2 = 0, count3 = 0;

        var voxels = volume.GetCurrentFrame().Voxels;
        foreach (var voxel in voxels)
        {
            if (voxel.Active)
            {
                Debug.Log($"Voxel color: {voxel.Color} | color1: {color1} | color2: {color2} | color3: {color3}");
                if (voxel.Color.Equals(color1)) count1++;
                else if (voxel.Color.Equals(color2)) count2++;
                else if (voxel.Color.Equals(color3)) count3++;
            }
        }
        Debug.Log($"KQ: {count1} - {count2} - {count3}");

        if (textColor1 != null) textColor1.text = count1.ToString();
        if (textColor2 != null) textColor2.text = count2.ToString();
        if (textColor3 != null) textColor3.text = count3.ToString();
    }

}
