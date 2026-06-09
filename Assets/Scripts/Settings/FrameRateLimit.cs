using UnityEngine;

public class FrameRateLimit : MonoBehaviour
{
    [SerializeField] private int targetFrameRate = 120;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFrameRate;
    }
}