using UnityEngine;
using TMPro;

public class DropZoneTrigger : MonoBehaviour
{
    public static bool isPlayerInDropZone = false;
    private int colliderCount = 0;

    [Header("UI")]
    [SerializeField] private GameObject orderPrompt;
    [SerializeField] private string promptMessage = "Selesaikan orderan";
    [SerializeField] private TMP_Text promptText;

    private void Awake()
    {
        // Cek kesalahan setup
        if (orderPrompt == null)
            Debug.LogWarning("[DropZone] orderPrompt belum diisi di Inspector!", this);
        else if (orderPrompt == gameObject || transform.IsChildOf(orderPrompt.transform))
            Debug.LogError("[DropZone] orderPrompt sama dengan / induk dari objek DropZone. Trigger akan mati saat prompt disembunyikan!", this);

        Collider col = GetComponent<Collider>();
        if (col == null)
            Debug.LogError("[DropZone] Tidak ada Collider di objek ini!", this);
        else if (!col.isTrigger)
            Debug.LogError("[DropZone] Collider belum dicentang Is Trigger!", this);
    }

    private void Start()
    {
        if (promptText != null) promptText.text = promptMessage;
        SetPrompt(false);
    }

    private bool IsPlayer(Collider other)
    {
        return other.CompareTag("Player") || other.transform.root.CompareTag("Player");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("[DropZone] Terpicu oleh: " + other.name);
        if (!IsPlayer(other)) return;

        colliderCount++;
        isPlayerInDropZone = true;
        SetPrompt(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;

        colliderCount = Mathf.Max(0, colliderCount - 1);
        if (colliderCount == 0)
        {
            isPlayerInDropZone = false;
            SetPrompt(false);
        }
    }

    private void OnDisable()
    {
        colliderCount = 0;
        isPlayerInDropZone = false;
        SetPrompt(false);
    }

    private void SetPrompt(bool show)
    {
        if (orderPrompt != null) orderPrompt.SetActive(show);
    }
}