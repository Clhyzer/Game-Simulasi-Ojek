using UnityEngine;

public class NavigationLine : MonoBehaviour
{
    public static NavigationLine Instance;

    public LineRenderer lineRenderer;
    public Transform playerTransform; // Transform Motor
    private Transform targetTransform; // Target yang dituju (NPC / Drop Zone)

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
    }

    void Update()
    {
        if (lineRenderer.enabled && playerTransform != null && targetTransform != null)
        {
            // Garis ditarik dari motor ke target
            lineRenderer.SetPosition(0, playerTransform.position + Vector3.up * 0.2f);
            lineRenderer.SetPosition(1, targetTransform.position + Vector3.up * 0.2f);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        targetTransform = newTarget;
        lineRenderer.enabled = (newTarget != null);
    }

    public void ClearTarget()
    {
        targetTransform = null;
        lineRenderer.enabled = false;
    }
}