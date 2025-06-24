using UnityEngine;
using TMPro;

public class ResetManager : MonoBehaviour
{
    public static ResetManager Instance;

    public TextMeshProUGUI resetText;
    public int resetCount = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void IncrementReset()
    {
        resetCount++;
        UpdateText();
    }

    private void UpdateText()
    {
        if (resetText != null)
            resetText.text = resetCount.ToString();
    }
    
}
