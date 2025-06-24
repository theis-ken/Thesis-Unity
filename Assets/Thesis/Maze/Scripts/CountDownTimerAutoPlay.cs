using UnityEngine;
using TMPro; 

public class CountDownTimerAutoPlay : MonoBehaviour
{
    public float startTime = 30f;
    private float currentTime;

    public static CountDownTimerAutoPlay Instance;

    [SerializeField] private TextMeshProUGUI timerText; 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void OnEnable()
    {
        ResetTimer();
    }

    void Update()
    {
        currentTime -= Time.deltaTime;
        currentTime = Mathf.Max(currentTime, 0f);

        if (timerText != null)
        {
            timerText.text = currentTime.ToString("F1"); 
        }
    }

    public float GetRemainingTime()
    {
        return currentTime;
    }

    public void ResetTimer()
    {
        currentTime = startTime;
    }
}
