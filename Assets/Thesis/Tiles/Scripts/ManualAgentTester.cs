using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ManualAgentController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 20f;
    public float rotationSpeed = 120f;
    public float gravityMultiplier = 2.5f;

    private Rigidbody rb;
    private bool isGrounded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        if (!isGrounded && rb.linearVelocity.y < 0f)
        {
            rb.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);
        }
    }

    void Update()
    {
        float rotationInput = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.up * rotationInput * rotationSpeed * Time.deltaTime);


        float moveInput = Input.GetAxis("Vertical");
        Vector3 moveDir = transform.forward * moveInput;
        Vector3 newPos = rb.position + moveDir * moveSpeed * Time.deltaTime;
        rb.MovePosition(newPos);

        if (Input.GetKeyDown(KeyCode.E) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("ground"))
        {
            isGrounded = true;
        }
    }
}
