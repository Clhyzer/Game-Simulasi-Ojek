using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class PickUpTrigger : MonoBehaviour
{
    [Header("UI Prompt")]
    [SerializeField] private string teksPrompt = "Tekan <b>[E]</b> untuk menjemput penumpang";

    private bool isPlayerNearby = false;
    private bool sudahDijemput = false;
    private SphereMotorController nearbyMotor;

    // UI dipakai bersama oleh semua PickUpTrigger
    private static GameObject promptRoot;
    private static TMP_Text promptText;

    void Update()
    {
        // Gunakan pengecekan tombol E versi Input System baru
        if (isPlayerNearby && !sudahDijemput && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            AmbilPenumpang();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (sudahDijemput) return;

        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;

            // Cari controller motor dari collider yang masuk
            nearbyMotor = other.GetComponentInParent<SphereMotorController>();
            if (nearbyMotor == null)
            {
                nearbyMotor = SphereMotorController.Instance;
            }

            TampilkanPrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            nearbyMotor = null;
            SembunyikanPrompt();
        }
    }

    void AmbilPenumpang()
    {
        sudahDijemput = true;
        isPlayerNearby = false;
        SembunyikanPrompt();

        Debug.Log("Penumpang berhasil naik ke motor!");

        // 1. Cari controller motor
        if (nearbyMotor == null)
        {
            nearbyMotor = SphereMotorController.Instance;
            if (nearbyMotor == null)
            {
                nearbyMotor = FindFirstObjectByType<SphereMotorController>();
            }
        }

        // 2. Naikkan NPC ke jok belakang motor
        if (nearbyMotor != null)
        {
            nearbyMotor.NaikkanPenumpang(gameObject);
        }
        else
        {
            Debug.LogWarning("SphereMotorController tidak ditemukan untuk menaikkan penumpang!");
        }

        // 3. Beri tahu spawner untuk spawn drop zone dan pindahkan rute navigasi
        if (NPCSpawner.Instance != null)
        {
            NPCSpawner.Instance.PenumpangDinaikkan();
        }

        // CATATAN: Destroy(gameObject) dihapus agar NPC tidak langsung musnah dan tetap berada di atas motor
    }

    // Kalau NPC dihapus/dinonaktifkan saat player masih di dekatnya, prompt ikut hilang
    private void OnDisable()
    {
        if (isPlayerNearby) SembunyikanPrompt();
    }

    private void OnDestroy()
    {
        if (isPlayerNearby) SembunyikanPrompt();
    }

    // --- UI PROMPT ---

    void TampilkanPrompt()
    {
        BuatPromptJikaBelumAda();
        if (promptText != null) promptText.text = teksPrompt;
        if (promptRoot != null) promptRoot.SetActive(true);
    }

    static void SembunyikanPrompt()
    {
        if (promptRoot != null) promptRoot.SetActive(false);
    }

    static void BuatPromptJikaBelumAda()
    {
        if (promptRoot != null) return;

        // Canvas khusus supaya tidak bergantung pada canvas lain di scene
        GameObject canvasObj = new GameObject("PickUpPromptCanvas", typeof(Canvas), typeof(CanvasScaler));
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // Panel di tengah bawah layar
        promptRoot = new GameObject("PromptPickUp", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        promptRoot.transform.SetParent(canvasObj.transform, false);

        RectTransform rt = promptRoot.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 140f);
        rt.sizeDelta = new Vector2(520f, 70f);

        promptRoot.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.14f, 0.88f);

        // Teks
        GameObject txtObj = new GameObject("TextPrompt", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        txtObj.transform.SetParent(promptRoot.transform, false);

        RectTransform tr = txtObj.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = new Vector2(15f, 5f);
        tr.offsetMax = new Vector2(-15f, -5f);

        TextMeshProUGUI tmp = txtObj.GetComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null) tmp.font = TMP_Settings.defaultFontAsset;
        tmp.fontSize = 30;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        promptText = tmp;

        promptRoot.SetActive(false);
    }
}