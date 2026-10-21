using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class PhoneOjekApp : MonoBehaviour
{
    public static PhoneOjekApp Instance { get; private set; }

    [Header("UI Toggle & Text")]
    public Toggle toggleOnOff;
    public TMP_Text textStatus;

    [Header("Panels")]
    public GameObject panelPopupOrder;
    public GameObject panelDalamOrderan;

    [Header("Buttons")]
    public Button btnTerimaOrder;
    public Button btnSelesaiOrder;

    [Header("Sistem Keuangan (2 Point)")]
    [Tooltip("Point 1: Saldo yang masuk ke rekening di HP saat pesanan selesai.")]
    public int saldoAplikasi = 0;

    [Tooltip("Point 2: Uang utama / kas fisik player setelah dicairkan.")]
    public int uangUtamaPlayer = 0;

    [Header("UI Keuangan (Opsional - Diisi otomatis jika kosong)")]
    public TMP_Text textSaldoAplikasi;
    public TMP_Text textUangUtama;
    public TMP_Text textUangUtamaHUD;
    public Button btnCairkanSaldo;
    public TMP_Text textPopupInfo;
    public TMP_Text textDalamOrderanInfo;

    [Header("Pengaturan Orderan")]
    public float waktuTungguMin = 3f;  // Detik minimal dapet orderan
    public float waktuTungguMax = 6f;  // Detik maksimal dapet orderan

    [Header("Pengaturan Tarif & Jarak")]
    [Tooltip("Tarif dasar minimum setiap orderan (Rupiah).")]
    public int tarifDasar = 8000;

    [Tooltip("Tarif tambahan per meter jarak pengantaran (Rupiah/meter).")]
    public float tarifPerMeter = 100f;

    [Tooltip("Kelipatan pembulatan tarif (misal 500 atau 1000).")]
    public int pembulatanTarif = 500;

    [Header("Pengaturan Batas Waktu Pengantaran")]
    [Tooltip("Waktu dasar minimum untuk mengantar (detik).")]
    public float waktuDasar = 15f;

    [Tooltip("Estimasi kecepatan motor untuk menghitung batas waktu (meter/detik).")]
    public float kecepatanEstimasi = 8f;

    [Header("Pengaturan Denda Keterlambatan")]
    [Tooltip("Persentase denda per detik keterlambatan (0.02 = 2% per detik).")]
    public float dendaPersenPerDetik = 0.02f;

    [Tooltip("Maksimal potongan denda dari tarif total (0.60 = maksimal dipotong 60%).")]
    public float dendaMaksimalPersen = 0.60f;

    [Header("Pengaturan Penalti Bukan di Drop Zone")]
    [Tooltip("Penalti tetap yang dikurangi dari saldo jika menyelesaikan orderan bukan di titik drop (Rupiah).")]
    public int penaltiTidakDiDrop = 5000;

    [Tooltip("Persentase tambahan dari tarif orderan yang juga dipotong (0.50 = 50% dari tarif awal).")]
    public float penaltiPersenTarif = 0.50f;

    // Status order berjalan
    private bool isSearching = false;
    private bool isBusy = false;
    private bool isDelivering = false;

    private float jarakOrder = 0f;
    private int tarifAwal = 0;
    private int tarifAkhir = 0;
    private int denda = 0;
    private float batasWaktuOrder = 0f;
    private float sisaWaktu = 0f;

    void Awake()
    {
        Instance = this;
        MuatDataKeuangan();
    }

    void Start()
    {
        // Hubungkan event klik ke fungsi lewat kode
        toggleOnOff.onValueChanged.AddListener(OnToggleChanged);
        btnTerimaOrder.onClick.AddListener(TerimaOrder);
        btnSelesaiOrder.onClick.AddListener(SelesaikanOrder);

        // Inisialisasi UI Keuangan & Binding Teks
        InisialisasiUIKeuanganOtomatis();

        // Sembunyikan panel-panel di awal & perbarui tampilan saldo
        ResetUI();
        UpdateTampilanSaldo();
    }

    void Update()
    {
        // Shortcut keyboard [C] untuk mencairkan saldo kapan saja
        if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
        {
            CairkanSaldo();
        }

        // Hitung mundur waktu saat sedang mengantar penumpang
        if (isBusy && isDelivering)
        {
            sisaWaktu -= Time.deltaTime;

            if (sisaWaktu >= 0f)
            {
                denda = 0;
                tarifAkhir = tarifAwal;
            }
            else
            {
                // Terlambat! Hitung pemotongan denda sesuai keterlambatan
                float waktuTerlambat = Mathf.Abs(sisaWaktu);
                float persenDenda = Mathf.Min(waktuTerlambat * dendaPersenPerDetik, dendaMaksimalPersen);
                denda = Mathf.RoundToInt(tarifAwal * persenDenda);
                tarifAkhir = Mathf.Max(0, tarifAwal - denda);
            }

            UpdateTampilanDalamOrderanTimer();
        }
    }

    // 1. Dijalankan saat Toggle ON / OFF diubah
    void OnToggleChanged(bool isOn)
    {
        // Jika sedang bawa penumpang, toggle dipaksa tetap ON
        if (isBusy)
        {
            toggleOnOff.isOn = true;
            Debug.Log("Selesaikan orderan dulu sebelum matikan aplikasi!");
            return;
        }

        if (isOn)
        {
            isSearching = true;
            textStatus.text = "Status: ON (Mencari Orderan...)";
            MulaiCariOrderan();
        }
        else
        {
            isSearching = false;
            textStatus.text = "Status: OFF";
            CancelInvoke(nameof(MutilasiOrderanMasuk));
            CancelInvoke(nameof(LanjutCariOrderan));
            panelPopupOrder.SetActive(false);
        }
    }

    void MulaiCariOrderan()
    {
        float jedaWaktu = Random.Range(waktuTungguMin, waktuTungguMax);
        Invoke(nameof(MutilasiOrderanMasuk), jedaWaktu);
    }

    // 2. Dipanggil otomatis setelah waktu penantian selesai
    void MutilasiOrderanMasuk()
    {
        if (isSearching && !isBusy)
        {
            // 1. Siapkan titik jemput & antar dan hitung jarak dari spawner
            float jarak = 50f;
            if (NPCSpawner.Instance != null && NPCSpawner.Instance.SiapkanOrderanBaru(out float calculatedDistance))
            {
                jarak = calculatedDistance;
            }

            // 2. Hitung tarif dan batas waktu sesuai jarak
            jarakOrder = jarak;
            tarifAwal = HitungTarif(jarak);
            tarifAkhir = tarifAwal;
            denda = 0;
            batasWaktuOrder = HitungBatasWaktu(jarak);
            sisaWaktu = batasWaktuOrder;

            textStatus.text = "ADA ORDERAN MASUK!";

            // 3. Perbarui teks di pop-up orderan
            UpdateTampilanPopupOrder();

            panelPopupOrder.SetActive(true); // Munculkan Pop-up
        }
    }

    // 3. Dipanggil saat tombol "Terima Order" diklik
    void TerimaOrder()
    {
        isBusy = true;
        isSearching = false;
        isDelivering = false; // Timer pengantaran baru jalan setelah NPC naik ke motor

        panelPopupOrder.SetActive(false);
        panelDalamOrderan.SetActive(true);
        textStatus.text = "Status: Menuju Lokasi Jemput";

        // Perbarui teks di panel sedang dalam orderan
        UpdateTampilanDalamOrderanMenujuJemput();

        // 1. SPAWN NPC DI DUNIA GAME
        if (NPCSpawner.Instance != null)
        {
            NPCSpawner.Instance.SpawnNPC();
        }
    }

    /// <summary>
    /// Dipanggil otomatis dari NPCSpawner saat penumpang sudah naik ke motor.
    /// </summary>
    public void MulaiPengantaran()
    {
        isDelivering = true;
        sisaWaktu = batasWaktuOrder;
        textStatus.text = "Status: Mengantar Penumpang";
        Debug.Log($"[OJOL] Penumpang sudah naik! Batas waktu: {batasWaktuOrder:F0}s, Tarif: Rp {tarifAwal:N0}");
    }

    // 4. Dipanggil saat tombol "Selesaikan Order" diklik
    void SelesaikanOrder()
    {
        isBusy = false;
        isDelivering = false;
        panelDalamOrderan.SetActive(false);

        // CEK APAKAH PLAYER BERADA DI TITIK DROP ZONE
        if (DropZoneTrigger.isPlayerInDropZone)
        {
            // === BERHASIL: Player di titik drop zone ===
            int uangDiterima = tarifAkhir;
            saldoAplikasi += uangDiterima;
            SimpanDataKeuangan();
            UpdateTampilanSaldo();

            if (denda > 0)
            {
                textStatus.text = $"Selesai! +Rp {uangDiterima:N0} (Telat -Rp {denda:N0})";
                Debug.Log($"[OJOL] Orderan Selesai Terlambat! Tarif Awal: Rp {tarifAwal:N0}, Denda: Rp {denda:N0}, Masuk Rekening: Rp {uangDiterima:N0}");
            }
            else
            {
                textStatus.text = $"Selesai Tepat Waktu! +Rp {uangDiterima:N0}";
                Debug.Log($"[OJOL] Orderan Selesai Tepat Waktu! Masuk Rekening: Rp {uangDiterima:N0}");
            }
        }
        else
        {
            // === GAGAL: Player BUKAN di titik drop zone ===
            int totalPenalti = penaltiTidakDiDrop + Mathf.RoundToInt(tarifAwal * penaltiPersenTarif);
            saldoAplikasi -= totalPenalti;

            // Jika saldo aplikasi minus, potong dari uang utama
            if (saldoAplikasi < 0)
            {
                uangUtamaPlayer += saldoAplikasi; // saldoAplikasi negatif, jadi ini mengurangi
                if (uangUtamaPlayer < 0) uangUtamaPlayer = 0;
                saldoAplikasi = 0;
            }

            SimpanDataKeuangan();
            UpdateTampilanSaldo();

            textStatus.text = $"ORDERAN DIBATALKAN! -Rp {totalPenalti:N0}";
            Debug.Log($"[OJOL] Orderan diselesaikan BUKAN di Drop Zone! Penalti: Rp {totalPenalti:N0} (Tetap: Rp {penaltiTidakDiDrop:N0} + {penaltiPersenTarif * 100f:F0}% tarif)");
        }

        // Bersihkan NPC, DropZone, dan Garis Navigasi
        if (NPCSpawner.Instance != null)
        {
            NPCSpawner.Instance.HapusSemuaJob();
        }

        // Lanjut mencari orderan setelah jeda singkat
        if (toggleOnOff.isOn)
        {
            Invoke(nameof(LanjutCariOrderan), 3f);
        }
        else
        {
            // Jangan timpa pesan penalti/selesai, biarkan 3 detik lalu ganti
            Invoke(nameof(SetStatusOFF), 3f);
        }
    }

    void SetStatusOFF()
    {
        if (!isBusy && !toggleOnOff.isOn)
        {
            textStatus.text = "Status: OFF";
        }
    }

    void LanjutCariOrderan()
    {
        if (toggleOnOff.isOn && !isBusy)
        {
            isSearching = true;
            textStatus.text = "Status: ON (Mencari Orderan...)";
            MulaiCariOrderan();
        }
    }

    /// <summary>
    /// Mencairkan saldo dari Rekening HP (Point 1) ke Uang Utama Player (Point 2).
    /// </summary>
    public void CairkanSaldo()
    {
        if (saldoAplikasi <= 0)
        {
            Debug.Log("[BANK] Saldo aplikasi kosong, tidak ada yang bisa dicairkan.");
            if (textStatus != null && !isBusy)
            {
                textStatus.text = "Saldo HP Masih Rp 0!";
            }
            return;
        }

        int jumlah = saldoAplikasi;
        uangUtamaPlayer += jumlah;
        saldoAplikasi = 0;

        SimpanDataKeuangan();
        UpdateTampilanSaldo();

        Debug.Log($"[BANK] Berhasil mencairkan dana Rp {jumlah:N0} ke Uang Utama Player! Total Kas: Rp {uangUtamaPlayer:N0}");
        if (textStatus != null && !isBusy)
        {
            textStatus.text = $"Cairkan Rp {jumlah:N0} Sukses!";
        }
    }

    // --- FORMULA KALKULASI ---

    int HitungTarif(float jarak)
    {
        int raw = Mathf.RoundToInt(tarifDasar + (jarak * tarifPerMeter));
        if (pembulatanTarif > 1)
        {
            raw = Mathf.RoundToInt((float)raw / pembulatanTarif) * pembulatanTarif;
        }
        return Mathf.Max(tarifDasar, raw);
    }

    float HitungBatasWaktu(float jarak)
    {
        float durasi = waktuDasar + (jarak / Mathf.Max(1f, kecepatanEstimasi));
        return Mathf.Max(10f, Mathf.Round(durasi));
    }

    // --- UPDATE TAMPILAN UI ---

    void UpdateTampilanPopupOrder()
    {
        if (textPopupInfo == null && panelPopupOrder != null)
        {
            textPopupInfo = panelPopupOrder.GetComponentInChildren<TMP_Text>();
        }

        if (textPopupInfo != null)
        {
            textPopupInfo.text = $"<b>ORDERAN MASUK!</b>\n" +
                                 $"Jarak  : {jarakOrder:F0} m\n" +
                                 $"Waktu  : {batasWaktuOrder:F0} dtk\n" +
                                 $"Tarif  : Rp {tarifAwal:N0}";
        }
    }

    void UpdateTampilanDalamOrderanMenujuJemput()
    {
        if (textDalamOrderanInfo == null && panelDalamOrderan != null)
        {
            textDalamOrderanInfo = panelDalamOrderan.GetComponentInChildren<TMP_Text>();
        }

        if (textDalamOrderanInfo != null)
        {
            textDalamOrderanInfo.text = $"<b>PENJEMPUTAN</b>\n" +
                                        $"Menuju titik jemput...\n" +
                                        $"Jarak Antar: {jarakOrder:F0} m\n" +
                                        $"Tarif: Rp {tarifAwal:N0}";
        }
    }

    void UpdateTampilanDalamOrderanTimer()
    {
        if (textDalamOrderanInfo == null && panelDalamOrderan != null)
        {
            textDalamOrderanInfo = panelDalamOrderan.GetComponentInChildren<TMP_Text>();
        }

        if (textDalamOrderanInfo != null)
        {
            if (sisaWaktu >= 0f)
            {
                int menit = Mathf.FloorToInt(sisaWaktu / 60f);
                int detik = Mathf.FloorToInt(sisaWaktu % 60f);

                textDalamOrderanInfo.text = $"<b>MENGANTAR PENUMPANG</b>\n" +
                                            $"Sisa Waktu : <color=green>{menit:D2}:{detik:D2}</color>\n" +
                                            $"Tarif      : Rp {tarifAwal:N0}";
            }
            else
            {
                float waktuTelat = Mathf.Abs(sisaWaktu);
                int menit = Mathf.FloorToInt(waktuTelat / 60f);
                int detik = Mathf.FloorToInt(waktuTelat % 60f);
                float persenDenda = Mathf.Min(waktuTelat * dendaPersenPerDetik, dendaMaksimalPersen) * 100f;

                textDalamOrderanInfo.text = $"<b><color=red>TERLAMBAT!</color></b>\n" +
                                            $"Waktu : <color=red>-{menit:D2}:{detik:D2}</color> (-{persenDenda:F0}%)\n" +
                                            $"Tarif : <s>Rp {tarifAwal:N0}</s> <b><color=yellow>Rp {tarifAkhir:N0}</color></b>";
            }
        }
    }

    public void UpdateTampilanSaldo()
    {
        if (textSaldoAplikasi != null)
        {
            textSaldoAplikasi.text = $"Rp {saldoAplikasi:N0}";
        }

        if (textUangUtama != null)
        {
            textUangUtama.text = $"Dompet: <b>Rp {uangUtamaPlayer:N0}</b>";
        }

        if (textUangUtamaHUD != null)
        {
            textUangUtamaHUD.text = $"💵 Uang: Rp {uangUtamaPlayer:N0}";
        }
    }

    void InisialisasiUIKeuanganOtomatis()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = Object.FindFirstObjectByType<Canvas>();

        TMP_FontAsset fontAsset = textStatus != null ? textStatus.font : TMP_Settings.defaultFontAsset;

        // 1. Sesuaikan ukuran teks dan warna pada panel popup & dalam orderan
        if (textPopupInfo == null && panelPopupOrder != null)
        {
            textPopupInfo = panelPopupOrder.GetComponentInChildren<TMP_Text>();
        }
        if (textPopupInfo != null)
        {
            textPopupInfo.fontSize = 11;
            textPopupInfo.enableWordWrapping = true;
            textPopupInfo.alignment = TextAlignmentOptions.Center;
            RectTransform rt = textPopupInfo.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = new Vector2(0f, 35f);
                rt.sizeDelta = new Vector2(180f, 85f);
            }
        }

        if (panelPopupOrder != null)
        {
            Image bg = panelPopupOrder.GetComponent<Image>();
            if (bg != null) bg.color = new Color(0.10f, 0.13f, 0.18f, 0.98f);
        }

        if (btnTerimaOrder != null)
        {
            Image bImg = btnTerimaOrder.GetComponent<Image>();
            if (bImg != null) bImg.color = new Color(0.15f, 0.65f, 0.35f, 1f);
            RectTransform bRt = btnTerimaOrder.GetComponent<RectTransform>();
            if (bRt != null)
            {
                bRt.anchoredPosition = new Vector2(0f, -50f);
                bRt.sizeDelta = new Vector2(140f, 30f);
            }
            TMP_Text bTxt = btnTerimaOrder.GetComponentInChildren<TMP_Text>();
            if (bTxt != null)
            {
                bTxt.text = "TERIMA ORDER";
                bTxt.fontSize = 11;
                bTxt.fontStyle = FontStyles.Bold;
            }
        }

        if (textDalamOrderanInfo == null && panelDalamOrderan != null)
        {
            textDalamOrderanInfo = panelDalamOrderan.GetComponentInChildren<TMP_Text>();
        }
        if (textDalamOrderanInfo != null)
        {
            textDalamOrderanInfo.fontSize = 11;
            textDalamOrderanInfo.enableWordWrapping = true;
            textDalamOrderanInfo.alignment = TextAlignmentOptions.Center;
            RectTransform rt = textDalamOrderanInfo.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = new Vector2(0f, 35f);
                rt.sizeDelta = new Vector2(180f, 85f);
            }
        }

        if (panelDalamOrderan != null)
        {
            Image bg = panelDalamOrderan.GetComponent<Image>();
            if (bg != null) bg.color = new Color(0.10f, 0.13f, 0.18f, 0.98f);
        }

        if (btnSelesaiOrder != null)
        {
            Image bImg = btnSelesaiOrder.GetComponent<Image>();
            if (bImg != null) bImg.color = new Color(0.85f, 0.25f, 0.25f, 1f);
            RectTransform bRt = btnSelesaiOrder.GetComponent<RectTransform>();
            if (bRt != null)
            {
                bRt.anchoredPosition = new Vector2(0f, -50f);
                bRt.sizeDelta = new Vector2(150f, 30f);
            }
            TMP_Text bTxt = btnSelesaiOrder.GetComponentInChildren<TMP_Text>();
            if (bTxt != null)
            {
                bTxt.text = "SELESAIKAN ORDER";
                bTxt.fontSize = 10;
                bTxt.fontStyle = FontStyles.Bold;
            }
        }

        // 2. Hubungkan / Buat HUD Uang Utama di pojok layar game jika ada canvas
        if (textUangUtamaHUD == null && canvas != null)
        {
            Transform existingHud = canvas.transform.Find("HUD_UangPlayer");
            if (existingHud != null)
            {
                textUangUtamaHUD = existingHud.GetComponentInChildren<TMP_Text>();
            }
            else
            {
                GameObject hudObj = new GameObject("HUD_UangPlayer", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                hudObj.transform.SetParent(canvas.transform, false);

                RectTransform hudRect = hudObj.GetComponent<RectTransform>();
                hudRect.anchorMin = new Vector2(0f, 1f);
                hudRect.anchorMax = new Vector2(0f, 1f);
                hudRect.pivot = new Vector2(0f, 1f);
                hudRect.anchoredPosition = new Vector2(25f, -25f);
                hudRect.sizeDelta = new Vector2(230f, 50f);

                Image hudImg = hudObj.GetComponent<Image>();
                hudImg.color = new Color(0.08f, 0.10f, 0.14f, 0.85f);

                GameObject txtObj = new GameObject("TextUangHUD", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                txtObj.transform.SetParent(hudObj.transform, false);

                RectTransform txtRect = txtObj.GetComponent<RectTransform>();
                txtRect.anchorMin = Vector2.zero;
                txtRect.anchorMax = Vector2.one;
                txtRect.offsetMin = new Vector2(15f, 5f);
                txtRect.offsetMax = new Vector2(-15f, -5f);

                TextMeshProUGUI tmp = txtObj.GetComponent<TextMeshProUGUI>();
                if (fontAsset != null) tmp.font = fontAsset;
                tmp.text = "💵 Uang: Rp 0";
                tmp.fontSize = 17;
                tmp.fontStyle = FontStyles.Bold;
                tmp.color = new Color(1f, 0.85f, 0.25f);
                tmp.alignment = TextAlignmentOptions.MidlineLeft;

                textUangUtamaHUD = tmp;
            }
        }

        // 3. Cari / Buat widget Panel Saldo di dalam HP
        Transform phoneParent = toggleOnOff != null ? toggleOnOff.transform.parent : transform;
        if (phoneParent != null)
        {
            Transform existingPanel = phoneParent.Find("PanelSaldo");
            if (existingPanel != null)
            {
                if (textSaldoAplikasi == null)
                {
                    var txt = existingPanel.Find("TextSaldoAplikasi");
                    if (txt != null) textSaldoAplikasi = txt.GetComponent<TMP_Text>();
                }
                if (textUangUtama == null)
                {
                    var txt = existingPanel.Find("TextUangUtama");
                    if (txt != null) textUangUtama = txt.GetComponent<TMP_Text>();
                }
                if (btnCairkanSaldo == null)
                {
                    var btn = existingPanel.Find("BtnCairkan");
                    if (btn != null) btnCairkanSaldo = btn.GetComponent<Button>();
                }
            }
            else if (textSaldoAplikasi == null || textUangUtama == null)
            {
                // Buat container Panel Saldo
                GameObject panelSaldoObj = new GameObject("PanelSaldo", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                panelSaldoObj.transform.SetParent(phoneParent, false);

                RectTransform panelRect = panelSaldoObj.GetComponent<RectTransform>();
                panelRect.anchorMin = new Vector2(0.5f, 0.5f);
                panelRect.anchorMax = new Vector2(0.5f, 0.5f);
                panelRect.pivot = new Vector2(0.5f, 0.5f);
                panelRect.anchoredPosition = new Vector2(0f, 15f);
                panelRect.sizeDelta = new Vector2(180f, 90f);

                Image panelImg = panelSaldoObj.GetComponent<Image>();
                panelImg.color = new Color(0.12f, 0.15f, 0.20f, 0.95f);

                // Label Judul
                GameObject judulObj = new GameObject("LabelJudul", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                judulObj.transform.SetParent(panelSaldoObj.transform, false);
                RectTransform rtJudul = judulObj.GetComponent<RectTransform>();
                rtJudul.anchoredPosition = new Vector2(0f, 32f);
                rtJudul.sizeDelta = new Vector2(170f, 16f);
                TextMeshProUGUI tmpJudul = judulObj.GetComponent<TextMeshProUGUI>();
                if (fontAsset != null) tmpJudul.font = fontAsset;
                tmpJudul.text = "SALDO REKENING OJOL";
                tmpJudul.fontSize = 9;
                tmpJudul.fontStyle = FontStyles.Bold;
                tmpJudul.color = new Color(0.65f, 0.78f, 0.90f);
                tmpJudul.alignment = TextAlignmentOptions.Center;

                // Text Saldo Rekening HP (Point 1)
                GameObject saldoObj = new GameObject("TextSaldoAplikasi", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                saldoObj.transform.SetParent(panelSaldoObj.transform, false);
                RectTransform rtSaldo = saldoObj.GetComponent<RectTransform>();
                rtSaldo.anchoredPosition = new Vector2(0f, 14f);
                rtSaldo.sizeDelta = new Vector2(170f, 22f);
                TextMeshProUGUI tmpSaldo = saldoObj.GetComponent<TextMeshProUGUI>();
                if (fontAsset != null) tmpSaldo.font = fontAsset;
                tmpSaldo.text = "Rp 0";
                tmpSaldo.fontSize = 15;
                tmpSaldo.fontStyle = FontStyles.Bold;
                tmpSaldo.color = new Color(0.20f, 0.95f, 0.45f);
                tmpSaldo.alignment = TextAlignmentOptions.Center;
                textSaldoAplikasi = tmpSaldo;

                // Text Dompet Kas Player (Point 2)
                GameObject dompetObj = new GameObject("TextUangUtama", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                dompetObj.transform.SetParent(panelSaldoObj.transform, false);
                RectTransform rtDompet = dompetObj.GetComponent<RectTransform>();
                rtDompet.anchoredPosition = new Vector2(0f, -6f);
                rtDompet.sizeDelta = new Vector2(170f, 16f);
                TextMeshProUGUI tmpDompet = dompetObj.GetComponent<TextMeshProUGUI>();
                if (fontAsset != null) tmpDompet.font = fontAsset;
                tmpDompet.text = "Dompet: Rp 0";
                tmpDompet.fontSize = 9;
                tmpDompet.color = new Color(0.9f, 0.85f, 0.65f);
                tmpDompet.alignment = TextAlignmentOptions.Center;
                textUangUtama = tmpDompet;

                // Tombol Cairkan Saldo
                GameObject btnObj = new GameObject("BtnCairkan", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                btnObj.transform.SetParent(panelSaldoObj.transform, false);
                RectTransform rtBtn = btnObj.GetComponent<RectTransform>();
                rtBtn.anchoredPosition = new Vector2(0f, -28f);
                rtBtn.sizeDelta = new Vector2(140f, 22f);

                Image btnImg = btnObj.GetComponent<Image>();
                btnImg.color = new Color(0.15f, 0.65f, 0.35f, 1f);

                btnCairkanSaldo = btnObj.GetComponent<Button>();

                GameObject btnTxtObj = new GameObject("TextBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                btnTxtObj.transform.SetParent(btnObj.transform, false);
                RectTransform rtBtnTxt = btnTxtObj.GetComponent<RectTransform>();
                rtBtnTxt.anchorMin = Vector2.zero;
                rtBtnTxt.anchorMax = Vector2.one;
                rtBtnTxt.offsetMin = Vector2.zero;
                rtBtnTxt.offsetMax = Vector2.zero;
                TextMeshProUGUI tmpBtn = btnTxtObj.GetComponent<TextMeshProUGUI>();
                if (fontAsset != null) tmpBtn.font = fontAsset;
                tmpBtn.text = "Tarik ke Dompet [C]";
                tmpBtn.fontSize = 9;
                tmpBtn.fontStyle = FontStyles.Bold;
                tmpBtn.color = Color.white;
                tmpBtn.alignment = TextAlignmentOptions.Center;
            }
        }

        if (btnCairkanSaldo != null)
        {
            btnCairkanSaldo.onClick.RemoveListener(CairkanSaldo);
            btnCairkanSaldo.onClick.AddListener(CairkanSaldo);
        }
    }

    void MuatDataKeuangan()
    {
        saldoAplikasi = PlayerPrefs.GetInt("Ojek_SaldoAplikasi", 0);
        uangUtamaPlayer = PlayerPrefs.GetInt("Ojek_UangUtamaPlayer", 0);
    }

    void SimpanDataKeuangan()
    {
        PlayerPrefs.SetInt("Ojek_SaldoAplikasi", saldoAplikasi);
        PlayerPrefs.SetInt("Ojek_UangUtamaPlayer", uangUtamaPlayer);
        PlayerPrefs.Save();
    }

    [ContextMenu("Reset Uang Player")]
    public void ResetUangPlayer()
    {
        saldoAplikasi = 0;
        uangUtamaPlayer = 0;
        SimpanDataKeuangan();
        UpdateTampilanSaldo();
        Debug.Log("[BANK] Saldo aplikasi dan uang utama telah direset ke 0.");
    }

    void ResetUI()
    {
        panelPopupOrder.SetActive(false);
        panelDalamOrderan.SetActive(false);
        textStatus.text = "Status: OFF";
        isBusy = false;
        isSearching = false;
        isDelivering = false;
    }
}
