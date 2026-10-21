using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

[InitializeOnLoad]
public static class SetupOjekUI
{
    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        EditorApplication.delayCall += () =>
        {
            ExecuteSetup(false);
        };
    }

    [MenuItem("Tools/Setup Ojek UI")]
    public static void ManualSetup()
    {
        ExecuteSetup(true);
    }

    public static void ExecuteSetup(bool isManual)
    {
        // 1. Cari Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[SetupOjekUI] Canvas tidak ditemukan di scene!");
            return;
        }

        // 2. Cari PhoneOjekApp
        PhoneOjekApp phoneApp = Object.FindFirstObjectByType<PhoneOjekApp>();
        if (phoneApp == null)
        {
            Debug.LogWarning("[SetupOjekUI] PhoneOjekApp tidak ditemukan di scene!");
            return;
        }

        // 3. Cari Font TMP default
        TMP_FontAsset fontAsset = TMP_Settings.defaultFontAsset;
        if (fontAsset == null && phoneApp.textStatus != null)
        {
            fontAsset = phoneApp.textStatus.font;
        }

        Transform phoneTransform = phoneApp.toggleOnOff != null ? phoneApp.toggleOnOff.transform.parent : null;
        if (phoneTransform == null)
        {
            phoneTransform = canvas.transform.Find("RawImage");
        }

        if (phoneTransform == null)
        {
            Debug.LogWarning("[SetupOjekUI] Phone RawImage tidak ditemukan di Canvas!");
            return;
        }

        // --- A. SETUP HUD UANG UTAMA DI LAYAR GAME (POJOK KIRI ATAS) ---
        Transform existingHUD = canvas.transform.Find("HUD_UangPlayer");
        GameObject hudObj;
        if (existingHUD == null)
        {
            hudObj = new GameObject("HUD_UangPlayer", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            hudObj.transform.SetParent(canvas.transform, false);
            Undo.RegisterCreatedObjectUndo(hudObj, "Create HUD_UangPlayer");
        }
        else
        {
            hudObj = existingHUD.gameObject;
        }

        RectTransform hudRect = hudObj.GetComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0f, 1f);
        hudRect.anchorMax = new Vector2(0f, 1f);
        hudRect.pivot = new Vector2(0f, 1f);
        hudRect.anchoredPosition = new Vector2(25f, -25f);
        hudRect.sizeDelta = new Vector2(230f, 50f);

        Image hudImage = hudObj.GetComponent<Image>();
        hudImage.color = new Color(0.08f, 0.10f, 0.14f, 0.85f); // Dark sleek HUD container

        // Child Text Uang HUD
        Transform existingHudText = hudObj.transform.Find("TextUangHUD");
        GameObject hudTextObj;
        if (existingHudText == null)
        {
            hudTextObj = new GameObject("TextUangHUD", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            hudTextObj.transform.SetParent(hudObj.transform, false);
        }
        else
        {
            hudTextObj = existingHudText.gameObject;
        }

        RectTransform hudTextRect = hudTextObj.GetComponent<RectTransform>();
        hudTextRect.anchorMin = new Vector2(0f, 0f);
        hudTextRect.anchorMax = new Vector2(1f, 1f);
        hudTextRect.pivot = new Vector2(0.5f, 0.5f);
        hudTextRect.offsetMin = new Vector2(15f, 5f);
        hudTextRect.offsetMax = new Vector2(-15f, -5f);

        TextMeshProUGUI hudTmp = hudTextObj.GetComponent<TextMeshProUGUI>();
        if (fontAsset != null) hudTmp.font = fontAsset;
        hudTmp.text = "💵 Uang: Rp 0";
        hudTmp.fontSize = 17;
        hudTmp.fontStyle = FontStyles.Bold;
        hudTmp.color = new Color(1f, 0.85f, 0.25f); // Emas
        hudTmp.alignment = TextAlignmentOptions.MidlineLeft;

        phoneApp.textUangUtamaHUD = hudTmp;

        // --- B. SETUP CARD REKENING DI HP (PANEL SALDO) ---
        Transform existingPanelSaldo = phoneTransform.Find("PanelSaldo");
        GameObject panelSaldoObj;
        if (existingPanelSaldo == null)
        {
            panelSaldoObj = new GameObject("PanelSaldo", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelSaldoObj.transform.SetParent(phoneTransform, false);
            Undo.RegisterCreatedObjectUndo(panelSaldoObj, "Create PanelSaldo");
        }
        else
        {
            panelSaldoObj = existingPanelSaldo.gameObject;
        }

        RectTransform panelSaldoRect = panelSaldoObj.GetComponent<RectTransform>();
        panelSaldoRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelSaldoRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelSaldoRect.pivot = new Vector2(0.5f, 0.5f);
        panelSaldoRect.anchoredPosition = new Vector2(0f, 15f);
        panelSaldoRect.sizeDelta = new Vector2(180f, 90f);

        Image panelSaldoImg = panelSaldoObj.GetComponent<Image>();
        panelSaldoImg.color = new Color(0.12f, 0.15f, 0.20f, 0.95f);

        // 1. Label Judul
        TextMeshProUGUI txtJudul = GetOrCreateText(panelSaldoObj.transform, "LabelJudul", fontAsset);
        RectTransform rtJudul = txtJudul.GetComponent<RectTransform>();
        rtJudul.anchorMin = new Vector2(0.5f, 0.5f);
        rtJudul.anchorMax = new Vector2(0.5f, 0.5f);
        rtJudul.pivot = new Vector2(0.5f, 0.5f);
        rtJudul.anchoredPosition = new Vector2(0f, 32f);
        rtJudul.sizeDelta = new Vector2(170f, 16f);
        txtJudul.text = "SALDO REKENING OJOL";
        txtJudul.fontSize = 9;
        txtJudul.fontStyle = FontStyles.Bold;
        txtJudul.color = new Color(0.65f, 0.78f, 0.90f);
        txtJudul.alignment = TextAlignmentOptions.Center;

        // 2. Text Saldo Aplikasi (Point 1)
        TextMeshProUGUI txtSaldo = GetOrCreateText(panelSaldoObj.transform, "TextSaldoAplikasi", fontAsset);
        RectTransform rtSaldo = txtSaldo.GetComponent<RectTransform>();
        rtSaldo.anchorMin = new Vector2(0.5f, 0.5f);
        rtSaldo.anchorMax = new Vector2(0.5f, 0.5f);
        rtSaldo.pivot = new Vector2(0.5f, 0.5f);
        rtSaldo.anchoredPosition = new Vector2(0f, 14f);
        rtSaldo.sizeDelta = new Vector2(170f, 22f);
        txtSaldo.text = "Rp 0";
        txtSaldo.fontSize = 15;
        txtSaldo.fontStyle = FontStyles.Bold;
        txtSaldo.color = new Color(0.20f, 0.95f, 0.45f); // Hijau ojol terang
        txtSaldo.alignment = TextAlignmentOptions.Center;
        phoneApp.textSaldoAplikasi = txtSaldo;

        // 3. Text Uang Dompet Kas
        TextMeshProUGUI txtDompet = GetOrCreateText(panelSaldoObj.transform, "TextUangUtama", fontAsset);
        RectTransform rtDompet = txtDompet.GetComponent<RectTransform>();
        rtDompet.anchorMin = new Vector2(0.5f, 0.5f);
        rtDompet.anchorMax = new Vector2(0.5f, 0.5f);
        rtDompet.pivot = new Vector2(0.5f, 0.5f);
        rtDompet.anchoredPosition = new Vector2(0f, -6f);
        rtDompet.sizeDelta = new Vector2(170f, 16f);
        txtDompet.text = "Dompet: Rp 0";
        txtDompet.fontSize = 9;
        txtDompet.color = new Color(0.9f, 0.85f, 0.65f);
        txtDompet.alignment = TextAlignmentOptions.Center;
        phoneApp.textUangUtama = txtDompet;

        // 4. Tombol Cairkan Saldo
        Transform existingBtnCairkan = panelSaldoObj.transform.Find("BtnCairkan");
        GameObject btnCairkanObj;
        if (existingBtnCairkan == null)
        {
            btnCairkanObj = new GameObject("BtnCairkan", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnCairkanObj.transform.SetParent(panelSaldoObj.transform, false);
        }
        else
        {
            btnCairkanObj = existingBtnCairkan.gameObject;
        }

        RectTransform rtBtn = btnCairkanObj.GetComponent<RectTransform>();
        rtBtn.anchorMin = new Vector2(0.5f, 0.5f);
        rtBtn.anchorMax = new Vector2(0.5f, 0.5f);
        rtBtn.pivot = new Vector2(0.5f, 0.5f);
        rtBtn.anchoredPosition = new Vector2(0f, -28f);
        rtBtn.sizeDelta = new Vector2(140f, 22f);

        Image btnImg = btnCairkanObj.GetComponent<Image>();
        btnImg.color = new Color(0.15f, 0.65f, 0.35f, 1f); // Hijau segar ojek

        Button btnComp = btnCairkanObj.GetComponent<Button>();
        phoneApp.btnCairkanSaldo = btnComp;

        TextMeshProUGUI btnTxt = GetOrCreateText(btnCairkanObj.transform, "TextBtn", fontAsset);
        RectTransform rtBtnTxt = btnTxt.GetComponent<RectTransform>();
        rtBtnTxt.anchorMin = Vector2.zero;
        rtBtnTxt.anchorMax = Vector2.one;
        rtBtnTxt.offsetMin = Vector2.zero;
        rtBtnTxt.offsetMax = Vector2.zero;
        btnTxt.text = "Tarik ke Dompet [C]";
        btnTxt.fontSize = 9;
        btnTxt.fontStyle = FontStyles.Bold;
        btnTxt.color = Color.white;
        btnTxt.alignment = TextAlignmentOptions.Center;

        // --- C. PENYESUAIAN PANEL POPUP ORDER ---
        if (phoneApp.panelPopupOrder != null)
        {
            Image popupBg = phoneApp.panelPopupOrder.GetComponent<Image>();
            if (popupBg != null) popupBg.color = new Color(0.10f, 0.13f, 0.18f, 0.98f);

            TextMeshProUGUI popupTxt = phoneApp.panelPopupOrder.GetComponentInChildren<TextMeshProUGUI>();
            if (popupTxt != null)
            {
                phoneApp.textPopupInfo = popupTxt;
                popupTxt.fontSize = 11;
                popupTxt.alignment = TextAlignmentOptions.Center;
                popupTxt.color = Color.white;
                RectTransform rt = popupTxt.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(0f, 35f);
                rt.sizeDelta = new Vector2(180f, 85f);
            }

            if (phoneApp.btnTerimaOrder != null)
            {
                Image bImg = phoneApp.btnTerimaOrder.GetComponent<Image>();
                if (bImg != null) bImg.color = new Color(0.15f, 0.65f, 0.35f, 1f);
                RectTransform bRt = phoneApp.btnTerimaOrder.GetComponent<RectTransform>();
                bRt.anchoredPosition = new Vector2(0f, -50f);
                bRt.sizeDelta = new Vector2(140f, 30f);
                TextMeshProUGUI bTxt = phoneApp.btnTerimaOrder.GetComponentInChildren<TextMeshProUGUI>();
                if (bTxt != null)
                {
                    bTxt.text = "TERIMA ORDER";
                    bTxt.fontSize = 11;
                    bTxt.fontStyle = FontStyles.Bold;
                }
            }
        }

        // --- D. PENYESUAIAN PANEL DALAM ORDERAN ---
        if (phoneApp.panelDalamOrderan != null)
        {
            Image dalamBg = phoneApp.panelDalamOrderan.GetComponent<Image>();
            if (dalamBg != null) dalamBg.color = new Color(0.10f, 0.13f, 0.18f, 0.98f);

            TextMeshProUGUI dalamTxt = phoneApp.panelDalamOrderan.GetComponentInChildren<TextMeshProUGUI>();
            if (dalamTxt != null)
            {
                phoneApp.textDalamOrderanInfo = dalamTxt;
                dalamTxt.fontSize = 11;
                dalamTxt.alignment = TextAlignmentOptions.Center;
                dalamTxt.color = Color.white;
                RectTransform rt = dalamTxt.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(0f, 35f);
                rt.sizeDelta = new Vector2(180f, 85f);
            }

            if (phoneApp.btnSelesaiOrder != null)
            {
                Image bImg = phoneApp.btnSelesaiOrder.GetComponent<Image>();
                if (bImg != null) bImg.color = new Color(0.85f, 0.25f, 0.25f, 1f);
                RectTransform bRt = phoneApp.btnSelesaiOrder.GetComponent<RectTransform>();
                bRt.anchoredPosition = new Vector2(0f, -50f);
                bRt.sizeDelta = new Vector2(150f, 30f);
                TextMeshProUGUI bTxt = phoneApp.btnSelesaiOrder.GetComponentInChildren<TextMeshProUGUI>();
                if (bTxt != null)
                {
                    bTxt.text = "SELESAIKAN ORDER";
                    bTxt.fontSize = 10;
                    bTxt.fontStyle = FontStyles.Bold;
                }
            }
        }

        // Simpan perubahan ke Scene
        EditorUtility.SetDirty(phoneApp);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

        Debug.Log("[SetupOjekUI] UI Ojek (HUD Uang, Rekening HP, Pop-up Order & Dalam Orderan) berhasil dibuat dan disimpan ke Scene!");
    }

    private static TextMeshProUGUI GetOrCreateText(Transform parent, string name, TMP_FontAsset font)
    {
        Transform existing = parent.Find(name);
        GameObject obj;
        if (existing == null)
        {
            obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            obj.transform.SetParent(parent, false);
        }
        else
        {
            obj = existing.gameObject;
        }

        TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        return tmp;
    }
}
