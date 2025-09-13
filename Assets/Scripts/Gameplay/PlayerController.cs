using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;

    [Header("Look Settings")]
    public float mouseSensitivity = 200f;
    public float maxHeadTurn = 90f;    // угол между телом и камерой
    public float bodyTurnSpeed = 360f; // градусы/сек, скорость поворота тела

    [Header("References")]
    public Transform playerBody;       // тело персонажа
    public Transform cameraTransform;  // камера (дочерний объект MainPlayer)

    private CharacterController controller;
    private Vector3 velocity;

    private float cameraPitch = 0f;      // вертикальный наклон
    private float cameraYaw = 0f;        // горизонтальный yaw камеры

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Инициализация вертикального наклона
        cameraPitch = cameraTransform.localEulerAngles.x;
        if (cameraPitch > 180f) cameraPitch -= 360f;

        // Инициализация yaw камеры
        cameraYaw = cameraTransform.eulerAngles.y;
    }

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        HandleMouseLook();
        HandleMovement(moveX, moveZ);
        HandleBodyRotation(moveX, moveZ);
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // вертикальный наклон камеры
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);

        // горизонтальный поворот камеры
        cameraYaw += mouseX;
        cameraYaw = Mathf.Repeat(cameraYaw, 360f);

        // камера вращается независимо от тела
        cameraTransform.rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0f);
    }

    void HandleMovement(float moveX, float moveZ)
    {
        // движение строго по горизонтали относительно камеры
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDir = forward * moveZ + right * moveX;
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        controller.Move(moveDir * moveSpeed * Time.deltaTime);

        // гравитация
        if (controller.isGrounded && velocity.y < 0f) velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleBodyRotation(float moveX, float moveZ)
    {
        float bodyYaw = playerBody.eulerAngles.y;
        float angleDiff = Mathf.DeltaAngle(bodyYaw, cameraYaw);

        bool movingForwardOnly = moveZ > 0f && Mathf.Abs(moveX) < 0.01f;

        // Если игрок движется только вперед → тело сразу поворачивается к камере
        if (movingForwardOnly)
        {
            float newBodyYaw = Mathf.MoveTowardsAngle(bodyYaw, cameraYaw, bodyTurnSpeed * Time.deltaTime);
            playerBody.rotation = Quaternion.Euler(0f, newBodyYaw, 0f);
        }
        else
        {
            // обычное правило: тело догоняет камеру только при превышении maxHeadTurn
            if (Mathf.Abs(angleDiff) > maxHeadTurn)
            {
                float newBodyYaw = Mathf.MoveTowardsAngle(bodyYaw, cameraYaw, bodyTurnSpeed * Time.deltaTime);
                playerBody.rotation = Quaternion.Euler(0f, newBodyYaw, 0f);
            }
        }
    }
}
