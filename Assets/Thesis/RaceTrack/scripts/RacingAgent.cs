using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.IO;

public class RacingAgent : Agent
{
    [Header("Movement Settings")]
    public float acceleration = 50f;
    public float turnSpeed = 150f;

    private Rigidbody rb;
    private Vector3 startPos;
    private Quaternion startRot;

    private HashSet<int> visitedCheckpointIDs = new HashSet<int>();
    private int lastCheckpointIndex = -1;

    private int wallCollisionCount = 0;
    private int agentCollisionCount = 0;
    private int checkpointCount = 0;

    private int finalFinishOrder = 0;
    private float finalLapTime = 0f;

    private bool hasCrossedStartGoal = false;
    private bool isOffTrack = false;
    private const int totalCheckpoints = 12;

    private StreamWriter logWriter;
    private StreamWriter statWriter;
    private int currentIteration = 0;
    private int frameCounter = 0;
    private float episodeTimer = 0f;
    private const float maxEpisodeTime = 30f;
    private float episodeStartTime;

    public static int finishOrder = 0;
    private static int episodeCounter = 0;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
        startRot = transform.rotation;

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        string agentID = GetInstanceID().ToString();
        string folder = Application.dataPath + "/Thesis/RaceTrack/Logs";
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        logWriter = new StreamWriter(Path.Combine(folder, $"RacingAgent_{agentID}.csv"), true);
        logWriter.WriteLine("iteration,x,y,z,rotY,rotX,rotZ,frame");

        statWriter = new StreamWriter(Path.Combine(folder, $"RacingStats_{agentID}.csv"), true);
        statWriter.WriteLine("iteration,wallCollisions,agentCollisions,checkpointsReached,finishOrder,lapTime");
    }

    void Update()
    {
        episodeTimer += Time.deltaTime;
        if (episodeTimer >= maxEpisodeTime)
        {
            SaveStats();
            EndEpisode();
        }

        if (isOffTrack)
        {
            AddReward(-0.4f);
        }
    }

    public override void OnEpisodeBegin()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = startPos;
        transform.rotation = startRot;

        visitedCheckpointIDs.Clear();
        lastCheckpointIndex = -1;
        checkpointCount = 0;
        wallCollisionCount = 0;
        agentCollisionCount = 0;
        finalFinishOrder = 0;
        finalLapTime = 0f;
        frameCounter = 0;
        episodeTimer = 0f;
        currentIteration++;
        episodeStartTime = Time.time;
        hasCrossedStartGoal = false;
        isOffTrack = false;

        ScoreManager.Instance?.IncrementReset();
        CountDownTimerRace.Instance?.ResetTimer();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.InverseTransformDirection(rb.linearVelocity));
        sensor.AddObservation(transform.forward);
        sensor.AddObservation(transform.right);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int moveAction = actions.DiscreteActions[0];
        int turnAction = actions.DiscreteActions[1];

        if (moveAction == 1)
            rb.AddForce(transform.forward * acceleration, ForceMode.Force);
        else if (moveAction == 2)
            rb.AddForce(-transform.forward * acceleration, ForceMode.Force);

        if (turnAction == 1)
            transform.Rotate(Vector3.up, -turnSpeed * Time.fixedDeltaTime);
        else if (turnAction == 2)
            transform.Rotate(Vector3.up, turnSpeed * Time.fixedDeltaTime);

        logWriter.WriteLine($"{currentIteration},{transform.position.x:F2},{transform.position.y:F2},{transform.position.z:F2}," +
                            $"{transform.eulerAngles.y:F2},{transform.eulerAngles.x:F2},{transform.eulerAngles.z:F2},{frameCounter}");
        frameCounter++;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discrete = actionsOut.DiscreteActions;
        discrete[0] = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? 2 : 0;
        discrete[1] = Input.GetKey(KeyCode.A) ? 1 : Input.GetKey(KeyCode.D) ? 2 : 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CheckPoint"))
        {
            Checkpoint checkpoint = other.GetComponent<Checkpoint>();
            if (checkpoint != null)
            {
                int currentIndex = checkpoint.checkpointIndex;
                if (currentIndex == lastCheckpointIndex + 1)
                {
                    visitedCheckpointIDs.Add(currentIndex);
                    lastCheckpointIndex = currentIndex;
                    checkpointCount++;
                    AddReward(2f);
                }
                else
                {
                    AddReward(-2f);
                }
            }
        }
        else if (other.CompareTag("Wall"))
        {
            AddReward(-0.2f);
            wallCollisionCount++;
        }
        else if (other.CompareTag("Goal"))
        {
            if (!hasCrossedStartGoal)
            {
                AddReward(1f);
                hasCrossedStartGoal = true;
            }
            else if (visitedCheckpointIDs.Count >= totalCheckpoints)
            {
                AddReward(15f);
                finishOrder++;

                finalFinishOrder = finishOrder;
                finalLapTime = Time.time - episodeStartTime;

                visitedCheckpointIDs.Clear();
                lastCheckpointIndex = -1;
                checkpointCount = 0;
                hasCrossedStartGoal = false;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Agent"))
        {
            AddReward(-1f);
            agentCollisionCount++;
        }
        else if (collision.collider.CompareTag("OffTrack"))
        {
            AddReward(-1.0f);
            isOffTrack = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("OffTrack"))
        {
            isOffTrack = false;
        }
    }

    private void SaveStats()
    {
        statWriter.WriteLine($"{currentIteration},{wallCollisionCount},{agentCollisionCount},{checkpointCount},{finalFinishOrder},{finalLapTime:F2}");

        string rewardPath = Application.dataPath + "/Thesis/RaceTrack/Logs/racing_rewards.csv";
        if (!File.Exists(rewardPath))
        {
            File.WriteAllText(rewardPath, "Iteration,TotalReward\n");
        }
        File.AppendAllText(rewardPath, $"{currentIteration},{GetCumulativeReward():F4}\n");
    }

    void OnDestroy()
    {
        logWriter?.Flush();
        logWriter?.Close();
        statWriter?.Flush();
        statWriter?.Close();
    }
}
