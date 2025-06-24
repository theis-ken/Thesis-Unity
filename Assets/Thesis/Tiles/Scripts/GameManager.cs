using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float episodeDuration = 60f;
    private float timer = 0f;

    private int fallenAgents = 0;
    public int totalAgents = 8;

    public float GetTimer() => timer;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= episodeDuration)
        {
            EndAllAgents();
            timer = 0f;
        }
    }

    public void AgentFell()
    {
        fallenAgents++;
        if (fallenAgents >= totalAgents)
        {
            EndAllAgents();
        }
    }

    public void EndAllAgents()
    {
        foreach (SurvivalAgent agent in FindObjectsOfType<SurvivalAgent>())
        {
            agent.LogTimeoutAndEnd();
        }

        foreach (TileController tile in FindObjectsOfType<TileController>())
        {
            tile.ResetImmediate();
        }

        FindObjectOfType<SurvivalLogger>().IncrementIteration();
        fallenAgents = 0;
        timer = 0f;
    }
}
