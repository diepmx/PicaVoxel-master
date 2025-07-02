using UnityEngine;
using PicaVoxel; // Chỉ cần nếu bạn dùng các hàm PicaVoxel

public class IslandsClickBoom : MonoBehaviour
{
    public static Color MauDangChon = Color.red;

    public static void ToMauTheoScreenPos(Vector2 screenPos)
    {
        Debug.Log("Gọi tô màu tại: " + screenPos);

        Camera cam = Camera.main;
        Ray ray = cam.ScreenPointToRay(screenPos);

        // Ưu tiên raycast như cũ
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            Volume volume = hit.collider.GetComponentInParent<Volume>();
            if (volume != null)
            {
                Voxel? v = volume.GetVoxelAtWorldPosition(hit.point);
                if (v.HasValue && v.Value.Active)
                {
                    Voxel newVoxel = v.Value;
                    newVoxel.Color = MauDangChon;
                    volume.SetVoxelAtWorldPosition(hit.point, newVoxel);
                    volume.UpdateAllChunks();
                    Debug.Log("Tô màu thành công!");
                    FindObjectOfType<VoxelColorCounter>()?.UpdateColorCount();

                    return;
                }
            }
        }

        // Fallback: Quét ray thủ công nếu raycast không tìm thấy voxel active
        foreach (Volume volume in GameObject.FindObjectsOfType<Volume>())
        {
            for (float d = 0; d < 50f; d += 0.05f)
            {
                Vector3 testPoint = ray.origin + ray.direction * d;
                Voxel? v = volume.GetVoxelAtWorldPosition(testPoint);
                if (v.HasValue && v.Value.Active)
                {
                    Voxel newVoxel = v.Value;
                    newVoxel.Color = MauDangChon;
                    volume.SetVoxelAtWorldPosition(testPoint, newVoxel);
                    volume.UpdateAllChunks();
                    Debug.Log($"Tô màu fallback tại {testPoint}");
                    FindObjectOfType<VoxelColorCounter>()?.UpdateColorCount();
                    return;
                }
            }
        }
        Debug.Log("Không có voxel active nào dọc theo ray.");
    }

}
