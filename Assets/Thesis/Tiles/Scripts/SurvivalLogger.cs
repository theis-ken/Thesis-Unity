using System;
using System.IO;
using UnityEngine;

public class SurvivalLogger : MonoBehaviour
{
    private string episodeLogPath;
    private string frameLogPath;
    private int iteration = 1;

    private void Awake()
    {
        string logDir = Path.Combine(Application.dataPath, "Thesis/Tiles/Logs/");
        episodeLogPath = Path.Combine(logDir, "SurvivalEpisodeLog.csv");
        frameLogPath = Path.Combine(logDir, "SurvivalFrameLog.csv");

        if (!File.Exists(episodeLogPath))
        {
            File.AppendAllText(episodeLogPath, "iteration,agent_id,time_alive,pushes,end_reason,x,y,z,y_rotation,reward\n");
        }

        if (!File.Exists(frameLogPath))
        {
            File.AppendAllText(frameLogPath, "iteration,agent_id,x,y,z,y_rotation\n");
        }
    }

    public void LogFrame(int agentId, Vector3 position, float yRotation)
    {
        string line = $"{iteration},{agentId},{position.x:F2},{position.y:F2},{position.z:F2},{yRotation:F2}";
        File.AppendAllText(frameLogPath, line + "\n");
    }

    public void LogEpisode(int agentId, float timeAlive, int pushes, string endReason, Vector3 position, float rotationY, float reward)
    {
        string line = $"{iteration},{agentId},{timeAlive:F2},{pushes},{endReason},{position.x:F2},{position.y:F2},{position.z:F2},{rotationY:F2},{reward:F2}";
        File.AppendAllText(episodeLogPath, line + Environment.NewLine);
    }

    public void IncrementIteration()
    {
        iteration++;
    }

    public int GetCurrentIteration()
    {
        return iteration;
    }
}
