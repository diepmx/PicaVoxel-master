using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetToTextExporter
{
    [MenuItem("Tools/Export VoxelModelData To Text")]
    public static void ExportVoxelModelDataToText()
    {
        // Chọn file asset trong Project trước khi chạy
        var obj = Selection.activeObject as VoxelModelData;
        if (obj == null)
        {
            Debug.LogError("Chọn VoxelModelData asset trước!");
            return;
        }
        string path = EditorUtility.SaveFilePanel("Save Text File", "", obj.name + ".txt", "txt");
        if (string.IsNullOrEmpty(path)) return;

        using (StreamWriter sw = new StreamWriter(path))
        {
            sw.WriteLine($"sx={obj.sx}, sy={obj.sy}, sz={obj.sz}");
            for (int x = 0; x < obj.sx; x++)
                for (int y = 0; y < obj.sy; y++)
                    for (int z = 0; z < obj.sz; z++)
                    {
                        Color32 c = obj.GetColor(x, y, z);
                        if (c.a > 0)
                            sw.WriteLine($"{x},{y},{z},{c.r},{c.g},{c.b},{c.a}");
                    }
        }
        Debug.Log("Đã xuất xong file text: " + path);
    }
}
