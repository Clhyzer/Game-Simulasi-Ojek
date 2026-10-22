using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Mengatur musik latar (BGM) dan mute/unmute dengan dua tombol terpisah.
/// Saat suara ON hanya tombol MUTE yang aktif, saat MUTE hanya tombol UNMUTE yang aktif.
/// Mute memakai AudioListener.volume, jadi semua suara (termasuk SFX) ikut dibisukan.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Musik Latar (BGM)")]
    public AudioClip[] bgmClips;
    [Range(0f, 1f)] public float bgmVolume = 0.5f;
    public bool acakLagu = false;
    public bool mulaiOtomatis = true;

    [Header("Suara Klik (Opsional)")]
    public AudioClip sfxKlik;
    [Range(0f, 1f)] public float sfxKlikVolume = 1f;

    [Header("UI (Opsional - tombol dibuat otomatis jika kosong)")]
    [Tooltip("Tombol yang tampil saat suara ON")]
    public Button btnMute;
    [Tooltip("Tombol yang tampil saat suara MUTE")]
    public Button btnUnmute;

    [Header("Pengaturan")]
    public Key tombolMute = Key.M;

    public bool IsMuted { get; private set; }

    private AudioSource bgmSource;
    private AudioSource sfxSource;
    private int indexLagu = -1;

    private const string KeyMute = "Audio_Muted";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = false;
        bgmSource.spatialBlend = 0f;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;

        IsMuted = PlayerPrefs.GetInt(KeyMute, 0) == 1;
    }

    void Start()
    {
        SiapkanUI();
        TerapkanAudio();

        if (mulaiOtomatis) MainkanLaguBerikutnya();
    }

    void Update()
    {
        // Shortcut keyboard tetap bisa dipakai
        if (Keyboard.current != null && Keyboard.current[tombolMute].wasPressedThisFrame)
        {
            SetMuted(!IsMuted);
        }

        if (mulaiOtomatis && bgmClips != null && bgmClips.Length > 0
            && !bgmSource.isPlaying && Application.isFocused)
        {
            MainkanLaguBerikutnya();
        }
    }

    // --- MUTE ---

    public void Mute() { SetMuted(true); }
    public void Unmute() { SetMuted(false); }

    public void SetMuted(bool mute)
    {
        IsMuted = mute;
        PlayerPrefs.SetInt(KeyMute, IsMuted ? 1 : 0);
        PlayerPrefs.Save();
        TerapkanAudio();
    }

    void TerapkanAudio()
    {
        AudioListener.volume = IsMuted ? 0f : 1f;
        if (bgmSource != null) bgmSource.volume = bgmVolume;

        // Tombol yang aktif bergantian
        if (btnMute != null) btnMute.gameObject.SetActive(!IsMuted);
        if (btnUnmute != null) btnUnmute.gameObject.SetActive(IsMuted);
    }

    // --- BGM ---

    public void MainkanLaguBerikutnya()
    {
        if (bgmClips == null || bgmClips.Length == 0) return;

        if (acakLagu && bgmClips.Length > 1)
        {
            int baru;
            do { baru = Random.Range(0, bgmClips.Length); } while (baru == indexLagu);
            indexLagu = baru;
        }
        else
        {
            indexLagu = (indexLagu + 1) % bgmClips.Length;
        }

        bgmSource.clip = bgmClips[indexLagu];
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }

    public void PauseBGM() { if (bgmSource != null) bgmSource.Pause(); }
    public void LanjutBGM() { if (bgmSource != null) bgmSource.UnPause(); }

    // --- SFX ---

    public void MainkanKlik()
    {
        if (sfxKlik != null) sfxSource.PlayOneShot(sfxKlik, sfxKlikVolume);
    }

    // --- UI ---

    void SiapkanUI()
    {
        if (btnMute == null || btnUnmute == null)
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                if (btnMute == null) btnMute = CariAtauBuatTombol(canvas, "BtnMute", "SUARA: ON");
                if (btnUnmute == null) btnUnmute = CariAtauBuatTombol(canvas, "BtnUnmute", "SUARA: MUTE");
            }
        }

        if (btnMute != null)
        {
            btnMute.onClick.RemoveListener(Mute);
            btnMute.onClick.AddListener(Mute);
        }

        if (btnUnmute != null)
        {
            btnUnmute.onClick.RemoveListener(Unmute);
            btnUnmute.onClick.AddListener(Unmute);
        }
    }

    Button CariAtauBuatTombol(Canvas canvas, string nama, string label)
    {
        // Transform.Find tetap menemukan objek yang sedang non-aktif
        Transform existing = canvas.transform.Find(nama);
        if (existing != null) return existing.GetComponent<Button>();

        GameObject btnObj = new GameObject(nama, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(canvas.transform, false);

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-25f, -25f);
        rt.sizeDelta = new Vector2(170f, 40f);
        btnObj.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.14f, 0.85f);

        GameObject txtObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        txtObj.transform.SetParent(btnObj.transform, false);
        RectTransform tr = txtObj.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = tr.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = txtObj.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 16;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        return btnObj.GetComponent<Button>();
    }
}