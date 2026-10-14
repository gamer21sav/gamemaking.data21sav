using UnityEngine;

public class SimpleMove : MonoBehaviour
{
    private Animator animator;
    public CharacterController cc;
    public float speed = 5f;
    public Transform cam;
    private float pitch = 0f;
    private float yaw = 0f;
    public float mouseSensitivity = 2f;
    private float jumpForce = 2.5f;
    private float gravity = -9.81f;
    private Vector3 velocity;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        if (animator == null)
            {animator = GetComponent<Animator>();}

        if (cam == null && Camera.main != null)
            {cam = Camera.main.transform;}

        if (cc == null)
            {cc = GetComponent<CharacterController>();}
    }

    void Update()
    {
        if (cam == null)
            {return;}

        jumping();
        SimpleMovement();
        CameraMovement();
        aiming();
    }

    void SimpleMovement()
    {
        if (cc == null)
            {return;}

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up);
        Vector3 camRight = Vector3.ProjectOnPlane(cam.right, Vector3.up);

        if (camForward.sqrMagnitude < 0.0001f)
            {camForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);}
        if (camRight.sqrMagnitude < 0.0001f)
            {camRight = Vector3.ProjectOnPlane(transform.right, Vector3.up);}

        Vector3 moveDirection = camRight * x + camForward * z;

        if (moveDirection.magnitude > 0.001f)
        {
            moveDirection = moveDirection.normalized;
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.1f);
        }

        if (animator != null )
        {animator.SetBool("Ismove", moveDirection.sqrMagnitude > 0.01f);}

        Vector3 moveVector = moveDirection * speed;
        moveVector.y = velocity.y;
        cc.Move(moveVector * Time.deltaTime);
    }

    void CameraMovement()
    {
        if (cam == null)
            return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        pitch -= mouseY * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -90f, 90f);
        yaw += mouseX * mouseSensitivity;

        cam.rotation = Quaternion.Euler(pitch, yaw, 0);
        cam.position = transform.position + new Vector3(0f, 1.8f, 0f);
    }

    void jumping()
    {
        if (cc != null && cc.isGrounded && velocity.y < 0f)
            {velocity.y = -2f;}



        if (cc != null && cc.isGrounded && Input.GetKeyDown(KeyCode.Space))
            {velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            animator.SetTrigger("Jumping");}

         velocity.y += gravity * Time.deltaTime;
}
void aiming ()
{
    if (Input.GetMouseButton(1) || Input.GetMouseButton(0))
    {
        animator.SetLayerWeight(1, 1f);
    }
    else
    {
        animator.SetLayerWeight(1, 0f);
    }
}

}
