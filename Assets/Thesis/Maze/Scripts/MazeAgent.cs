using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.IO;

public class MazeAgent : Agent
{
    public float moveSpeed = 5f;
    public float turnSpeed = 100f;

    private Rigidbody rb;
    private Vector3 startPos;
    private Quaternion startRot;

    private float episodeTimer = 0f;
    private const float maxEpisodeTime = 30f;

    private StreamWriter logWriter;
    private StreamWriter goalWriter;
    private StreamWriter rewardWriter;

    private float episodeStartTime;
    private float cumulativeReward = 0f;

    private HashSet<Transform> visitedCheckpoints;
    private HashSet<Vector2Int> visitedTiles = new HashSet<Vector2Int>();

    private int currentEpisode = 0;
    private int episodeFrame = 0;
    private bool isTouchingWall = false;
    private float wallContactTime = 0f;



    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
        startRot = transform.rotation;

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        string agentID = GetInstanceID().ToString();
        string folder = Application.dataPath + "/Logs";
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        logWriter = new StreamWriter(Path.Combine(folder, $"Agent_{agentID}.csv"), true);
        logWriter.WriteLine("iteration,x,z,rotY,frame");

        goalWriter = new StreamWriter(Path.Combine(folder, $"GoalStats_{agentID}.csv"), true);
        goalWriter.WriteLine("iteration,goalReached,timeToGoal");

        rewardWriter = new StreamWriter(Path.Combine(folder, $"RewardStats_{agentID}.csv"), true);
        rewardWriter.WriteLine("iteration,totalReward");
    }

    void Update()
    {
        if (CountdownTimer.Instance != null && CountdownTimer.Instance.GetRemainingTime() <= 0)
        {
            AddAndTrackReward(-0.5f);
            goalWriter.WriteLine($"{currentEpisode},0,0");
            rewardWriter.WriteLine($"{currentEpisode},{cumulativeReward:F2}");
            EndEpisode();
        }
    }

    public override void OnEpisodeBegin()
    {
        ResetManager.Instance?.IncrementReset();
        CountdownTimer.Instance?.ResetTimer();

        episodeTimer = 0f;
        cumulativeReward = 0f;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = startPos;
        transform.rotation = startRot;

        episodeStartTime = Time.time;

        visitedCheckpoints = new HashSet<Transform>();
        visitedTiles.Clear();
        currentEpisode++;
        episodeFrame = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position.x);
        sensor.AddObservation(transform.position.z);
        sensor.AddObservation(transform.forward);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int moveAction = actions.DiscreteActions[0];
        int turnAction = actions.DiscreteActions[1];

        Vector3 moveDir = Vector3.zero;
        if (moveAction == 1) moveDir = transform.forward;
        else if (moveAction == 2) moveDir = -transform.forward;

        rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);

        if (turnAction == 1)
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0, -turnSpeed * Time.fixedDeltaTime, 0));
        else if (turnAction == 2)
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0, turnSpeed * Time.fixedDeltaTime, 0));

        Vector3 pos = transform.position;
        float rotY = transform.rotation.eulerAngles.y;
        logWriter.WriteLine($"{currentEpisode},{pos.x:F2},{pos.z:F2},{rotY:F2},{episodeFrame}");
        episodeFrame++;

        Vector2Int currentTile = new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
        if (!visitedTiles.Contains(currentTile))
        {
            visitedTiles.Add(currentTile);
            AddAndTrackReward(0.05f);
        }
    }

    private void AddAndTrackReward(float reward)
    {
        AddReward(reward);
        cumulativeReward += reward;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            isTouchingWall = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            isTouchingWall = false;
            wallContactTime = 0f;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal"))
        {
            float timeToGoal = Time.time - episodeStartTime;
            AddAndTrackReward(10f);
            GoalCounter.Instance?.IncrementGoal();
            goalWriter.WriteLine($"{currentEpisode},1,{timeToGoal:F2}");
            rewardWriter.WriteLine($"{currentEpisode},{cumulativeReward:F2}");
            EndEpisode();
        }
    }

    void FixedUpdate()
    {
        episodeTimer += Time.fixedDeltaTime;
        if (episodeTimer >= maxEpisodeTime)
        {
            goalWriter.WriteLine($"{currentEpisode},0,0");
            rewardWriter.WriteLine($"{currentEpisode},{cumulativeReward:F2}");
            EndEpisode();
        }
        if (isTouchingWall)
        {
            wallContactTime += Time.fixedDeltaTime;
            if (wallContactTime >= 1.0f)
            {
                AddAndTrackReward(-0.1f);
                wallContactTime = 0f;
            }
        }

    }

    void OnDestroy()
    {
        logWriter?.Flush(); logWriter?.Close();
        goalWriter?.Flush(); goalWriter?.Close();
        rewardWriter?.Flush(); rewardWriter?.Close();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? 2 : 0;
        discreteActions[1] = Input.GetKey(KeyCode.A) ? 1 : Input.GetKey(KeyCode.D) ? 2 : 0;
    }
}
