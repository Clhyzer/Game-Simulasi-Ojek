using UnityEngine;
using UnityEngine.InputSystem; // 1. Tambahkan namespace ini

public class PickUpTrigger : MonoBehaviour
{
    private bool isPlayerNearby = false;

    void Update()
    {
        // 2. Gunakan pengecekan tombol E versi Input System baru
        if (isPlayerNearby && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            AmbilPenumpang();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            Debug.Log("Tekan [E] untuk menjemput penumpang!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }

    void AmbilPenumpang()
    {
        Debug.Log("Penumpang berhasil naik ke motor!");
        
        if (NPCSpawner.Instance != null)
        {
            NPCSpawner.Instance.PenumpangDinaikkan();
        }

        Destroy(gameObject);
    }
}