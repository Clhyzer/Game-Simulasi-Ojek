using UnityEngine;
using UnityEngine.InputSystem;

public class SphereMotorController : MonoBehaviour
{
    public static SphereMotorController Instance { get; private set; }

    [Header("References")]
    public Rigidbody sphereRB;
    public Transform motorModel;

    [Header("Passenger / Boncengan Settings")]
    [Tooltip("Titik / Transform tempat duduk penumpang. Jika kosong, bisa diisi manual atau otomatis menggunakan passengerOffset.")]
    public Transform passengerSeatPoint;

    [Tooltip("Offset posisi penumpang jika passengerSeatPoint tidak ditentukan (X: samping, Y: tinggi, Z: maju/mundur).")]
    public Vector3 passengerOffset = new Vector3(0f, 1.1f, -0.6f);

    [Tooltip("Rotasi lokal penumpang saat duduk di motor (derajat).")]
    public Vector3 passengerRotation = Vector3.zero;

    [Tooltip("Aktifkan jika ingin mengubah ukuran (scale) NPC saat naik motor.")]
    public bool customPassengerScale = false;

    [Tooltip("Ukuran lokal penumpang jika customPassengerScale dicentang.")]
    public Vector3 passengerScale = Vector3.one;

    private GameObject currentPassenger;

    [Header("Movement Settings")]
    public float forwardSpeed = 80f;
    public float reverseSpeed = 40f;
    public float turnSpeed = 100f;
    public float gravityForce = 20f;

    [Header("Steering Settings")]
    [Tooltip("Seberapa cepat arah gerak bola mengikuti arah motor.")]
    public float steeringForce = 5f;

    [Tooltip("Kecepatan minimum agar motor bisa berbelok.")]
    public float minTurnSpeed = 0.5f;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.6f;

    private float moveInput;
    private float turnInput;
    private bool isGrounded;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (sphereRB == null)
        {
            Debug.LogError(
                "Sphere RB belum diisi di Inspector!"
            );
            enabled = false;
            return;
        }

        // Pisahkan bola fisika dari objek controller.
        sphereRB.transform.SetParent(null);

        sphereRB.interpolation =
            RigidbodyInterpolation.Interpolate;

