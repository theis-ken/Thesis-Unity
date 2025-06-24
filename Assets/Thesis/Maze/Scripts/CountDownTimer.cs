using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    public static CountdownTimer Instance;

    public TextMeshProUGUI countdownText;
    public float countdownDuration = 30f;

    private float currentTime;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        ResetTimer();
    }

    void Update()
    {
        currentTime -= Time.deltaTime;

        if (countdownText != null)
            countdownText.text = Mathf.Ceil(currentTime).ToString();

        if (currentTime <= 0)
        {
            countdownText.text = "0";
        }
    }

    public void ResetTimer()
    {
        currentTime = countdownDuration;
    }

    public float GetRemainingTime()
    {
        return currentTime;
    }
}
