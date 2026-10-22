using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CarWaypointAI : MonoBehaviour
{
    [Header("Rute")]
    [Tooltip("Objek induk yang anak-anaknya adalah titik waypoint, urut sesuai arah jalan")]
    public Transform waypointParent;
    public bool acakRute = false;          // true = pilih waypoint acak, false = berurutan
    public bool mulaiDariTerdekat = true;

    [Header("Gerak Mobil")]
    public float kecepatan = 8f;
    public float percepatan = 6f;
    public float kecepatanBelok = 90f;     // derajat per detik
    public float jarakSampai = 2.5f;       // jarak ke waypoint dianggap sudah sampai

    Transform[] titik;
    int indeks;
    NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = kecepatan;
        agent.acceleration = percepatan;
        agent.angularSpeed = kecepatanBelok;
        agent.autoBraking = false;         // penting: supaya tidak melambat/berhenti di tiap waypoint

        int n = waypointParent.childCount;
        titik = new Transform[n];
        for (int i = 0; i < n; i++)
            titik[i] = waypointParent.GetChild(i);

        if (mulaiDariTerdekat)
            indeks = CariTerdekat();

        agent.SetDestination(titik[indeks].position);
    }

    void Update()
    {
        if (agent.pathPending) return;

        if (agent.remainingDistance <= jarakSampai)
        {
            indeks = acakRute ? Random.Range(0, titik.Length) : (indeks + 1) % titik.Length;
            agent.SetDestination(titik[indeks].position);
        }
    }

    int CariTerdekat()
    {
        int terbaik = 0;
        float jarakMin = float.MaxValue;
        for (int i = 0; i < titik.Length; i++)
        {
            float d = (titik[i].position - transform.position).sqrMagnitude;
            if (d < jarakMin) { jarakMin = d; terbaik = i; }
        }
        return terbaik;
    }

    void OnDrawGizmosSelected()
    {
        if (waypointParent == null) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypointParent.childCount; i++)
        {
            Transform a = waypointParent.GetChild(i);
            Transform b = waypointParent.GetChild((i + 1) % waypointParent.childCount);
            Gizmos.DrawSphere(a.position, 0.5f);
            Gizmos.DrawLine(a.position, b.position);
        }
    }
}