using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FPSDisplay : MonoBehaviour
{
    public TMP_Text fpsText; // Gán trong Inspector
    float deltaTime;
    void Awake()
    {
        Application.targetFrameRate = 60; // thử 60, nếu máy hỗ trợ thì tự động lên 60
    }

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        fpsText.text = $"FPS: {fps:0.}";
    }
}
