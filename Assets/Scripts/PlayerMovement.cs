using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody rb;
    public CapsuleCollider col;

    [Header("Movement")]
    public float walkSpeed = 8f;
    public float crouchSpeed = 4f;
    public float jumpForce = 5f;

    [Header("Crouch Settings")]
    public float normalHeight = 2f;
    public float crouchHeight = 1f;
    public Transform cameraTransform;
    public float cameraNormalY = 0.8f;
    public float cameraCrouchY = 0.4f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;

    bool isGrounded;
    float currentSpeed;

    void Start()
    {
        rb.freezeRotation = true;
        rb.useGravity = true;
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        HandleCrouch();

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = (transform.right * x + transform.forward * z).normalized;

        Vector3 targetVelocity = move * currentSpeed;
        targetVelocity.y = rb.linearVelocity.y;

        rb.linearVelocity = targetVelocity;
    }

    void HandleCrouch()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            col.height = crouchHeight;
            currentSpeed = crouchSpeed;
            cameraTransform.localPosition = new Vector3(0, cameraCrouchY, 0);
        }
        else
        {
            col.height = normalHeight;
            currentSpeed = walkSpeed;
            cameraTransform.localPosition = new Vector3(0, cameraNormalY, 0);
        }
    }
}