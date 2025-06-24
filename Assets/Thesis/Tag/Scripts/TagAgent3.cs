using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using System.IO;

public class TagAgent3 : Agent
{
    public Transform player;
    public float moveSpeed = 20f;
    public float turnSpeed = 200f;
    public float maxEpisodeTime = 30f;
    public string agentId = "Agent1";

    private float elapsedTime = 0f;
    private Vector3 agentStartPos;
    private Quaternion agentStartRot;

    private TagEnvironment2 environment;
    private Rigidbody rb;

    private int lastIteration = -1;
    private int frameCounter = 1;

    private string movementLogPath;
    private string logDirectory = "";

    public float repulsionRadius = 15f;
    public Color gizmoColor = Color.red;

    public void SetEnvironment(TagEnvironment2 env)
    {
        environment = env;
    }

    public override void Initialize()
    {
        agentStartPos = transform.localPosition;
        agentStartRot = transform.rotation;
        rb = GetComponent<Rigidbody>();

        Directory.CreateDirectory(logDirectory);
        movementLogPath = Path.Combine(logDirectory, $"agent_{agentId}.csv");

        if (!File.Exists(movementLogPath))
        {
            File.WriteAllText(movementLogPath, "Iteration,X,Z,YRotation,Frame\n");
        }
    }

    public override void OnEpisodeBegin()
    {
    }

    public void ResetAgent()
    {
        transform.localPosition = agentStartPos;
        transform.rotation = agentStartRot;
        elapsedTime = 0f;
        EndEpisode();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.z);
        sensor.AddObservation(transform.forward.x);
        sensor.AddObservation(transform.forward.z);

        Vector3 dirToPlayer = (player.localPosition - transform.localPosition).normalized;
        sensor.AddObservation(dirToPlayer.x);
        sensor.AddObservation(dirToPlayer.z);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int move = actions.DiscreteActions[0];
        int turn = actions.DiscreteActions[1];

        Vector3 direction = Vector3.zero;
        if (move == 1) direction = GetChaseWithRepulsion();
        else if (move == 2) direction = -transform.forward;

        if (rb != null)
        {
            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            transform.position += direction * moveSpeed * Time.fixedDeltaTime;
        }

        if (turn == 1) transform.Rotate(Vector3.up, -turnSpeed * Time.fixedDeltaTime);
        else if (turn == 2) transform.Rotate(Vector3.up, turnSpeed * Time.fixedDeltaTime);

        float maxDist = 200f;
        float currentDist = Vector3.Distance(transform.localPosition, player.localPosition);
        float proximityReward = Mathf.Clamp01((maxDist - currentDist) / maxDist);
        AddReward(proximityReward * 0.012f);

        elapsedTime += Time.fixedDeltaTime;
        if (elapsedTime >= maxEpisodeTime)
        {
            AddReward(-3.0f);
            environment.ForceTimeoutReset();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            AddReward(10.0f);
            environment.ReportCatch(agentId);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var da = actionsOut.DiscreteActions;
        da[0] = 0;
        da[1] = 0;

        if (Input.GetKey(KeyCode.W)) da[0] = 1;
        if (Input.GetKey(KeyCode.S)) da[0] = 2;
        if (Input.GetKey(KeyCode.A)) da[1] = 1;
        if (Input.GetKey(KeyCode.D)) da[1] = 2;
    }

    private void FixedUpdate()
    {
        if (environment == null) return;

        int currentIteration = environment.GetCurrentEpisode();
        if (currentIteration != lastIteration)
        {
            frameCounter = 1;
            lastIteration = currentIteration;
        }

        LogMovement(currentIteration);
        frameCounter++;
    }

    private void LogMovement(int iteration)
    {
        float x = transform.localPosition.x;
        float z = transform.localPosition.z;
        float yRot = transform.rotation.eulerAngles.y;

        string line = $"{iteration},{x:F2},{z:F2},{yRot:F1},{frameCounter}";
        File.AppendAllText(movementLogPath, line + "\n");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, repulsionRadius);
    }

    private Vector3 GetChaseWithRepulsion()
    {
        Vector3 toPlayer = (player.localPosition - transform.localPosition).normalized;
        Vector3 repulsion = Vector3.zero;

        foreach (var other in environment.agents)
        {
            if (other == this) continue;

            Vector3 offset = transform.localPosition - other.transform.localPosition;
            float distance = offset.magnitude;

            if (distance < repulsionRadius && distance > 0.01f)
            {
                float strength = (repulsionRadius - distance) / repulsionRadius;
                repulsion += offset.normalized * strength;
            }
        }

        return (toPlayer + repulsion * 1.5f).normalized;
    }

}

