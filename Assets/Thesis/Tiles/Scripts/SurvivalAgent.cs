using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

[RequireComponent(typeof(Rigidbody))]
public class SurvivalAgent : Agent
{
    [Header("Movement Settings")]
    public float moveForce = 8f;
    public float jumpForce = 20f;
    public float rotationSpeed = 120f;
    public float gravityMultiplier = 4f;

    [Header("Interaction Settings")]
    public float interactionRange = 1.5f;
    public float pushForce = 10f;

    private Rigidbody rb;
    private Vector3 spawnPosition;
    private bool isGrounded = false;

    private SurvivalLogger logger;
    private float episodeTimer = 0f;
    private int pushCount = 0;
    private int agentId;
    private static int globalId = 0;
    private bool hasFallen = false;

    private int currentTileID = -1;
    private int previousTileID = -1;
    private bool hasJumped = false;
private bool hasLoggedEpisode = false;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        spawnPosition = transform.position;
        logger = FindObjectOfType<SurvivalLogger>();
        agentId = globalId++;
    }

    public override void OnEpisodeBegin()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = spawnPosition + new Vector3(
            Random.Range(-0.5f, 0.5f),
            0,
            Random.Range(-0.5f, 0.5f)
        );
        hasFallen = false;
        isGrounded = false;
        episodeTimer = 0f;
        pushCount = 0;
         hasLoggedEpisode = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(rb.transform.position);
        sensor.AddObservation(rb.transform.forward);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float rotationInput = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float moveInput = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        float jump = actions.ContinuousActions[2];
        float interaction = actions.ContinuousActions[3];

        transform.Rotate(Vector3.up * rotationInput * rotationSpeed * Time.deltaTime);
        Vector3 moveDir = transform.forward * moveInput;
        rb.MovePosition(rb.position + moveDir * moveForce * Time.deltaTime);

        if (jump > 0.5f && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
            hasJumped = true;
        }

        if (interaction >= 1.5f)
        {
            TryPush();
        }

        logger.LogFrame(agentId, transform.position, transform.eulerAngles.y);

        if (!hasFallen)
        {
            AddReward(0.015f);
            episodeTimer += Time.deltaTime;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var c = actionsOut.ContinuousActions;
        c[0] = Input.GetKey(KeyCode.A) ? -1f : Input.GetKey(KeyCode.D) ? 1f : 0f;
        c[1] = Input.GetKey(KeyCode.W) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0f;
        c[2] = Input.GetKey(KeyCode.E) ? 1f : 0f;
        c[3] = Input.GetKey(KeyCode.LeftControl) ? 2f : 0f;
    }

    private void TryPush()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRange);
        foreach (var hit in hits)
        {
            if (hit.gameObject != gameObject && hit.CompareTag("agent"))
            {
                var other = hit.GetComponent<Rigidbody>();
                if (other)
                {
                    Vector3 dir = (hit.transform.position - transform.position).normalized;
                    other.AddForce(dir * pushForce, ForceMode.Impulse);
                    pushCount++;
                    AddReward(0.2f); 
                }
                break;
            }
        }
    }

    private void FixedUpdate()
    {
        CheckGrounded();

        if (!isGrounded && rb.linearVelocity.y < 0f)
        {
            rb.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);
        }
    }

    private void CheckGrounded()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
        isGrounded = Physics.Raycast(ray, 1.2f, LayerMask.GetMask("Ground"));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("OutOfBounds"))
        {
            if (!hasFallen && !hasLoggedEpisode)
            {
                 hasFallen = true;
                AddReward(-5f);
                hasLoggedEpisode = true;
                logger.LogEpisode(agentId, episodeTimer, pushCount, "fall", transform.position, transform.eulerAngles.y, GetCumulativeReward());
                GameManager.Instance.AgentFell();
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("ground"))
        {
            isGrounded = true;

            TileController tile = collision.gameObject.GetComponent<TileController>();
            if (tile != null)
            {
                currentTileID = tile.tileID;

                if (hasJumped && currentTileID != previousTileID)
                {
                    //AddReward(0.2f);
                }

                previousTileID = currentTileID;
                hasJumped = false;
            }
        }
    }

public void LogTimeoutAndEnd()
    {
        if (!hasLoggedEpisode)
        {
            AddReward(3.0f);
            hasLoggedEpisode = true;
            logger.LogEpisode(agentId, episodeTimer, pushCount, "timeout", transform.position, transform.eulerAngles.y, GetCumulativeReward());
        }
        EndEpisode();
    }
}
