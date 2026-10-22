using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; private set; }

    /// <summary>
    /// Cek dari script lain: if (PauseMenu.IsPaused) return;
    /// </summary>
    public static bool IsPaused { get; private set; }

    [Header("UI (Opsional - Dibuat otomatis jika kosong)")]
    public GameObject panelPause;
    public Button btnLanjut;
    public Button btnRestart;
    public Button btnKeluar;

    [Header("Pengaturan")]
    [Tooltip("Pause juga semua suara (suara mesin, SFX, dll).")]
    public bool pauseAudio = true;

    [Tooltip("Tampilkan kursor mouse saat pause.")]
    public bool tampilkanKursorSaatPause = true;

    private bool kursorSebelumnyaTampil;
    private CursorLockMode kursorLockSebelumnya;

    void Awake()
    {
        Instance = this;
        IsPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    void Start()
    {
        if (panelPause == null)
        {
            BuatUIOtomatis();
        }
        else
        {
            if (btnLanjut != null) btnLanjut.onClick.AddListener(Lanjutkan);
            if (btnRestart != null) btnRestart.onClick.AddListener(Restart);
            if (btnKeluar != null) btnKeluar.onClick.AddListener(Keluar);
        }

        panelPause.SetActive(false);
    }

    void Update()
    {
        // [Esc] atau [P] untuk pause / lanjut
        if (Keyboard.current != null &&
            (Keyboard.current.escapeKey.wasPressedThisFrame ||
             Keyboard.current.pKey.wasPressedThisFrame))
        {
            if (IsPaused) Lanjutkan();
            else Pause();
        }
    }

    public void Pause()
    {
        if (IsPaused) return;
        IsPaused = true;

        Time.timeScale = 0f;
        if (pauseAudio) AudioListener.pause = true;

        kursorSebelumnyaTampil = Cursor.visible;
        kursorLockSebelumnya = Cursor.lockState;
        if (tampilkanKursorSaatPause)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        if (panelPause != null) panelPause.SetActive(true);
    }

    public void Lanjutkan()
    {
        if (!IsPaused) return;
        IsPaused = false;

        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (tampilkanKursorSaatPause)
        {
            Cursor.visible = kursorSebelumnyaTampil;
            Cursor.lockState = kursorLockSebelumnya;
        }

        if (panelPause != null) panelPause.SetActive(false);
    }

    public void Restart()
    {
        // Kembalikan waktu & suara ke normal sebelum memuat ulang scene
        IsPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Keluar()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnDestroy()
    {
        // Pastikan waktu tidak tertinggal beku jika objek ini hilang
        if (Instance == this)
        {
            IsPaused = false;
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
    }

    // --- UI OTOMATIS ---

    void BuatUIOtomatis()
    {
        // Pastikan ada EventSystem agar tombol bisa diklik
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        TMP_FontAsset fontAsset = TMP_Settings.defaultFontAsset;

        // Canvas khusus menu pause
        GameObject canvasObj = new GameObject("PauseCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObj.transform.SetParent(transform, false);

        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // Panel gelap satu layar penuh
        panelPause = new GameObject("PanelPause", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelPause.transform.SetParent(canvasObj.transform, false);
        RectTransform panelRect = panelPause.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        panelPause.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);

        // Kotak menu di tengah
        GameObject box = new GameObject("Box", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        box.transform.SetParent(panelPause.transform, false);
        RectTransform boxRect = box.GetComponent<RectTransform>();
        boxRect.anchorMin = new Vector2(0.5f, 0.5f);
        boxRect.anchorMax = new Vector2(0.5f, 0.5f);
        boxRect.pivot = new Vector2(0.5f, 0.5f);
        boxRect.sizeDelta = new Vector2(480f, 480f);
        box.GetComponent<Image>().color = new Color(0.10f, 0.13f, 0.18f, 0.98f);

        // Judul
        GameObject judulObj = new GameObject("Judul", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        judulObj.transform.SetParent(box.transform, false);
        RectTransform judulRect = judulObj.GetComponent<RectTransform>();
        judulRect.anchoredPosition = new Vector2(0f, 170f);
        judulRect.sizeDelta = new Vector2(420f, 80f);
        TextMeshProUGUI judul = judulObj.GetComponent<TextMeshProUGUI>();
        if (fontAsset != null) judul.font = fontAsset;
        judul.text = "PAUSE";
        judul.fontSize = 56;
        judul.fontStyle = FontStyles.Bold;
        judul.color = Color.white;
        judul.alignment = TextAlignmentOptions.Center;

        // Tombol-tombol
        btnLanjut = BuatTombol(box.transform, "BtnLanjut", "LANJUTKAN", new Vector2(0f, 60f),
            new Color(0.15f, 0.65f, 0.35f, 1f), fontAsset);
        btnRestart = BuatTombol(box.transform, "BtnRestart", "ULANGI", new Vector2(0f, -40f),
            new Color(0.20f, 0.45f, 0.80f, 1f), fontAsset);
        btnKeluar = BuatTombol(box.transform, "BtnKeluar", "KELUAR", new Vector2(0f, -140f),
            new Color(0.85f, 0.25f, 0.25f, 1f), fontAsset);

        btnLanjut.onClick.AddListener(Lanjutkan);
        btnRestart.onClick.AddListener(Restart);
        btnKeluar.onClick.AddListener(Keluar);

        // Petunjuk tombol
        GameObject hintObj = new GameObject("Hint", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        hintObj.transform.SetParent(box.transform, false);
        RectTransform hintRect = hintObj.GetComponent<RectTransform>();
        hintRect.anchoredPosition = new Vector2(0f, -210f);
        hintRect.sizeDelta = new Vector2(420f, 30f);
        TextMeshProUGUI hint = hintObj.GetComponent<TextMeshProUGUI>();
        if (fontAsset != null) hint.font = fontAsset;
        hint.text = "Tekan [Esc] atau [P] untuk lanjut";
        hint.fontSize = 20;
        hint.color = new Color(0.65f, 0.78f, 0.90f);
        hint.alignment = TextAlignmentOptions.Center;
    }

    Button BuatTombol(Transform parent, string nama, string label, Vector2 posisi, Color warna, TMP_FontAsset fontAsset)
    {
        GameObject btnObj = new GameObject(nama, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchoredPosition = posisi;
        rt.sizeDelta = new Vector2(320f, 70f);

        btnObj.GetComponent<Image>().color = warna;

        GameObject txtObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        txtObj.transform.SetParent(btnObj.transform, false);
        RectTransform txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = txtObj.GetComponent<TextMeshProUGUI>();
        if (fontAsset != null) tmp.font = fontAsset;
        tmp.text = label;
        tmp.fontSize = 32;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        return btnObj.GetComponent<Button>();
    }
}