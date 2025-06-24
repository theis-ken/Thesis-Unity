using System.IO;
using UnityEngine;
using Unity.MLAgents; 

public class TagEnvironment2 : MonoBehaviour
{
    public Agent[] agents; 
    public Transform player;

    private Vector3[] agentStartPositions;
    private Vector3 playerStartPosition;

    private int episodeCounter = 1; 
    private int frameCounter = 0;
    private bool caughtThisRound = false;
    private string catcherId = "None";

    private float episodeTimer = 0f;
    private float maxEpisodeTime = 30f;

    private string logFilePath = "";

    void Start()
    {
        playerStartPosition = player.localPosition;
        agentStartPositions = new Vector3[agents.Length];

        for (int i = 0; i < agents.Length; i++)
        {
            agentStartPositions[i] = agents[i].transform.localPosition;

            if (agents[i] is TagAgent3 agent3)
            {
                agent3.SetEnvironment(this);
            }
            else if (agents[i] is TagAgentDistanceOnly agentSimple)
            {
                agentSimple.SetEnvironment(this);
            }
        }

        if (!File.Exists(logFilePath))
        {
            File.WriteAllText(logFilePath, "Iteration,PlayerCaught,Catcher\n");
        }
    }

    void Update()
    {
        episodeTimer += Time.deltaTime;
        frameCounter++;

        if (episodeTimer >= maxEpisodeTime)
        {
            ForceTimeoutReset();
        }
    }

    public int GetCurrentEpisode() => episodeCounter;

    public int GetFrameCount() => frameCounter;

    public void ReportCatch(string agentName)
    {
        if (!caughtThisRound)
        {
            caughtThisRound = true;
            catcherId = agentName;
            ForceTimeoutReset();
        }
    }

    public void ForceTimeoutReset()
    {
        string logLine = $"{episodeCounter},{(caughtThisRound ? 1 : 0)},{catcherId}";
        try
        {
            File.AppendAllText(logFilePath, logLine + "\n");
        }
        catch (IOException e)
        {
        }

        player.localPosition = playerStartPosition + new Vector3(
            Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));

        for (int i = 0; i < agents.Length; i++)
        {
            if (agents[i] is TagAgent3 agent3)
            {
                agent3.ResetAgent();
            }
            else if (agents[i] is TagAgentDistanceOnly agentSimple)
            {
                agentSimple.ResetAgent();
            }
        }

        episodeCounter++;
        frameCounter = 0;
        episodeTimer = 0f;
        caughtThisRound = false;
        catcherId = "None";
    }
}
