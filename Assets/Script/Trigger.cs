using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Collider))]
public class Trigger : MonoBehaviour
{
    [Header("Teks yang Muncul")]
    [TextArea(1, 3)]
    [SerializeField] private string teks = "Selesaikan orderan";

    [Header("Pengaturan")]
    [SerializeField] private string tagPlayer = "Player";

    [Tooltip("Opsional: pakai objek UI buatanmu sendiri. Kosongkan untuk memakai teks otomatis di tengah bawah layar.")]
    [SerializeField] private GameObject objekUISendiri;

    [Header("Event Opsional")]
    public UnityEvent saatPlayerMasuk;
    public UnityEvent saatPlayerKeluar;

    private bool playerDiDalam = false;

    // UI otomatis dipakai bersama; pemilik = trigger yang sedang menampilkannya
    private static GameObject promptRoot;
    private static TMP_Text promptText;
    private static Trigger pemilikAktif;

    private void Reset()
    {
        // Otomatis jadikan collider sebagai trigger saat script dipasang
        Collider c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    private void Start()
    {
        if (objekUISendiri != null) objekUISendiri.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(tagPlayer)) return;

        playerDiDalam = true;
        Tampilkan();
        saatPlayerMasuk?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(tagPlayer)) return;

        playerDiDalam = false;
        Sembunyikan();
        saatPlayerKeluar?.Invoke();
    }

    private void OnDisable()
    {
        if (playerDiDalam)
        {
            playerDiDalam = false;
            Sembunyikan();
        }
    }

    // --- TAMPIL / SEMBUNYI ---

    private void Tampilkan()
    {
        if (objekUISendiri != null)
        {
            objekUISendiri.SetActive(true);
            return;
        }

        BuatPromptJikaBelumAda();
        pemilikAktif = this;
        promptText.text = teks;
        promptRoot.SetActive(true);
    }

    private void Sembunyikan()
    {
        if (objekUISendiri != null)
        {
            objekUISendiri.SetActive(false);
            return;
        }

        // Sembunyikan hanya kalau prompt ini milik trigger yang sama,
        // supaya tidak mematikan prompt dari trigger lain yang masih aktif
        if (pemilikAktif == this && promptRoot != null)
        {
            promptRoot.SetActive(false);
            pemilikAktif = null;
        }
    }

    private static void BuatPromptJikaBelumAda()
    {
        if (promptRoot != null) return;

        GameObject canvasObj = new GameObject("TriggerPromptCanvas", typeof(Canvas), typeof(CanvasScaler));
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        promptRoot = new GameObject("PromptTeks", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        promptRoot.transform.SetParent(canvasObj.transform, false);

        RectTransform rt = promptRoot.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 140f);
        rt.sizeDelta = new Vector2(560f, 80f);

        promptRoot.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.14f, 0.88f);

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