using UnityEngine;
using System.Collections;
using UnityEngine.Audio; // 추가

public enum DustChuteButtonType
{
    Toggle,
    Burn
}

public class DustChuteButton : MonoBehaviour, IInteractable
{
    public DustChute targetChute;
    public DustChuteButtonType buttonType;

    public 상호작용_타입 상호작용_종류 => 상호작용_타입.일반상호작용;
    public 상호작용_방식 상호작용_방식 => 상호작용_방식.즉시;

    [SerializeField] private Material targetMaterial; // 인스펙터에서 할당 가능
    private Renderer _renderer;
    private Material _material;
    private Color _originalEmission;
    private Coroutine _emissionCoroutine;
    [SerializeField] private float emissionFlashDuration = 0.15f;
    [SerializeField] private Color flashEmissionColor = Color.white;
    [SerializeField] private float flashEmissionIntensity = 2.5f;

    [Header("효과음")]
    public AudioClip interactClip; // 상호작용 효과음
    public AudioMixerGroup sfxMixerGroup; // SFX 그룹
    private AudioSource audioSource;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        // 인스펙터에서 할당된 메트리얼이 있으면 사용, 아니면 Renderer에서 가져옴
        _material = targetMaterial != null ? targetMaterial : (_renderer != null ? _renderer.material : null);

        if (_material != null && _material.HasProperty("_EmissionColor"))
            _originalEmission = _material.GetColor("_EmissionColor");

        // 오디오소스 준비
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        if (sfxMixerGroup != null)
            audioSource.outputAudioMixerGroup = sfxMixerGroup;
    }

    public void 상호작용_시작()
    {
        if (targetChute == null) return;

        // 효과음 재생
        if (interactClip != null && audioSource != null)
            audioSource.PlayOneShot(interactClip);

        // Emission 반짝임 효과
        if (_material != null && _material.HasProperty("_EmissionColor"))
        {
            if (_emissionCoroutine != null)
                StopCoroutine(_emissionCoroutine);
            _emissionCoroutine = StartCoroutine(FlashEmission());
        }

        switch (buttonType)
        {
            case DustChuteButtonType.Toggle:
                targetChute.Toggle();
                break;
            case DustChuteButtonType.Burn:
                targetChute.TryBurn();
                break;
        }
    }

    private IEnumerator FlashEmission()
    {
        _material.EnableKeyword("_EMISSION");
        _material.SetColor("_EmissionColor", flashEmissionColor * flashEmissionIntensity);

        yield return new WaitForSeconds(emissionFlashDuration);

        _material.SetColor("_EmissionColor", _originalEmission);
    }

    public void 상호작용_유지(float 유지시간) { }
    public void 상호작용_종료() { }
}
