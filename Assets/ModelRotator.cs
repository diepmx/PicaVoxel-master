using UnityEngine;

public class ModelRotator : MonoBehaviour
{
    public float tocDoXoay = 30f;
    public float nguongKichHoatXoay = 15f; // tăng ngưỡng để tránh lẫn tap và drag

    // PC
    private Vector3 mouseDownPos;
    private bool draggedMouse = false;

    // Mobile
    private Vector2 touchDownPos;
    private bool draggedTouch = false;

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
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.position - touchDownPos;
                if (delta.magnitude > nguongKichHoatXoay)
                {
                    float xoayY = -delta.x * tocDoXoay * Time.deltaTime * 0.1f;
                    float xoayX = delta.y * tocDoXoay * Time.deltaTime * 0.1f;
                    transform.Rotate(Vector3.up, xoayY, Space.World);
                    transform.Rotate(Vector3.right, xoayX, Space.World);
                    draggedTouch = true;
                    touchDownPos = touch.position; // reset để drag tiếp mượt
                }
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                if (!draggedTouch)
                {
                    Debug.Log("Tap mobile: gọi tô màu");
                    IslandsClickBoom.ToMauTheoScreenPos(touch.position);
                }
                draggedTouch = false; // reset
            }
            return;
        }

        // --- CHUỘT PC ---
        if (Input.GetMouseButtonDown(0))
        {
            mouseDownPos = Input.mousePosition;
            draggedMouse = false;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - mouseDownPos;
            if (delta.magnitude > nguongKichHoatXoay)
            {
                float xoayY = -delta.x * tocDoXoay * Time.deltaTime;
                float xoayX = delta.y * tocDoXoay * Time.deltaTime;
                transform.Rotate(Vector3.up, xoayY, Space.World);
                transform.Rotate(Vector3.right, xoayX, Space.World);
                draggedMouse = true;
                mouseDownPos = Input.mousePosition; // reset để drag tiếp mượt
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (!draggedMouse)
            {
                Debug.Log("Click PC: gọi tô màu");
                IslandsClickBoom.ToMauTheoScreenPos(Input.mousePosition);
            }
            draggedMouse = false; // reset
        }
    }
}
