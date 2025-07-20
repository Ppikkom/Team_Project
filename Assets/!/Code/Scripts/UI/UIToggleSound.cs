using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

[RequireComponent(typeof(Toggle))]
public class UIToggleSound : MonoBehaviour
{
    public AudioClip toggleClip; // 토글 클릭 효과음
    public AudioMixerGroup sfxMixerGroup; // SFX 그룹 할당

    private AudioSource audioSource;
    private Toggle toggle;

    void Awake()
    {
        toggle = GetComponent<Toggle>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        if (sfxMixerGroup != null)
            audioSource.outputAudioMixerGroup = sfxMixerGroup;

        toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    void OnDestroy()
    {
        toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
    }

    private void OnToggleValueChanged(bool isOn)
    {
        if (toggleClip != null)
            audioSource.PlayOneShot(toggleClip);
    }
}
