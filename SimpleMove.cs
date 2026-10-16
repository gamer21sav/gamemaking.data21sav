using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleMove : MonoBehaviour
{
    [Header("References")]
    public CharacterController cc;
    public Transform cam;
    private Animator animator;

    [Header("Movement")]
    public float speed = 5f;
    public float turnSpeed = 720f;        // degrees per second
    public float jumpHeight = 1.2f;
    public float gravity = -20f;

    [Header("Camera")]
    public float mouseSensitivity = 2f;
    public float cameraHeight = 1.8f;

    [Header("Respawn")]
    public Vector3 respawnPoint = new Vector3(81f, 10.095f, 107f);

    private float pitch;
    private float yaw;
    private Vector3 velocity;   // only the vertical part is used here
    private bool wasGrounded;

    // Animator parameter names (must match the Animator Controller exactly)
    static readonly int IsMoveHash = Animator.StringToHash("Ismove");
    static readonly int JumpHash = Animator.StringToHash("Jumping");
    static readonly int GroundedHash = Animator.StringToHash("Grounded");
    static readonly int VerticalHash = Animator.StringToHash("VerticalVelocity");

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        animator = GetComponent<Animator>();
        if (cc == null) cc = GetComponent<CharacterController>();
        if (cam == null && Camera.main != null) cam = Camera.main.transform;
    }

    void Update()
    {
        if (cc == null || cam == null) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        HandleJumpAndGravity();
        HandleMovement();
        HandleAiming();
        UpdateAnimator();
    }

    void LateUpdate()
    {
        if (cam == null) return;

        if (!IsValid(transform.position))
        {
            Debug.LogError($"Player position corrupted! pos={transform.position} vel={velocity}");
            cc.enabled = false;
            transform.position = respawnPoint;
            cc.enabled = true;
            velocity = Vector3.zero;
            return;
        }

        HandleCamera();
    }

    void HandleJumpAndGravity()
    {
        bool grounded = cc.isGrounded;

        // Keep a small downward force while grounded so the controller sticks to slopes
        if (grounded && velocity.y < 0f)
            velocity.y = -2f;

        if (grounded && Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (animator != null) animator.SetTrigger(JumpHash);
        }

        velocity.y += gravity * Time.deltaTime;
        wasGrounded = grounded;
    }

    void HandleMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;

        Vector3 moveDirection = camRight * x + camForward * z;
        if (moveDirection.sqrMagnitude > 1f) moveDirection.Normalize();

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        Vector3 move = moveDirection * speed;
        move.y = velocity.y;
        cc.Move(move * Time.deltaTime);
    }

    void HandleCamera()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        pitch = Mathf.Clamp(pitch - mouseY * mouseSensitivity, -90f, 90f);
        yaw += mouseX * mouseSensitivity;

        cam.rotation = Quaternion.Euler(pitch, yaw, 0f);
        cam.position = transform.position + Vector3.up * cameraHeight;
    }

    void HandleAiming()
    {
        if (animator == null) return;

        bool aiming = Input.GetMouseButton(1) || Input.GetMouseButton(0);
        float current = animator.GetLayerWeight(1);
        animator.SetLayerWeight(1, Mathf.MoveTowards(current, aiming ? 1f : 0f, Time.deltaTime * 6f));
    }

void UpdateAnimator()
{
    if (animator == null) return;

    bool moving = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).sqrMagnitude > 0.01f;
    bool grounded = cc.isGrounded;

    SetBoolIfExists(IsMoveHash, moving && grounded);
    SetBoolIfExists(GroundedHash, grounded);
    SetFloatIfExists(VerticalHash, velocity.y);
}

bool HasParam(int hash)
{
    foreach (AnimatorControllerParameter p in animator.parameters)
        if (p.nameHash == hash) return true;
    return false;
}

void SetBoolIfExists(int hash, bool value)
{
    if (HasParam(hash)) animator.SetBool(hash, value);
}

void SetFloatIfExists(int hash, float value)
{
    if (HasParam(hash)) animator.SetFloat(hash, value);
}

    static bool IsValid(Vector3 v) =>
        !float.IsNaN(v.x) && !float.IsNaN(v.y) && !float.IsNaN(v.z) &&
        !float.IsInfinity(v.x) && !float.IsInfinity(v.y) && !float.IsInfinity(v.z);
}