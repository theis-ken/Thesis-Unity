using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TagReplayAgent : MonoBehaviour
{
    public string csvFilePath;
    public int targetIteration = 1;
    public float frameRate = 25f;

    private List<FrameData> replayFrames = new List<FrameData>();
    private int currentFrameIndex = 0;
    private float frameTimer = 0f;
    private bool replaying = false;

    private class FrameData
    {
        public int iteration;
        public float x;
        public float z;
        public float yRot;
        public int frame;
    }

    void Start()
    {
        LoadCSV();
        if (replayFrames.Count > 1)
        {
            replaying = true;
            ApplyExactFrame(0); 
        }
    }

    void Update()
    {
        if (!replaying || replayFrames.Count < 2) return;

        frameTimer += Time.deltaTime;

        float frameDuration = 1f / frameRate;

        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            currentFrameIndex++;

            if (currentFrameIndex >= replayFrames.Count - 1)
            {
                replaying = false;
                ApplyExactFrame(replayFrames.Count - 1);
                Debug.Log($"Replay finished for {gameObject.name}");
                return;
            }
        }

        FrameData current = replayFrames[currentFrameIndex];
        FrameData next = replayFrames[currentFrameIndex + 1];

        float t = frameTimer / frameDuration;

        float x = Mathf.Lerp(current.x, next.x, t);
        float z = Mathf.Lerp(current.z, next.z, t);
        float yRot = Mathf.LerpAngle(current.yRot, next.yRot, t);

        transform.localPosition = new Vector3(x, transform.localPosition.y, z);
        transform.rotation = Quaternion.Euler(0, yRot, 0);
    }

    void LoadCSV()
    {
        replayFrames.Clear();

        if (!File.Exists(csvFilePath))
        {
            return;
        }

        using (StreamReader reader = new StreamReader(csvFilePath))
        {
            string header = reader.ReadLine(); 
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',');
                if (parts.Length < 5) continue; 
                int iter = int.Parse(parts[0]);
                if (iter != targetIteration) continue;

                FrameData frame = new FrameData()
                {
                    iteration = iter,
                    x = float.Parse(parts[1]),
                    z = float.Parse(parts[2]),
                    yRot = float.Parse(parts[3]),
                    frame = int.Parse(parts[4])
                };
                replayFrames.Add(frame);
            }
        }

    }

    void ApplyExactFrame(int index)
    {
        var frame = replayFrames[index];
        transform.localPosition = new Vector3(frame.x, transform.localPosition.y, frame.z);
        transform.rotation = Quaternion.Euler(0, frame.yRot, 0);
    }
}
