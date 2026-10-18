
using UnityEngine;
using UnityEngine.InputSystem;

public class SphereMotorController : MonoBehaviour
{
    [Header("References")]
    public Rigidbody sphereRB;
    public Transform motorModel;

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
}
