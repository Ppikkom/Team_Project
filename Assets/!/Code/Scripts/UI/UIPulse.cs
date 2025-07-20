using UnityEngine;

public class UIPulse : MonoBehaviour
{
    public float pulseSpeed = 2f;
    public float minScale = 1f;
    public float maxScale = 1.2f;

    private Vector3 originalScale;
    private float pulseTimer = 0f;
    private bool isPulsing = false;

    void Start()
    {
        originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
        StartCoroutine(StartPulse());
    }

    private System.Collections.IEnumerator StartPulse()
    {
        yield return new WaitForSeconds(1f);

        // 처음 1.2까지 확대
        float duration = 1f / pulseSpeed; // 한 사이클의 절반 시간
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float scale = Mathf.Lerp(0f, maxScale, t);
            transform.localScale = originalScale * scale;
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = originalScale * maxScale;

        isPulsing = true;
        pulseTimer = 0f;
    }

    void Update()
    {
        if (!isPulsing)
            return;

        // 1.2 → 1 → 1.2 반복
        pulseTimer += Time.deltaTime * pulseSpeed;
        float sinValue = Mathf.Sin(pulseTimer * Mathf.PI); // 0~π: 1.2→1, π~2π: 1→1.2
        float scale = Mathf.Lerp(minScale, maxScale, (sinValue + 1f) / 2f);
        transform.localScale = originalScale * scale;
    }
}
