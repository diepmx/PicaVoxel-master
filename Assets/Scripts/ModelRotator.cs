using UnityEngine;

public class ModelRotator : MonoBehaviour
{
    public float tocDoXoay = 30f;
    public float nguongKichHoatXoay = 15f;
    public bool isPaintMode = false; // false = Xoay, true = Tô

    // PC
    private Vector3 mouseDownPos;
    private bool draggedMouse = false;

    // Mobile
    private Vector2 touchDownPos;
    private bool draggedTouch = false;

    // Đổi chế độ khi gọi từ UI Button
    public void TogglePaintMode()
    {
        isPaintMode = !isPaintMode;
        Debug.Log("Chế độ hiện tại: " + (isPaintMode ? "TÔ" : "XOAY"));
        // Có thể cập nhật text/nút trên UI ở đây
    }
    void Awake()
    {
        Application.targetFrameRate = 60;
    }

    void Update()
    {
        // --- TOUCH MOBILE ---
        if (Input.touchSupported && Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchDownPos = touch.position;
                draggedTouch = false;
                if (isPaintMode)
                    IslandsClickBoom.ResetBrushLerp(); // RESET đầu drag
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.position - touchDownPos;
                if (delta.magnitude > nguongKichHoatXoay)
                {
                    if (!isPaintMode)
                    {
                        // Xoay
                        float xoayY = -delta.x * tocDoXoay * Time.deltaTime * 0.1f;
                        float xoayX = delta.y * tocDoXoay * Time.deltaTime * 0.1f;
                        transform.Rotate(Vector3.up, xoayY, Space.World);
                        transform.Rotate(Vector3.right, xoayX, Space.World);
                    }
                    else
                    {
                        // Tô liên tục khi kéo
                        IslandsClickBoom.ToMauTheoScreenPos(touch.position);
                    }
                    draggedTouch = true;
                    touchDownPos = touch.position; // reset để drag tiếp mượt
                }
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                if (isPaintMode)
                    IslandsClickBoom.ResetBrushLerp(); // RESET cuối drag

                if (isPaintMode && !draggedTouch)
                {
                    //Debug.Log("Tap mobile: gọi tô màu");
                    IslandsClickBoom.ToMauTheoScreenPos(touch.position);
                }
                draggedTouch = false;
            }
            else if (touch.phase == TouchPhase.Stationary && isPaintMode)
            {
                // Nếu muốn hỗ trợ "kéo để tô", có thể gọi thêm ToMauTheoScreenPos ở đây
            }
            return;
        }

        // --- CHUỘT PC ---
        if (Input.GetMouseButtonDown(0))
        {
            mouseDownPos = Input.mousePosition;
            draggedMouse = false;
            if (isPaintMode)
                IslandsClickBoom.ResetBrushLerp(); // RESET đầu drag
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - mouseDownPos;
            if (delta.magnitude > nguongKichHoatXoay)
            {
                if (!isPaintMode)
                {
                    // Xoay
                    float xoayY = -delta.x * tocDoXoay * Time.deltaTime;
                    float xoayX = delta.y * tocDoXoay * Time.deltaTime;
                    transform.Rotate(Vector3.up, xoayY, Space.World);
                    transform.Rotate(Vector3.right, xoayX, Space.World);
                }
                else
                {
                    // Tô liên tục khi kéo
                    IslandsClickBoom.ToMauTheoScreenPos(Input.mousePosition);
                }
                draggedMouse = true;
                mouseDownPos = Input.mousePosition;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (isPaintMode)
                IslandsClickBoom.ResetBrushLerp(); // RESET cuối drag

            if (isPaintMode && !draggedMouse)
            {
                //Debug.Log("Click PC: gọi tô màu");
                IslandsClickBoom.ToMauTheoScreenPos(Input.mousePosition);
            }
            draggedMouse = false;
        }
    }
    void LateUpdate()
    {
        // Update mesh 1 lần/frame nếu có volume tô mới
        if (IslandsClickBoom.volumesToUpdate != null && IslandsClickBoom.volumesToUpdate.Count > 0)
        {
            foreach (var v in IslandsClickBoom.volumesToUpdate)
                v.UpdateAllChunks();
            IslandsClickBoom.volumesToUpdate.Clear();
        }
    }

}
