using UnityEngine;
using UnityEditor;
using System.IO;
using System.Drawing;

public class VoxelBinImporter : EditorWindow
{
    string binPath = "";
    string savePath = "Assets/VoxelModelData.asset";

    [MenuItem("Tools/Voxel Bin Importer")]
    public static void ShowWindow()
    {
        GetWindow<VoxelBinImporter>("Voxel Bin Importer");
    }

    void OnGUI()
    {
        GUILayout.Label("Import .bin to VoxelModelData", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        binPath = EditorGUILayout.TextField("Bin File", binPath);
        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            string path = EditorUtility.OpenFilePanel("Chọn file bin", "", "bin");
            if (!string.IsNullOrEmpty(path))
                binPath = path;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        savePath = EditorGUILayout.TextField("Save Asset As", savePath);
        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            string path = EditorUtility.SaveFilePanelInProject("Lưu Asset", "VoxelModelData.asset", "asset", "");
            if (!string.IsNullOrEmpty(path))
                savePath = path;
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);
        if (GUILayout.Button("Import BIN"))
        {
            ImportBIN(binPath, savePath);
            AssetDatabase.Refresh();
            Debug.Log("Done!");
        }
    }

    void ImportBIN(string binPath, string savePath)
    {
        if (string.IsNullOrEmpty(binPath) || string.IsNullOrEmpty(savePath))
        {
            Debug.LogError("Thiếu đường dẫn file!");
            return;
        }
        using (BinaryReader reader = new BinaryReader(File.Open(binPath, FileMode.Open)))
        {
            int sizeX = reader.ReadInt32();
            int sizeZ = reader.ReadInt32();
            int sizeY = reader.ReadInt32();
            int len = sizeX * sizeY * sizeZ;


            Color32[] data = new Color32[len];
            byte[] regionId = new byte[len];
            for (int i = 0; i < len; i++)
            {
                byte r = reader.ReadByte();
                byte g = reader.ReadByte();
                byte b = reader.ReadByte();
                byte a = reader.ReadByte();
                byte reg = reader.ReadByte();
                data[i] = new Color32(r, g, b, a);
                regionId[i] = reg;
            }

            var asset = ScriptableObject.CreateInstance<VoxelModelData>();
            asset.sx = sizeX;
            asset.sz = sizeZ;
            asset.sy = sizeY;
            asset.data = data;
            asset.regionId = regionId; // Thêm dòng này

            AssetDatabase.CreateAsset(asset, savePath);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            Debug.Log("Import thành công! Asset đã được tạo: " + savePath);
        }
    }

}
