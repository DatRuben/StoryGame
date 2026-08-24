using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
public sealed class WorldDamageNumber :
    MonoBehaviour
{
    [Header("Position")]

    [SerializeField]
    private float startingWorldOffset = 0.2f;

    [SerializeField]
    [Min(0f)]
    private float riseDistance = 0.75f;

    [SerializeField]
    [Min(0.01f)]
    private float riseDuration = 0.3f;

    [Header("Lifetime")]

    [SerializeField]
    [Min(0.01f)]
    private float lifetimeAfterLastDamage = 0.8f;

    [SerializeField]
    [Range(0f, 1f)]
    private float fadeStartPercent = 0.5f;

    private TextMeshPro text;

    private Camera viewingCamera;

    private Vector3 startingPosition;

    private float totalDamage;

    private float spawnTime;
    private float lastDamageTime;

    private Color startingColor;

    private bool isInitialized;

    private void Awake()
    {
        text =
            GetComponent<TextMeshPro>();

        if (text != null)
        {
            startingColor =
                text.color;
        }
    }

    public void Initialize(
        float amount,
        Vector3 hitPoint,
        Camera camera)
    {
        viewingCamera =
            camera;

        startingPosition =
            hitPoint +
            Vector3.up *
            startingWorldOffset;

        transform.position =
            startingPosition;

        totalDamage =
            Mathf.Max(
                0f,
                amount
            );

        spawnTime =
            Time.time;

        lastDamageTime =
            Time.time;

        if (text != null)
        {
            text.text =
                FormatAmount(
                    totalDamage
                );

            text.color =
                startingColor;
        }

        isInitialized = true;

        FaceCamera();
    }

    public void AddDamage(
        float amount)
    {
        if (!isInitialized)
            return;

        amount =
            Mathf.Max(
                0f,
                amount
            );

        totalDamage +=
            amount;

        lastDamageTime =
            Time.time;

        if (text != null)
        {
            text.text =
                FormatAmount(
                    totalDamage
                );

            text.color =
                startingColor;
        }
    }

    private void LateUpdate()
    {
        if (!isInitialized)
            return;

        float timeSinceSpawn =
            Time.time -
            spawnTime;

        float timeSinceLastDamage =
            Time.time -
            lastDamageTime;

        if (timeSinceLastDamage >=
            lifetimeAfterLastDamage)
        {
            Destroy(gameObject);
            return;
        }

        UpdatePosition(
            timeSinceSpawn
        );

        FaceCamera();

        UpdateFade(
            timeSinceLastDamage
        );
    }

    private void UpdatePosition(
        float timeSinceSpawn)
    {
        float risePercent =
            Mathf.Clamp01(
                timeSinceSpawn /
                riseDuration
            );

        transform.position =
            startingPosition +
            Vector3.up *
            riseDistance *
            risePercent;
    }

    private void FaceCamera()
    {
        if (viewingCamera == null)
            return;

        transform.rotation =
            viewingCamera.transform.rotation;
    }

    private void UpdateFade(
        float timeSinceLastDamage)
    {
        if (text == null)
            return;

        float fadeStartTime =
            lifetimeAfterLastDamage *
            fadeStartPercent;

        float alpha = 1f;

        if (timeSinceLastDamage >
            fadeStartTime)
        {
            float fadeDuration =
                Mathf.Max(
                    0.01f,
                    lifetimeAfterLastDamage -
                    fadeStartTime
                );

            alpha =
                1f -
                Mathf.Clamp01(
                    (
                        timeSinceLastDamage -
                        fadeStartTime
                    ) /
                    fadeDuration
                );
        }

        Color color =
            startingColor;

        color.a *=
            alpha;

        text.color =
            color;
    }

    private string FormatAmount(
        float amount)
    {
        float rounded =
            Mathf.Round(
                amount
            );

        if (Mathf.Abs(
                amount -
                rounded) <
            0.05f)
        {
            return Mathf
                .RoundToInt(
                    amount
                )
                .ToString();
        }

        return amount.ToString(
            "0.0"
        );
    }
}