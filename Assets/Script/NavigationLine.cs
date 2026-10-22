using UnityEngine;
using UnityEngine.AI;

public class NavigationLine : MonoBehaviour
{
    public static NavigationLine Instance;

    public LineRenderer lineRenderer;
    public Transform playerTransform; // Transform Motor
    private Transform targetTransform;

    [Header("Pengaturan Rute")]
    [Tooltip("Seberapa sering rute dihitung ulang (detik).")]
    public float intervalUpdate = 0.25f;

    [Tooltip("Jarak maksimal mencari titik NavMesh terdekat dari player/target.")]
    public float jarakSampling = 10f;

    [Tooltip("Tinggi garis di atas permukaan jalan.")]
    public float tinggiGaris = 0.3f;

    private NavMeshPath path;
    private float timer = 0f;

    void Awake()
    {
        Instance = this;
        path = new NavMeshPath();
    }

    void Start()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 0;
        lineRenderer.enabled = false;
    }

    void Update()
    {
        if (!lineRenderer.enabled || playerTransform == null || targetTransform == null)
            return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = intervalUpdate;

        HitungRute();
    }

    void HitungRute()
    {
        // Tempelkan posisi player & target ke titik terdekat di NavMesh
        if (!NavMesh.SamplePosition(playerTransform.position, out NavMeshHit hitStart, jarakSampling, NavMesh.AllAreas) ||
            !NavMesh.SamplePosition(targetTransform.position, out NavMeshHit hitEnd, jarakSampling, NavMesh.AllAreas))
        {
            GarisLurusCadangan();
            return;
        }

        if (NavMesh.CalculatePath(hitStart.position, hitEnd.position, NavMesh.AllAreas, path)
            && path.status != NavMeshPathStatus.PathInvalid
            && path.corners.Length >= 2)
        {
            Vector3[] corners = path.corners;
            lineRenderer.positionCount = corners.Length;
            for (int i = 0; i < corners.Length; i++)
            {
                lineRenderer.SetPosition(i, corners[i] + Vector3.up * tinggiGaris);
            }
        }
        else
        {
            GarisLurusCadangan();
        }
    }

    // Kalau NavMesh tidak ketemu, tetap tampilkan garis lurus agar tidak hilang
    void GarisLurusCadangan()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, playerTransform.position + Vector3.up * tinggiGaris);
        lineRenderer.SetPosition(1, targetTransform.position + Vector3.up * tinggiGaris);
    }

    public void SetTarget(Transform newTarget)
    {
        targetTransform = newTarget;
        lineRenderer.enabled = (newTarget != null);
        timer = 0f; // langsung hitung rute baru
    }

    public void ClearTarget()
    {
        targetTransform = null;
        lineRenderer.positionCount = 0;
        lineRenderer.enabled = false;
    }
}