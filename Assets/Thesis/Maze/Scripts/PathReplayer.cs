using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;

public class PathReplayer : MonoBehaviour
{
    public string csvPath = "";
    public int iterationToPlay = 1;
    public float playbackFPS = 25f; 
    public bool useYOffset = true;

    public float manualYStart = 0f;

    private struct FrameData
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    private List<FrameData> replayFrames = new List<FrameData>();
    private int currentFrame = 0;
    private float timer = 0f;
    private float frameDuration;

    void Start()
    {
        frameDuration = 1f / playbackFPS;
        LoadCSV(csvPath);
    }

    void LoadCSV(string path)
    {

        using (var reader = new StreamReader(path))
        {
            reader.ReadLine(); 

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] values = line.Split(',');

                if (values.Length < 5)
                {
                    continue;
                }

                int iteration = int.Parse(values[0]);
                if (iteration != iterationToPlay) continue;

                float x = float.Parse(values[1], CultureInfo.InvariantCulture);
                float z = float.Parse(values[2], CultureInfo.InvariantCulture);
                float rotY = float.Parse(values[3], CultureInfo.InvariantCulture);

                replayFrames.Add(new FrameData
                {
                    position = new Vector3(x, manualYStart, z),
                    rotation = Quaternion.Euler(0f, rotY, 0f)
                });
            }
        }
    }

    void Update()
    {
        if (replayFrames.Count == 0 || currentFrame >= replayFrames.Count)
            return;

        timer += Time.deltaTime;

        while (timer >= frameDuration && currentFrame < replayFrames.Count)
        {
            transform.position = replayFrames[currentFrame].position;
            transform.rotation = replayFrames[currentFrame].rotation;
            currentFrame++;
            timer -= frameDuration;
        }
    }
}
