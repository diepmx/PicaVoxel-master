using UnityEngine;

[CreateAssetMenu(fileName = "VoxelLabelData", menuName = "VoxelModel/Label")]
public class VoxelLabelData : ScriptableObject
{
    public int sx, sy, sz;
    public byte[] data; // Flatten: data[x + sx * (y + sy * z)]

    public byte GetLabel(int x, int y, int z)
    {
        return data[x + sx * (y + sy * z)];
    }

    public byte[,,] To3DArray()
    {
        byte[,,] arr = new byte[sx, sy, sz];
        for (int x = 0; x < sx; x++)
            for (int y = 0; y < sy; y++)
                for (int z = 0; z < sz; z++)
                    arr[x, y, z] = GetLabel(x, y, z);
        return arr;
    }
}
