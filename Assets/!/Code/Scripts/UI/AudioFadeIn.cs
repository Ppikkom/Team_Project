using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioFadeIn : MonoBehaviour
{
    public float fadeDuration = 2f; // 볼륨이 1이 되기까지 걸리는 시간(초)

    private AudioSource audioSource;
    private float timer = 0f;
    private bool fadingIn = true;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = 0f;
        audioSource.Play();
    }

    void Update()
    {
        if (fadingIn)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Clamp01(timer / fadeDuration);

            if (audioSource.volume >= 1f)
            {
                fadingIn = false;
            }
        }
    }
}
