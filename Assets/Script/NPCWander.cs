using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCWander : MonoBehaviour
{
    [Header("Pengaturan Jalan")]
    public float radius = 15f;          // jangkauan dari titik awal
    public float waitTimeMin = 2f;      // lama berhenti minimum
    public float waitTimeMax = 5f;      // lama berhenti maksimum

    [Header("Opsional")]
    public Animator animator;           // kosongkan kalau tidak pakai animasi

    NavMeshAgent agent;
    Vector3 center;
    float timer;
    float waitTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        center = transform.position;
        waitTime = Random.Range(waitTimeMin, waitTimeMax);
    }

    void Update()
    {
        if (animator != null)
            animator.SetFloat("Speed", agent.velocity.magnitude);

        bool sudahSampai = !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;

        if (sudahSampai)
        {
            timer += Time.deltaTime;
            if (timer >= waitTime)
            {
                JalanKeTitikAcak();
                timer = 0f;
                waitTime = Random.Range(waitTimeMin, waitTimeMax);
            }
        }
    }

    void JalanKeTitikAcak()
    {
        Vector3 acak = center + Random.insideUnitSphere * radius;
        if (NavMesh.SamplePosition(acak, out NavMeshHit hit, radius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    // Menampilkan area jelajah di Scene view saat NPC dipilih
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Application.isPlaying ? center : transform.position, radius);
    }
}