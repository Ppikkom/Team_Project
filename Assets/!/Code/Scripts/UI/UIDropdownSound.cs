using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Audio;
using System.Collections.Generic;

//

[RequireComponent(typeof(TMP_Dropdown))]
public class UIDropdownSound : MonoBehaviour, IPointerClickHandler
{
    public AudioClip buttonClickClip;   // 드롭다운 버튼 클릭 효과음
    public AudioClip itemHoverClip;     // 항목 마우스오버 효과음
    public AudioClip itemSelectClip;    // 항목 선택 효과음
    public AudioMixerGroup sfxMixerGroup;

    private AudioSource audioSource;
    private TMP_Dropdown dropdown;
    private List<TMP_Dropdown.OptionData> options;

    void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        if (sfxMixerGroup != null)
            audioSource.outputAudioMixerGroup = sfxMixerGroup;
    }

    void OnEnable()
    {
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    void OnDisable()
    {
        dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
    }

    // 드롭다운 버튼 클릭
    public void OnPointerClick(PointerEventData eventData)
    {
        if (buttonClickClip != null)
            audioSource.PlayOneShot(buttonClickClip);

        // 드롭다운이 열릴 때 항목에 마우스오버 이벤트 연결
        StartCoroutine(SetupDropdownItems());
    }

    // 항목 선택
    private void OnDropdownValueChanged(int index)
    {
        if (itemSelectClip != null)
            audioSource.PlayOneShot(itemSelectClip);
    }

    // 드롭다운이 열릴 때 항목에 마우스오버 이벤트 연결
    private System.Collections.IEnumerator SetupDropdownItems()
    {
        // 드롭다운이 열릴 때까지 대기
        yield return null;

        // TMP_Dropdown의 드롭다운 리스트 오브젝트 찾기
        var dropdownList = GameObject.Find("TMP Dropdown List");
        if (dropdownList == null)
            yield break;

        // 각 항목(Toggle)에 마우스오버 이벤트 추가
        foreach (var toggle in dropdownList.GetComponentsInChildren<Toggle>())
        {
            var trigger = toggle.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = toggle.gameObject.AddComponent<EventTrigger>();

            // 기존 마우스오버 이벤트 제거
            trigger.triggers.RemoveAll(e => e.eventID == EventTriggerType.PointerEnter);

            // 새 마우스오버 이벤트 추가
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((data) =>
            {
                if (itemHoverClip != null)
                    audioSource.PlayOneShot(itemHoverClip);
            });
            trigger.triggers.Add(entry);
        }
    }

}
