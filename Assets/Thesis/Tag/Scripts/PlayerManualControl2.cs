using UnityEngine;
using System.IO;

public class PlayerManualControl2 : MonoBehaviour
{
    public float moveSpeed = 5f;
    public TagEnvironment2 environment;

    private string logDirectory = "";
    private string logPath;

    private void Start()
    {
        logPath = Path.Combine(logDirectory, "player.csv");

        if (!File.Exists(logPath))
            File.WriteAllText(logPath, "Iteration,X,Z,YRotation,Frame\n");
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(moveX, 0, moveZ).normalized;
        transform.position += move * moveSpeed * Time.deltaTime;

        LogPlayerPosition();
    }

    private void LogPlayerPosition()
    {
        if (environment == null) return;

        int episode = environment.GetCurrentEpisode();
        int frame = environment.GetFrameCount();
        float yRotation = transform.rotation.eulerAngles.y;

        string line = $"{episode},{transform.localPosition.x:F2},{transform.localPosition.z:F2},{yRotation:F1},{frame}";
        File.AppendAllText(logPath, line + "\n");
    }
}
