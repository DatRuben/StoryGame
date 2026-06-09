using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private float smoothing = 0.1f;

    private float deltaTime;

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * smoothing;

        float fps = 1f / deltaTime;

        if (fpsText != null)
        {
            fpsText.text = "FPS: " + Mathf.RoundToInt(fps);
        }
    }
}