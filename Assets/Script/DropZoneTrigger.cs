using UnityEngine;

public class DropZoneTrigger : MonoBehaviour
{
    public static bool isPlayerInDropZone = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInDropZone = true;
            Debug.Log("Player masuk area tujuan!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInDropZone = false;
        }
    }

    private void OnDestroy()
    {
        isPlayerInDropZone = false;
    }
}