        sphereRB.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;
    }

    private void Update()
    {
        ReadInput();

        // Controller mengikuti posisi bola.
        transform.position = sphereRB.position;

        // Putar arah hadap controller saat membelok.
        if (isGrounded && Mathf.Abs(turnInput) > 0.01f)
        {
            float rotationAmount =
                turnInput * turnSpeed * Time.deltaTime;

            transform.Rotate(
                0f,
                rotationAmount,
                0f,
                Space.World
            );
        }

        // Model visual mengikuti arah controller.
        if (motorModel != null)
        {
            motorModel.rotation = transform.rotation;
        }
    }

    private void ReadInput()
    {
        moveInput = 0f;
        turnInput = 0f;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.wKey.isPressed ||
            Keyboard.current.upArrowKey.isPressed)
        {
            moveInput = 1f;
        }
        else if (Keyboard.current.sKey.isPressed ||
                 Keyboard.current.downArrowKey.isPressed)
        {
            moveInput = -1f;
        }

        if (Keyboard.current.dKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed)
        {
            turnInput += 1f;
        }

        if (Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed)
        {
            turnInput -= 1f;
        }
    }

    private void FixedUpdate()
    {
        if (sphereRB == null)
            return;

        // Periksa apakah bola berada di atas tanah.
        isGrounded = Physics.Raycast(
            sphereRB.position,
            Vector3.down,
            groundCheckDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        if (isGrounded)
        {
            ApplyMovement();
            ApplySteering();
        }
        else
        {
            sphereRB.AddForce(
                Vector3.down * gravityForce,
                ForceMode.Acceleration
            );
        }
    }

    private void ApplyMovement()
    {
        float speed = moveInput >= 0f
            ? forwardSpeed
            : reverseSpeed;

        Vector3 moveForce =
            transform.forward * moveInput * speed;

        sphereRB.AddForce(
            moveForce,
            ForceMode.Acceleration
        );
    }

    private void ApplySteering()
    {
        Vector3 velocity = sphereRB.linearVelocity;

        // Abaikan gerakan vertikal saat menghitung belokan.
        Vector3 horizontalVelocity =
            new Vector3(velocity.x, 0f, velocity.z);

        if (horizontalVelocity.magnitude < minTurnSpeed)
            return;

        // Ubah arah gerak mengikuti arah hadap motor.
        Vector3 targetDirection = transform.forward;

        Vector3 newDirection = Vector3.RotateTowards(
            horizontalVelocity.normalized,
            targetDirection,
            steeringForce * Mathf.Abs(turnInput) * Time.fixedDeltaTime,
            0f
        );

        // Pertahankan kecepatan horizontal agar belokan
        // tidak membuat bola mendadak berhenti.
        Vector3 newVelocity =
            newDirection * horizontalVelocity.magnitude;

        sphereRB.linearVelocity = new Vector3(
            newVelocity.x,
            velocity.y,
            newVelocity.z
        );
    }

    /// <summary>
    /// Mendapatkan titik boncengan penumpang. Jika belum ada, akan mencari child PassengerSeat atau membuatnya otomatis.
    /// </summary>
    public Transform GetPassengerSeat()
    {
        if (passengerSeatPoint != null)
            return passengerSeatPoint;

        Transform parentTransform = motorModel != null ? motorModel : transform;

        // Cari apakah sudah ada child bernama "PassengerSeat"
        Transform existingSeat = parentTransform.Find("PassengerSeat");
        if (existingSeat != null)
        {
            passengerSeatPoint = existingSeat;
            return passengerSeatPoint;
        }

        // Buat GameObject tempat duduk otomatis jika belum ada
        GameObject seatObj = new GameObject("PassengerSeat");
        seatObj.transform.SetParent(parentTransform);
        seatObj.transform.localPosition = passengerOffset;
        seatObj.transform.localRotation = Quaternion.Euler(passengerRotation);
        passengerSeatPoint = seatObj.transform;

        return passengerSeatPoint;
    }

    /// <summary>
    /// Menaikkan NPC ke jok belakang motor.
    /// </summary>
    public void NaikkanPenumpang(GameObject npc)
    {
        if (npc == null) return;

        currentPassenger = npc;

        // 1. Nonaktifkan semua collider pada NPC agar tidak berbenturan atau memicu trigger saat berkendara
        Collider[] colliders = npc.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        // 2. Nonaktifkan Rigidbody jika ada agar tidak bentrok dengan pergerakan motor
        Rigidbody rb = npc.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 3. Nonaktifkan script PickUpTrigger agar tidak dapat dijemput berulang kali
        PickUpTrigger pickUp = npc.GetComponent<PickUpTrigger>();
        if (pickUp != null)
        {
            pickUp.enabled = false;
        }

        // 4. Pasang NPC sebagai anak (child) dari titik tempat duduk motor
        Transform seat = GetPassengerSeat();
        npc.transform.SetParent(seat);
        npc.transform.localPosition = Vector3.zero;
        npc.transform.localRotation = Quaternion.identity;

        // 5. Atur ukuran jika customPassengerScale diaktifkan
        if (customPassengerScale)
        {
            npc.transform.localScale = passengerScale;
        }

        Debug.Log($"Penumpang [{npc.name}] berhasil naik ke motor di boncengan!");
    }

    /// <summary>
    /// Menurunkan penumpang dari motor.
    /// </summary>
    public void TurunkanPenumpang()
    {
        currentPassenger = null;
    }

    private void OnDrawGizmosSelected()
    {
        Transform parentTransform = motorModel != null ? motorModel : transform;
        Vector3 seatPos = passengerSeatPoint != null 
            ? passengerSeatPoint.position 
            : parentTransform.TransformPoint(passengerOffset);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(seatPos, 0.25f);
        Gizmos.color = Color.cyan;
        Vector3 forwardDir = passengerSeatPoint != null 
            ? passengerSeatPoint.forward 
            : parentTransform.forward;
        Gizmos.DrawRay(seatPos, forwardDir * 0.5f);
    }
}
