using UnityEngine;

public class RandomWalkPlayer : MonoBehaviour
{
    private Rigidbody rb;
    public float moveSpeed = 20f;
    private Vector3 moveDirection;
    private float changeDirectionTime = 2f;
    private float timer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        PickRandomDirection();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= changeDirectionTime)
        {
            PickRandomDirection();
            timer = 0f;
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveDirection.normalized * moveSpeed;
    }

    private void PickRandomDirection()
    {
        float randomX = Random.Range(-1f, 1f);
        float randomZ = Random.Range(-1f, 1f);
        moveDirection = new Vector3(randomX, 0f, randomZ);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            PickRandomDirection();
        }
    }
}
