using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections;

[RequireComponent(typeof(Slider))]
public class UISliderSound : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public AudioClip dragClip; // 슬라이더 조절 효과음
    public AudioMixerGroup sfxMixerGroup; // SFX 그룹 할당

    private AudioSource audioSource;
    private Coroutine dragCoroutine;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        audioSource.loop = false; // 루프 사용 안함
        if (sfxMixerGroup != null)
            audioSource.outputAudioMixerGroup = sfxMixerGroup;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (dragClip != null && dragCoroutine == null)
        {
            dragCoroutine = StartCoroutine(PlayDragSoundLoop());
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (dragCoroutine != null)
        {
            StopCoroutine(dragCoroutine);
            dragCoroutine = null;
        }
    }

    void OnDisable()
    {
        if (dragCoroutine != null)
        {
            StopCoroutine(dragCoroutine);
            dragCoroutine = null;
        }
    }

    private IEnumerator PlayDragSoundLoop()
    {
        while (true)
        {
            audioSource.PlayOneShot(dragClip);
            yield return new WaitForSeconds(0.1f);
        }
    }
}
