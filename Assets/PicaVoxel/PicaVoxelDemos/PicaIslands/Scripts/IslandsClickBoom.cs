using UnityEngine;
using PicaVoxel;

public class IslandsClickBoom : MonoBehaviour
{
    static Vector3? lastWorldPos = null;
    public static Color MauDangChon = Color.red;
    public static int brushSize = 1; // size cọ (1: chấm, 2: vuông 3x3...)

    public static void ResetBrushLerp()
    {
        lastWorldPos = null;
    }

    public static void ToMauTheoScreenPos(Vector2 screenPos)
    {
        Debug.Log("Gọi tô màu tại: " + screenPos);

        var counter = FindObjectOfType<VoxelColorCounter>();
        if (counter == null)
        {
            Debug.LogWarning("Không tìm thấy VoxelColorCounter!");
            return;
        }

        if (!counter.HasInk((Color32)MauDangChon))
        {
            Debug.Log("Hết mực màu này rồi!");
            return;
        }

        Camera cam = Camera.main;
        Ray ray = cam.ScreenPointToRay(screenPos);

        // Duyệt từng volume để kiểm tra ray trúng voxel nào (fallback chuẩn nhất)
        foreach (Volume volume in GameObject.FindObjectsOfType<Volume>())
        {
            // Nội suy mượt giữa các điểm kéo (vẽ liên tục)
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
                int steps = Mathf.CeilToInt(dist / 0.2f);
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
            return; // Đã tô ở 1 volume là đủ (dừng)
        }

        // Nếu không trúng volume nào thì reset chuỗi vẽ mượt
        lastWorldPos = null;
    }

    // Hàm tô cả vùng brush (cube) quanh vị trí worldPos
    static void PaintBrush(Volume volume, Vector3 worldPos, Color32 color, VoxelColorCounter counter, int brushSize)
    {
        Vector3 localPos = volume.transform.InverseTransformPoint(worldPos);
        int bx = Mathf.FloorToInt(localPos.x);
        int by = Mathf.FloorToInt(localPos.y);
        int bz = Mathf.FloorToInt(localPos.z);

        int xSize = volume.XSize, ySize = volume.YSize, zSize = volume.ZSize;
        var frame = volume.GetCurrentFrame();
        var voxels = frame.Voxels;

        bool anyChanged = false; // <-- Chỉ update nếu có thay đổi

        if (bx < 0 || bx >= xSize || by < 0 || by >= ySize || bz < 0 || bz >= zSize)
            return;

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
                        anyChanged = true; // Đánh dấu đã thay đổi
                    }
                }
        if (anyChanged)
            volume.UpdateAllChunks(); // <-- Chỉ update nếu có voxel được tô
    }

}
