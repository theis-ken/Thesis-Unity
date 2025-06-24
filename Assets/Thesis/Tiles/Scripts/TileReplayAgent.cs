using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class TileAgentReplayer_OneIteration : MonoBehaviour
{
    [Header("Replay Settings")]
    public int agentId = 0;
    public int iterationToReplay = 1;
    public string basePath = " ";
    public string filePrefix = " ";
    public float playbackFPS = 25f;

    private class Frame
    {
        public Vector3 pos;
        public float rotY;
    }

    private List<Frame> frames = new List<Frame>();
    private int currentFrame = 0;
    private float timer = 0f;
    private float frameDuration;

    void Start()
    {
        frameDuration = 1f / playbackFPS;
        LoadFramesForIteration();
    }

    void LoadFramesForIteration()
    {
        string filePath = Path.Combine(basePath, $"{filePrefix}{agentId}.csv");

        if (!File.Exists(filePath))
        {
            return;
        }

        using (var reader = new StreamReader(filePath))
        {
            string header = reader.ReadLine(); 
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',');

                int iteration = int.Parse(parts[0], CultureInfo.InvariantCulture);
                if (iteration != iterationToReplay) continue;

                float x = float.Parse(parts[1], CultureInfo.InvariantCulture);
                float y = float.Parse(parts[2], CultureInfo.InvariantCulture);
                float z = float.Parse(parts[3], CultureInfo.InvariantCulture);
                float rotY = float.Parse(parts[4], CultureInfo.InvariantCulture);

                frames.Add(new Frame
                {
                    pos = new Vector3(x, y, z),
                    rotY = rotY
                });
            }
        }
    }

    void Update()
    {
        if (currentFrame >= frames.Count) return;

        timer += Time.deltaTime;

        while (timer >= frameDuration && currentFrame < frames.Count)
        {
            Frame f = frames[currentFrame];
            transform.position = f.pos;
            transform.rotation = Quaternion.Euler(0, f.rotY, 0);

            currentFrame++;
            timer -= frameDuration;
        }
    }
}
