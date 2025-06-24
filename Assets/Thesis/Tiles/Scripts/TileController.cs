using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TileController : MonoBehaviour
{
    public float fadeDuration = 1f;
    public float respawnDelay = 2f;

    public Material greenMaterial;
    public Material yellowMaterial;
    public Material orangeMaterial;
    public Material redMaterial;

    private float currentTime;
    private float fadeTimer;
    private bool fading = false;

    private Renderer tileRenderer;
    private Collider tileCollider;
    private TextMeshPro textMeshPro;

    private static List<float> timerPool = new List<float>();
    private static int poolIndex = 0;
    public int tileID;

    private void Awake()
    {
        tileRenderer = GetComponent<Renderer>();
        tileCollider = GetComponent<Collider>();
        textMeshPro = GetComponentInChildren<TextMeshPro>();
    }

    private void Start()
    {
        ResetTile();
    }

    private void Update()
    {
        if (fading) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            StartFadeOut();
        }
        else
        {
            UpdateMaterial();
            textMeshPro.text = Mathf.FloorToInt(currentTime).ToString();
        }
    }

    private void StartFadeOut()
    {
        fading = true;
        StartCoroutine(FadeAndRespawn());
    }

    private System.Collections.IEnumerator FadeAndRespawn()
    {
        float elapsed = 0f;
        Color startColor = tileRenderer.material.color;

        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            float alpha = Mathf.Lerp(1f, 0f, t);

            Color newColor = tileRenderer.material.color;
            newColor.a = alpha;
            tileRenderer.material.color = newColor;

            Color textColor = textMeshPro.color;
            textColor.a = alpha;
            textMeshPro.color = textColor;

            elapsed += Time.deltaTime;
            yield return null;
        }

        tileRenderer.enabled = false;
        tileCollider.enabled = false;

        yield return new WaitForSeconds(respawnDelay);

        ResetTile();
    }

    private void ResetTile()
    {
        currentTime = GetNextUniqueTimer();
        fading = false;

        tileRenderer.enabled = true;
        tileCollider.enabled = true;
        tileRenderer.material.color = Color.white;
        tileRenderer.material = greenMaterial;

        textMeshPro.text = Mathf.CeilToInt(currentTime).ToString();
        textMeshPro.alpha = 1f;
    }

    private float GetNextUniqueTimer()
{
    if (timerPool.Count == 0 || poolIndex >= timerPool.Count)
    {
        timerPool = new List<float>();


        timerPool.AddRange(Repeat(2f, 2));
        timerPool.AddRange(Repeat(3f, 3));
        timerPool.AddRange(Repeat(4f, 4));
        timerPool.AddRange(Repeat(5f, 6));
        timerPool.AddRange(Repeat(6f, 8));
        timerPool.AddRange(Repeat(7f, 8));
        timerPool.AddRange(Repeat(8f, 6));
        timerPool.AddRange(Repeat(9f, 3));

        Shuffle(timerPool);
        poolIndex = 0;
    }

    return timerPool[poolIndex++];
}

private List<float> Repeat(float value, int count)
{
    var list = new List<float>();
    for (int i = 0; i < count; i++) list.Add(value);
    return list;
}


    private void Shuffle(List<float> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            float temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    private void UpdateMaterial()
    {
        if (currentTime > 6f)
            tileRenderer.material = greenMaterial;
        else if (currentTime > 4f)
            tileRenderer.material = yellowMaterial;
        else if (currentTime > 2f)
            tileRenderer.material = orangeMaterial;
        else
            tileRenderer.material = redMaterial;
    }

    public void ResetImmediate()
    {
        currentTime = GetNextUniqueTimer();
        fading = false;
        fadeTimer = 0f;

        tileRenderer.enabled = true;
        tileCollider.enabled = true;

        UpdateMaterial();

        textMeshPro.alpha = 1f;
        textMeshPro.text = Mathf.FloorToInt(currentTime).ToString();
    }
}
