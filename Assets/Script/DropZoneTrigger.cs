using UnityEngine;

public class DropZoneTrigger : MonoBehaviour
{
    public static bool isPlayerInDropZone = false;
    private int colliderCount = 0;

    private bool IsPlayer(Collider other)
    {
        return other.CompareTag("Player") || other.transform.root.CompareTag("Player");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;
        colliderCount++;
        isPlayerInDropZone = true;
        Debug.Log("Player masuk area tujuan!");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;
        colliderCount = Mathf.Max(0, colliderCount - 1);
        if (colliderCount == 0) isPlayerInDropZone = false;
    }

    private void OnDisable()
    {
        colliderCount = 0;
        isPlayerInDropZone = false;
    }
}