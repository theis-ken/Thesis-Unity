using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public TextMeshProUGUI collisionText;

    private int resetCount = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateCollisionText();
    }

    public void IncrementReset()
    {
        resetCount++;
        UpdateCollisionText();
    }

    private void UpdateCollisionText()
    {
        if (collisionText != null)
        {
            collisionText.text = resetCount.ToString();
        }
    }
}
