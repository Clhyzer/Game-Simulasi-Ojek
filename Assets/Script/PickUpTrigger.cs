using UnityEngine;
using UnityEngine.InputSystem;

public class PickUpTrigger : MonoBehaviour
{
    private bool isPlayerNearby = false;
    private SphereMotorController nearbyMotor;

    void Update()
    {
        // Gunakan pengecekan tombol E versi Input System baru
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

            // Cari controller motor dari collider yang masuk
            nearbyMotor = other.GetComponentInParent<SphereMotorController>();
            if (nearbyMotor == null)
            {
                nearbyMotor = SphereMotorController.Instance;
            }

            Debug.Log("Tekan [E] untuk menjemput penumpang!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            nearbyMotor = null;
        }
    }

    void AmbilPenumpang()
    {
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
}
