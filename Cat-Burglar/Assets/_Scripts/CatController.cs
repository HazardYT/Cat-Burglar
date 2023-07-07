using UnityEngine;

public class CatController : MonoBehaviour
{
    public float moveSpeed = 5f;        // Movement speed
    public float sprintSpeed = 10f;     // Sprint speed
    public float jumpForce = 5f;        // Jump force
    public float rotationSpeed = 10f;   // Rotation speed

    private float cameraFollowSpeed = 0.1f;  // Camera follow speed
    private Vector3 cameraOffset;       // Offset between player and camera
    private bool isJumping = false;     // Flag to check if the player is jumping

    private CharacterController controller;
    private Transform cameraTransform;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;
        cameraOffset = transform.position - cameraTransform.position;
    }

    void Update()
    {
        // Player movement
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool sprint = Input.GetKey(KeyCode.LeftShift);

        Vector3 movement = new Vector3(horizontal, 0f, vertical).normalized;

        // Rotate player towards movement direction
        if (movement != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(movement, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }

        // Calculate movement speed
        float speed = sprint ? sprintSpeed : moveSpeed;

        // Apply movement to the player
        Vector3 moveDirection = transform.TransformDirection(movement);
        moveDirection *= speed;

        controller.Move(moveDirection);

        // Jumping
        if (Input.GetButtonDown("Jump") && !controller.isGrounded)
        {
            moveDirection.y = jumpForce;
            isJumping = true;
        }

        // Camera follow
        Vector3 targetCameraPosition = transform.position - cameraOffset;
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetCameraPosition, cameraFollowSpeed);
    }
}
