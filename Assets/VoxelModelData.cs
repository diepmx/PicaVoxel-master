using UnityEngine;

[CreateAssetMenu(fileName = "VoxelModelData", menuName = "VoxelModel/Data")]
public class VoxelModelData : ScriptableObject
{
    public int sx, sy, sz;
    public Color32[] data; // Flatten: data[x + sx * (y + sy * z)]

    // Lấy màu voxel tại vị trí (x, y, z)
    public Color32 GetColor(int x, int y, int z)
    {
        return data[x + sx * (y + sy * z)];
    }

    // Trả về mảng Color[,,] cho ModelPlace hoặc các hàm legacy
    public Color[,,] To3DArray()
    {
        Color[,,] arr = new Color[sx, sy, sz];
        for (int x = 0; x < sx; x++)
            for (int y = 0; y < sy; y++)
                for (int z = 0; z < sz; z++)
                    arr[x, y, z] = GetColor(x, y, z);
        return arr;
    }
}
