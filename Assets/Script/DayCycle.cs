using System;
using UnityEngine;
using TMPro;

public class DayCycle : MonoBehaviour
{
    public static DayCycle Instance { get; private set; }

    // Event yang bisa didengar script lain
    public static event Action<int> OnLaporanHarian; // jam laporan tercapai (parameter: nomor hari)
    public static event Action<int> OnHariBaru;      // hari baru dimulai (parameter: nomor hari)

    [Header("Referensi")]
    [SerializeField] private Light sunLight;
    [SerializeField] private TMP_Text clockText;
    [Tooltip("Opsional: teks untuk menampilkan 'Hari 1', 'Hari 2', dst.")]
    [SerializeField] private TMP_Text dayText;

    [Header("Pengaturan Waktu")]
    [Tooltip("Durasi 1 hari in-game dalam detik real-time")]
    [SerializeField] private float dayDurationInSeconds = 120f;
    [Range(0f, 24f)]
    [SerializeField] private float startHour = 6f;

    [Header("Laporan Harian")]
    [Range(0f, 24f)]
    [SerializeField] private float jamLaporan = 23f;

    [Header("Tampilan")]
    [SerializeField] private bool use24HourFormat = true;

    // BARU: pengaturan langit dan cahaya
    [Header("Langit & Cahaya")]
    [SerializeField] private float exposureSiang = 1.2f;
    [SerializeField] private float exposureMalam = 0.05f;
    [SerializeField] private Color tintSiang = Color.white;
    [SerializeField] private Color tintMalam = new Color(0.3f, 0.35f, 0.6f);
    [SerializeField] private float intensitasMatahariSiang = 1f;
    [SerializeField] private float intensitasMatahariMalam = 0f;
    [SerializeField] private float ambientSiang = 1f;
    [SerializeField] private float ambientMalam = 0.1f;
    [SerializeField] private bool ubahWarnaFog = true;
    [SerializeField] private Color fogSiang = new Color(0.7f, 0.8f, 0.9f);
    [SerializeField] private Color fogMalam = new Color(0.03f, 0.04f, 0.08f);

    public float CurrentHour { get; private set; }
    public int Day { get; private set; } = 1;

    private bool laporanSudahMuncul = false;

    // BARU
    private Material skyInstance;
    private float timerGI;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // BARU: pakai salinan material supaya file skybox asli tidak ikut berubah
        if (RenderSettings.skybox != null)
        {
            skyInstance = new Material(RenderSettings.skybox);
            RenderSettings.skybox = skyInstance;
        }

        CurrentHour = startHour;
        laporanSudahMuncul = CurrentHour >= jamLaporan;
        UpdateSun();
        UpdateClock();
        UpdateDayText();
    }

    private void Update()
    {
        float hoursPerSecond = 24f / dayDurationInSeconds;
        CurrentHour += hoursPerSecond * Time.deltaTime;

        // Cek jam laporan (sekali per hari)
        if (!laporanSudahMuncul && CurrentHour >= jamLaporan)
        {
            laporanSudahMuncul = true;
            OnLaporanHarian?.Invoke(Day);
        }

        // Pengaman: kalau laporan tidak ditutup sampai lewat tengah malam
        if (CurrentHour >= 24f)
        {
            MulaiHariBaru();
            return;
        }

        UpdateSun();
        UpdateClock();
    }

    /// <summary>
    /// Mulai hari berikutnya: nomor hari +1, jam kembali ke jam mulai.
    /// Dipanggil saat laporan harian ditutup.
    /// </summary>
    public void MulaiHariBaru()
    {
        Day++;
        CurrentHour = startHour;
        laporanSudahMuncul = CurrentHour >= jamLaporan;

        UpdateSun();
        UpdateClock();
        UpdateDayText();

        OnHariBaru?.Invoke(Day);
    }

    private void UpdateSun()
    {
        if (sunLight == null) return;
        float angle = (CurrentHour / 24f) * 360f - 90f;
        sunLight.transform.rotation = Quaternion.Euler(angle, 170f, 0f);

        UpdateLangit(); // BARU
    }

    // BARU
    private void UpdateLangit()
    {
        // 1 = matahari tepat di atas, -1 = tepat di bawah horizon
        float tinggi = Vector3.Dot(-sunLight.transform.forward, Vector3.up);
        float t = Mathf.Clamp01(Mathf.InverseLerp(-0.1f, 0.3f, tinggi));

        if (skyInstance != null)
        {
            if (skyInstance.HasProperty("_Exposure"))
                skyInstance.SetFloat("_Exposure", Mathf.Lerp(exposureMalam, exposureSiang, t));
            if (skyInstance.HasProperty("_Tint"))
                skyInstance.SetColor("_Tint", Color.Lerp(tintMalam, tintSiang, t));
        }

        sunLight.intensity = Mathf.Lerp(intensitasMatahariMalam, intensitasMatahariSiang, t);
        RenderSettings.ambientIntensity = Mathf.Lerp(ambientMalam, ambientSiang, t);

        if (ubahWarnaFog)
            RenderSettings.fogColor = Color.Lerp(fogMalam, fogSiang, t);

        // Refresh pencahayaan dari skybox (cukup 2x per detik, tidak perlu tiap frame)
        timerGI += Time.deltaTime;
        if (timerGI >= 0.5f)
        {
            timerGI = 0f;
            DynamicGI.UpdateEnvironment();
        }
    }

    private void UpdateDayText()
    {
        if (dayText != null) dayText.text = $"Hari {Day}";
    }

    private void UpdateClock()
    {
        if (clockText == null) return;

        int hour = Mathf.FloorToInt(CurrentHour);
        int minute = Mathf.FloorToInt((CurrentHour - hour) * 60f);

        if (use24HourFormat)
        {
            clockText.text = $"{hour:00}:{minute:00}";
        }
        else
        {
            string period = hour >= 12 ? "PM" : "AM";
            int displayHour = hour % 12;
            if (displayHour == 0) displayHour = 12;
            clockText.text = $"{displayHour:00}:{minute:00} {period}";
        }
    }
}