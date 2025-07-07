using UnityEngine;
using UnityEngine.UI;
using TMPro; // Nếu dùng TextMeshPro

public class BrushSizeUI : MonoBehaviour
{
    public TextMeshProUGUI brushSizeText; // Kéo thả vào Inspector
    public int minBrush = 1;
    public int maxBrush = 8;

    void Start()
    {
        UpdateUI();
    }

    public void IncreaseBrush()
    {
        if (IslandsClickBoom.brushSize < maxBrush)
            IslandsClickBoom.brushSize++;
        UpdateUI();
    }

    public void DecreaseBrush()
    {
        if (IslandsClickBoom.brushSize > minBrush)
            IslandsClickBoom.brushSize--;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (brushSizeText)
            brushSizeText.text = $"Brush: {IslandsClickBoom.brushSize}";
    }
}
