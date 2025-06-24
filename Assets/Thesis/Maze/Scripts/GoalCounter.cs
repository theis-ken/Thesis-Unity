using UnityEngine;
using TMPro;

public class GoalCounter : MonoBehaviour
{
    public static GoalCounter Instance;

    public TextMeshProUGUI goalText;
    private int goalCount = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateText();
    }

    public void IncrementGoal()
    {
        goalCount++;
        UpdateText();
    }

    private void UpdateText()
    {
        if (goalText != null)
        {
            goalText.text = goalCount.ToString();
        }
    }
}
