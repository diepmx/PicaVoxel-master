using UnityEngine;
using PicaVoxel;
using System.Collections.Generic;

public class IslandsClickBoom : MonoBehaviour
{
    static Vector3? lastWorldPos = null;
    public static Color MauDangChon = Color.red;
    public static int brushSize = 1;

    // Lưu danh sách volume đã thay đổi, update mesh sau
    public static HashSet<Volume> volumesToUpdate = new HashSet<Volume>();

    public static void ResetBrushLerp()
    {
        lastWorldPos = null;
        // UpdateAllChunks cho mọi volume vừa paint xong
        foreach (var v in volumesToUpdate)
            v.UpdateAllChunks();
        volumesToUpdate.Clear();
    }

    public static void ToMauTheoScreenPos(Vector2 screenPos)
    {
        var counter = FindObjectOfType<VoxelColorCounter>();
        if (counter == null) return;
        if (!counter.HasInk((Color32)MauDangChon)) return;

        Camera cam = Camera.main;
        Ray ray = cam.ScreenPointToRay(screenPos);

        foreach (Volume volume in GameObject.FindObjectsOfType<Volume>())
        {
            Vector3? worldHit = null;
            for (float d = 0; d < 50f; d += 0.05f)
            {
                Vector3 testPoint = ray.origin + ray.direction * d;
                Voxel? v = volume.GetVoxelAtWorldPosition(testPoint);
                if (v.HasValue && v.Value.Active)
                {
                    worldHit = testPoint;
                    break;
                }
            }
            if (worldHit == null) continue;

            if (lastWorldPos.HasValue)
            {
                float dist = Vector3.Distance(lastWorldPos.Value, worldHit.Value);
                int steps = Mathf.CeilToInt(dist / 0.5f);
                for (int i = 0; i <= steps; i++)
                {
                    Vector3 lerpPoint = Vector3.Lerp(lastWorldPos.Value, worldHit.Value, i / (float)steps);
                    PaintBrush(volume, lerpPoint, (Color32)MauDangChon, counter, brushSize);
                }
            }
            else
            {
                PaintBrush(volume, worldHit.Value, (Color32)MauDangChon, counter, brushSize);
            }

            lastWorldPos = worldHit.Value;
            return;
        }
        lastWorldPos = null;
    }

    static void PaintBrush(Volume volume, Vector3 worldPos, Color32 color, VoxelColorCounter counter, int brushSize)
    {
        Vector3 localPos = volume.transform.InverseTransformPoint(worldPos);
        int bx = Mathf.FloorToInt(localPos.x);
        int by = Mathf.FloorToInt(localPos.y);
        int bz = Mathf.FloorToInt(localPos.z);

        int xSize = volume.XSize, ySize = volume.YSize, zSize = volume.ZSize;
        var frame = volume.GetCurrentFrame();
        var voxels = frame.Voxels;

        if (bx < 0 || bx >= xSize || by < 0 || by >= ySize || bz < 0 || bz >= zSize)
            return;

        bool painted = false;
        for (int dx = -brushSize + 1; dx < brushSize; dx++)
            for (int dy = -brushSize + 1; dy < brushSize; dy++)
                for (int dz = -brushSize + 1; dz < brushSize; dz++)
                {
                    int x = bx + dx, y = by + dy, z = bz + dz;
                    if (x < 0 || x >= xSize || y < 0 || y >= ySize || z < 0 || z >= zSize) continue;
                    int idx = x + y * xSize + z * xSize * ySize;
                    var v = voxels[idx];
                    if (!v.Active) continue;
                    if (v.Color.Equals(color)) continue;
                    if (!counter.HasInk(color)) return;
                    if (counter.TryUseInk(color))
                    {
                        v.Color = color;
                        voxels[idx] = v;
                        painted = true;
                    }
                }
        if (painted) volumesToUpdate.Add(volume); // Đánh dấu volume này cần update sau
    }
}
