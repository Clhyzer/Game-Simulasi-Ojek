using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public static NPCSpawner Instance;

    [Header("Prefabs")]
    public GameObject npcPrefab;
    public GameObject dropZonePrefab;

    [Header("Locations")]
    public Transform[] spawnPoints;     // Titik jemput
    public Transform[] dropOffPoints;   // Titik antar

    private GameObject currentNPC;
    private GameObject currentDropZone;
    private bool sudahDijemput = false;

    void Awake()
    {
        Instance = this;
    }

    public void SpawnNPC()
    {
        HapusSemuaJob();
        sudahDijemput = false;

        if (spawnPoints.Length == 0 || npcPrefab == null) return;

        // 1. Spawn NPC
        Transform randomSpawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
        currentNPC = Instantiate(npcPrefab, randomSpawn.position, randomSpawn.rotation);

        // 2. Arahkan garis navigasi ke NPC
        if (NavigationLine.Instance != null)
        {
            NavigationLine.Instance.SetTarget(currentNPC.transform);
        }
    }

    // Dipanggil dari PickUpTrigger saat menekan E
    public void PenumpangDinaikkan()
    {
        sudahDijemput = true;

        // 1. Spawn Drop Zone di titik tujuan acak
        if (dropOffPoints.Length > 0 && dropZonePrefab != null)
        {
            Transform randomDrop = dropOffPoints[Random.Range(0, dropOffPoints.Length)];
            currentDropZone = Instantiate(dropZonePrefab, randomDrop.position, randomDrop.rotation);

            // 2. Pindahkan garis navigasi menunjuk ke Drop Zone
            if (NavigationLine.Instance != null)
            {
                NavigationLine.Instance.SetTarget(currentDropZone.transform);
            }
        }
    }

    public void HapusSemuaJob()
    {
        if (currentNPC != null) Destroy(currentNPC);
        if (currentDropZone != null) Destroy(currentDropZone);

        if (NavigationLine.Instance != null)
        {
            NavigationLine.Instance.ClearTarget();
        }
    }

    public bool CekSudahDijemput() => sudahDijemput;
}