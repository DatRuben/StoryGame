
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public sealed class HitscanBeamVisual : MonoBehaviour
{
    [SerializeField, Min(0.01f)]
    private float duration = 0.2f;

    private LineRenderer line;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    public void Show(Vector3 start, Vector3 end)
    {
        line.useWorldSpace = true;
        line.positionCount = 2;

        line.SetPosition(0, start);
        line.SetPosition(1, end);

        Destroy(gameObject, duration);
    }
}
