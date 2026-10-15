using UnityEngine;
using UnityEngine.InputSystem; // 1. Tambahkan namespace ini di atas

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

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.6f;

    private float moveInput;
    private float turnInput;
    private bool isGrounded;

    void Start()
    {
        sphereRB.transform.parent = null;
    }

    void Update()
    {
        // 2. Gunakan Keyboard.current untuk membaca input WASD / Panah
        if (Keyboard.current != null)
        {
            float vertical = 0f;
            float horizontal = 0f;

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal -= 1f;

            moveInput = vertical;
            turnInput = horizontal;
        }

        if (moveInput > 0)
        {
            moveInput *= forwardSpeed;
        }
        else
        {
            moveInput *= reverseSpeed;
        }

        transform.position = sphereRB.transform.position;

        if (isGrounded && turnInput != 0)
        {
            float rotationAmount = turnInput * turnSpeed * Time.deltaTime;
            transform.Rotate(0, rotationAmount, 0, Space.World);
        }
    }

    void FixedUpdate()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        if (isGrounded)
        {
            Vector3 moveForce = transform.forward * moveInput;
            sphereRB.AddForce(moveForce, ForceMode.Acceleration);
        }
        else
        {
            sphereRB.AddForce(Vector3.down * gravityForce, ForceMode.Acceleration);
        }
    }
}