using UnityEngine;
using UnityEngine.UI;
using TMPro; // Pakai TextMeshPro

public class PhoneOjekApp : MonoBehaviour
{
    [Header("UI Toggle & Text")]
    public Toggle toggleOnOff;
    public TMP_Text textStatus;

    [Header("Panels")]
    public GameObject panelPopupOrder;
    public GameObject panelDalamOrderan;

    [Header("Buttons")]
    public Button btnTerimaOrder;
    public Button btnSelesaiOrder;

    [Header("Pengaturan Orderan")]
    public float waktuTungguMin = 3f;  // Detik minimal dapet orderan
    public float waktuTungguMax = 6f;  // Detik maksimal dapet orderan

    private bool isSearching = false;
    private bool isBusy = false;

    void Start()
    {
        // Hubungkan event klik ke fungsi lewat kode
        toggleOnOff.onValueChanged.AddListener(OnToggleChanged);
        btnTerimaOrder.onClick.AddListener(TerimaOrder);
        btnSelesaiOrder.onClick.AddListener(SelesaikanOrder);

        // Sembunyikan panel-panel di awal
        ResetUI();
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
            
            // Mulai hitung mundur acak untuk dapet orderan
            MulaiCariOrderan();
        }
        else
        {
            isSearching = false;
            textStatus.text = "Status: OFF";
            
            // Batalkan pencarian jika aplikasi dimatikan
            CancelInvoke(nameof(MutilasiOrderanMasuk));
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
            textStatus.text = "ADA ORDERAN MASUK!";
            panelPopupOrder.SetActive(true); // Munculkan Pop-up
        }
    }

    // 3. Dipanggil saat tombol "Terima Order" diklik
    void TerimaOrder()
{
    isBusy = true;
    isSearching = false;

    panelPopupOrder.SetActive(false);
    panelDalamOrderan.SetActive(true);
    textStatus.text = "Status: Mengantar Penumpang";

    // 1. SPAWN NPC DI DUNIA GAME
    if (NPCSpawner.Instance != null)
    {
        NPCSpawner.Instance.SpawnNPC();
    }
}

    // 4. Dipanggil saat tombol "Selesaikan Order" diklik
    void SelesaikanOrder()
{
    isBusy = false;
    panelDalamOrderan.SetActive(false);

    // Bersihkan NPC, DropZone, dan Garis Navigasi
    if (NPCSpawner.Instance != null)
    {
        NPCSpawner.Instance.HapusSemuaJob();
    }

    if (toggleOnOff.isOn)
    {
        isSearching = true;
        textStatus.text = "Status: ON (Mencari Orderan...)";
        MulaiCariOrderan();
    }
    else
    {
        textStatus.text = "Status: OFF";
    }
}

    void ResetUI()
    {
        panelPopupOrder.SetActive(false);
        panelDalamOrderan.SetActive(false);
        textStatus.text = "Status: OFF";
    }
}