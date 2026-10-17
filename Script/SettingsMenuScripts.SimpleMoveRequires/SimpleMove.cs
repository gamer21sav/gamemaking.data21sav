
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleMove : MonoBehaviour
{
    [Header("References")]
    public CharacterController cc;
    public Transform cam;

    Animator animator;

    [Header("Movement")]
    public float speed = 5f;
    public float turnSpeed = 720f;
    public float jumpHeight = 1.2f;
    public float gravity = -20f;

    [Header("Camera")]
    public float cameraHeight = 1.8f;

    [Header("Respawn")]
    public Vector3 respawnPoint =
    new Vector3(81f, 10.095f, 107f);

    [Header("Control")]
    public bool isLocalPlayer = false;   // tick this on the Y Bot you control

    float pitch;
    float yaw;
    Vector3 velocity;

    static readonly int IsMoveHash =
        Animator.StringToHash("Ismove");
    static readonly int JumpHash =
        Animator.StringToHash("Jumping");
    static readonly int GroundedHash =
        Animator.StringToHash("Grounded");
    static readonly int VerticalHash =
        Animator.StringToHash("VerticalVelocity");

    GameSettings Settings => GameSettings.Instance;
    public void ApplyLocalState()
{
    if (cam == null) return;

    Camera c = cam.GetComponent<Camera>();
    AudioListener al = cam.GetComponent<AudioListener>();
    if (cam != null)
{
    foreach (Canvas canvas in cam.GetComponentsInChildren<Canvas>(true))
        canvas.enabled = isLocalPlayer;
}
    if (c != null) c.enabled = isLocalPlayer;
    if (al != null) al.enabled = isLocalPlayer;

    if (!isLocalPlayer) return;

    if (Settings != null && c != null)
    {
        Settings.playerCamera = c;
        c.fieldOfView = Settings.fov;
    }

    // Start the look angles from this bot's current camera rotation
    Vector3 angles = cam.eulerAngles;
    pitch = angles.x > 180f ? angles.x - 360f : angles.x;
    yaw = angles.y;
}
    void Start()
{
    animator = GetComponent<Animator>();

    if (cc == null)
        cc = GetComponent<CharacterController>();

    if (cam == null && Camera.main != null)
        cam = Camera.main.transform;

    ApplyLocalState();
    SetCursor(true);
}

    void Update()
    {
        if (cc == null || cam == null) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Let the settings UI handle pause-menu opening.
            if (SettingsUI.Instance == null)
                SetCursor(false);
        }

        if (SettingsUI.IsOpen) return;

        if (Input.GetMouseButtonDown(0) &&
            Cursor.lockState != CursorLockMode.Locked)
        {
            SetCursor(true);
        }

        HandleJumpAndGravity();
        HandleMovement();
        HandleAiming();
        UpdateAnimator();
    }

    void LateUpdate()
    {
        if (cam == null || cc == null) return;

        if (!IsValid(transform.position))
        {
            Debug.LogError(
                $"Player position corrupted! pos={transform.position} vel={velocity}");

            cc.enabled = false;
            transform.position = respawnPoint;
            cc.enabled = true;
            velocity = Vector3.zero;
            return;
        }

        if (!SettingsUI.IsOpen && isLocalPlayer)
        HandleCamera();
    }

    void HandleJumpAndGravity()
    {
        bool grounded = cc.isGrounded;

        if (grounded && velocity.y < 0f)
            velocity.y = -2f;

        if (grounded && GetKeyDown("Jump"))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            if (animator != null)
                animator.SetTrigger(JumpHash);
        }

        velocity.y += gravity * Time.deltaTime;
    }

    void HandleMovement()
    {
        float x = 0f;
        float z = 0f;

        if (GetKey("Left")) x -= 1f;
        if (GetKey("Right")) x += 1f;
        if (GetKey("Backward")) z -= 1f;
        if (GetKey("Forward")) z += 1f;

        Vector3 camForward =
            Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 camRight =
            Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;

        Vector3 moveDirection = camRight * x + camForward * z;

        if (moveDirection.sqrMagnitude > 1f)
            moveDirection.Normalize();

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(moveDirection, Vector3.up);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime);
        }

        Vector3 move = moveDirection * speed;

        if (GetKey("Sprint"))
            move *= 1.5f;

        move.y = velocity.y;
        cc.Move(move * Time.deltaTime);
    }

    void HandleCamera()
    {
        float sensX = Settings != null ? Settings.sensitivityX : 2f;
        float sensY = Settings != null ? Settings.sensitivityY : 2f;
        float aimMultiplier =
            Settings != null ? Settings.aimSensitivity : 0.5f;

        bool aiming = GetKey("Aim") || GetKey("Fire");

        if (aiming)
        {
            sensX *= aimMultiplier;
            sensY *= aimMultiplier;
        }

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        float invert = Settings != null && Settings.invertY ? 1f : -1f;

        yaw += mouseX * sensX;
        pitch += mouseY * sensY * invert;
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        cam.rotation = Quaternion.Euler(pitch, yaw, 0f);
        cam.position = transform.position + Vector3.up * cameraHeight;
    }

    void HandleAiming()
    {
        if (animator == null) return;

        bool aiming = GetKey("Aim") || GetKey("Fire");
        float current = animator.GetLayerWeight(1);

        animator.SetLayerWeight(
            1, Mathf.MoveTowards(current, aiming ? 1f : 0f,
            Time.deltaTime * 6f));
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        bool moving =
            GetKey("Forward") || GetKey("Backward") ||
            GetKey("Left") || GetKey("Right");

        bool grounded = cc.isGrounded;

        SetBoolIfExists(IsMoveHash, moving && grounded);
        SetBoolIfExists(GroundedHash, grounded);
        SetFloatIfExists(VerticalHash, velocity.y);
    }

    bool GetKey(string action)
    {
        if (!isLocalPlayer || Settings == null) return false;
        KeyCode key = Settings.GetKey(action);
        return key != KeyCode.None && Input.GetKey(key);
    }

    bool GetKeyDown(string action)
    {
        if (!isLocalPlayer || Settings == null) return false;
        KeyCode key = Settings.GetKey(action);
        return key != KeyCode.None && Input.GetKeyDown(key);
    }

    bool HasParam(int hash)
    {
        if (animator == null) return false;

        foreach (AnimatorControllerParameter p in animator.parameters)
            if (p.nameHash == hash) return true;

        return false;
    }

    void SetBoolIfExists(int hash, bool value)
    {
        if (HasParam(hash))
            animator.SetBool(hash, value);
    }

    void SetFloatIfExists(int hash, float value)
    {
        if (HasParam(hash))
            animator.SetFloat(hash, value);
    }

    public static void SetCursor(bool locked)
    {
        Cursor.lockState = locked
            ? CursorLockMode.Locked
            : CursorLockMode.None;

        Cursor.visible = !locked;
    }

    static bool IsValid(Vector3 v) =>
        !float.IsNaN(v.x) && !float.IsNaN(v.y) &&
        !float.IsNaN(v.z) && !float.IsInfinity(v.x) &&
        !float.IsInfinity(v.y) && !float.IsInfinity(v.z);
}
