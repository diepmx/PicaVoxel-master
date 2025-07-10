using UnityEngine;

[CreateAssetMenu(fileName = "VoxelModelData", menuName = "VoxelModel/Data")]
public class VoxelModelData : ScriptableObject
{
    public int sx, sz, sy;   // THỨ TỰ sx, sz, sy (giống khi ghi file)
    public Color32[] data; // Flatten: data[x + sx * (y + sy * z)]
    public byte[] regionId;

    // Chú ý: y thực tế là z của file cũ, z là y
    public Color32 GetColor(int x, int y, int z)
    {
        return data[x + sx * (y + sy * z)];
    }
    public byte GetRegion(int x, int y, int z)
    {
        return regionId[x + sx * (y + sy * z)];
    }
    public Color[,,] To3DArray()
    {
        Color[,,] arr = new Color[sx, sy, sz];
        for (int x = 0; x < sx; x++)
            for (int y = 0; y < sy; y++)
                for (int z = 0; z < sz; z++)
                    arr[x, y, z] = GetColor(x, y, z); // Đúng thứ tự
        return arr;
    }
}
