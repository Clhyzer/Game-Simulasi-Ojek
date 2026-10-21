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

    private Transform targetSpawnPoint;
    private Transform targetDropOffPoint;

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Menyiapkan titik jemput dan antar untuk pesanan baru, serta menghitung jaraknya.
    /// </summary>
    public bool SiapkanOrderanBaru(out float jarak)
    {
        jarak = 50f;
        if (spawnPoints == null || spawnPoints.Length == 0) return false;
        if (dropOffPoints == null || dropOffPoints.Length == 0) return false;

        targetSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        targetDropOffPoint = dropOffPoints[Random.Range(0, dropOffPoints.Length)];

        jarak = Vector3.Distance(targetSpawnPoint.position, targetDropOffPoint.position);
        return true;
    }

    public void SpawnNPC()
    {
        HapusSemuaJob();
        sudahDijemput = false;

        if (spawnPoints.Length == 0 || npcPrefab == null) return;

        // Gunakan titik yang sudah dipilih sebelumnya, jika belum ada pilih acak
        Transform spawnLoc = targetSpawnPoint != null 
            ? targetSpawnPoint 
            : spawnPoints[Random.Range(0, spawnPoints.Length)];

        currentNPC = Instantiate(npcPrefab, spawnLoc.position, spawnLoc.rotation);

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

        // 1. Spawn Drop Zone di titik tujuan yang sudah ditentukan
        if (dropOffPoints.Length > 0 && dropZonePrefab != null)
        {
            Transform dropLoc = targetDropOffPoint != null 
                ? targetDropOffPoint 
                : dropOffPoints[Random.Range(0, dropOffPoints.Length)];

            currentDropZone = Instantiate(dropZonePrefab, dropLoc.position, dropLoc.rotation);

            // Pastikan DropZoneTrigger terpasang di objek drop zone
            if (currentDropZone.GetComponent<DropZoneTrigger>() == null)
            {
                currentDropZone.AddComponent<DropZoneTrigger>();
            }

            // 2. Pindahkan garis navigasi menunjuk ke Drop Zone
            if (NavigationLine.Instance != null)
            {
                NavigationLine.Instance.SetTarget(currentDropZone.transform);
            }
        }

        // 3. Beri tahu aplikasi ojek bahwa proses mengantar dimulai (timer mulai berjalan)
        if (PhoneOjekApp.Instance != null)
        {
            PhoneOjekApp.Instance.MulaiPengantaran();
        }
    }

    public void HapusSemuaJob()
    {
        if (SphereMotorController.Instance != null)
        {
            SphereMotorController.Instance.TurunkanPenumpang();
        }

        if (currentNPC != null)
        {
            Destroy(currentNPC);
            currentNPC = null;
        }

        if (currentDropZone != null)
        {
            Destroy(currentDropZone);
            currentDropZone = null;
        }

        targetSpawnPoint = null;
        targetDropOffPoint = null;

        if (NavigationLine.Instance != null)
        {
            NavigationLine.Instance.ClearTarget();
        }
    }

    public bool CekSudahDijemput() => sudahDijemput;
}
