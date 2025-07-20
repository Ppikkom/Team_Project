using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio; // 추가

public class DustChute : MonoBehaviour, IInteractable
{
    public 상호작용_타입 상호작용_종류 => 상호작용_타입.일반상호작용;
    public 상호작용_방식 상호작용_방식 => 상호작용_방식.즉시;

    [SerializeField] private Collider burnRangeCollider;

    [Header("Door Slide")]
    [SerializeField] private Transform doorObject; // 문 오브젝트
    [SerializeField] private Vector3 closedPosition; // 닫힌 위치(로컬)
    [SerializeField] private Vector3 openPosition;   // 열린 위치(로컬)
    [SerializeField] private float slideDuration = 0.5f; // 슬라이드 시간

    [SerializeField] private float burnTimePerTrash = 1.0f; // 쓰레기 1개당 소각 시간(초)
    private bool isBurning = false;

    private Coroutine slideCoroutine;

    public bool IsOpen { get; private set; } = false;

    [Header("소각 효과음")]
    public AudioClip burnClip;
    public AudioMixerGroup sfxMixerGroup;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        if (sfxMixerGroup != null)
            audioSource.outputAudioMixerGroup = sfxMixerGroup;
    }

    void Start()
    {

    }

    // 열기
    public void Open()
    {
        if (!IsOpen && !isBurning)
        {
            IsOpen = true;
            SlideDoor(openPosition);
            Debug.Log("폐기물 투입구가 열렸습니다.");
        }
        else if (isBurning)
        {
            Debug.Log("소각 중에는 문을 열 수 없습니다.");
        }
    }

    public void Close()
    {
        if (IsOpen)
        {
            IsOpen = false;
            SlideDoor(closedPosition);
            Debug.Log("폐기물 투입구가 닫혔습니다.");
        }
    }

    public void TryBurn()
    {
        if (isBurning)
        {
            Debug.Log("이미 소각 중입니다.");
            return;
        }

        if (!IsOpen)
        {
            int trashCount = CountTrashInRange();
            if (trashCount > 0)
            {
                StartCoroutine(BurnCoroutine(trashCount));
            }
            else
            {
                Debug.Log("소각할 쓰레기가 없습니다.");
            }
        }
        else
        {
            Debug.Log("폐기물 투입구가 닫혀 있어야 소각할 수 있습니다.");
        }
    }


    private int CountTrashInRange()
    {
        if (burnRangeCollider == null) return 0;

        int count = 0;
        var trashObjects = GameObject.FindGameObjectsWithTag("Trash");
        var rangeBounds = burnRangeCollider.bounds;

        foreach (var trash in trashObjects)
        {
            var col = trash.GetComponent<Collider>();
            if (col != null && rangeBounds.Intersects(col.bounds))
            {
                count++;
            }
        }
        Debug.Log($"Trash 태그 감지 개수(직접 bounds 체크): {count}");
        return count;
    }

    private void BurnObjectsInRange()
    {
        if (burnRangeCollider == null) return;

        var trashObjects = GameObject.FindGameObjectsWithTag("Trash");
        var rangeBounds = burnRangeCollider.bounds;

        foreach (var trash in trashObjects)
        {
            var col = trash.GetComponent<Collider>();
            if (col != null && rangeBounds.Intersects(col.bounds))
            {
                Destroy(trash);
            }
        }
    }


    private IEnumerator BurnCoroutine(int trashCount)
    {
        isBurning = true;
        float burnTime = trashCount * burnTimePerTrash;
        Debug.Log($"소각 시작: {trashCount}개, {burnTime}초 동안 문이 잠깁니다.");

        // 소각 효과음 재생 (볼륨 50%)
        if (burnClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(burnClip, 0.2f); // 볼륨을 0.5로 지정
            yield return new WaitForSeconds(burnClip.length);
        }
        else
        {
            float elapsed = 0f;
            while (elapsed < burnTime)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        BurnObjectsInRange(); // 소각 완료 후 실제 삭제
        Debug.Log("소각 완료!");
        isBurning = false;
    }


    private void SlideDoor(Vector3 targetLocalPos)
    {
        if (doorObject == null) return;
        if (slideCoroutine != null) StopCoroutine(slideCoroutine);
        slideCoroutine = StartCoroutine(SlideDoorCoroutine(targetLocalPos));
    }

    private IEnumerator SlideDoorCoroutine(Vector3 targetLocalPos)
    {
        Vector3 start = doorObject.localPosition;
        Vector3 end = targetLocalPos;
        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            doorObject.localPosition = Vector3.Lerp(start, end, elapsed / slideDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        doorObject.localPosition = end;
    }


    // 토글
    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    public void 상호작용_시작()
    {
        //if (!IsOpen)
        //{
        //    Debug.Log("폐기물 투입구가 닫혀 있어 상호작용할 수 없습니다.");
        //    return;
        //}

        //var player = Object.FindAnyObjectByType<PlayerController>();
        //if (player == null)
        //{
        //    Debug.LogWarning("플레이어를 찾을 수 없습니다.");
        //    return;
        //}

        //// 플레이어가 들고 있는 아이템 가져오기 (BaseItem 타입 가정)
        //BaseItem heldItem = player.현재_들고있는_아이템; // 실제 변수명에 맞게 수정 필요

        //if (heldItem == null)
        //{
        //    Debug.Log("플레이어가 들고 있는 아이템이 없습니다.");
        //    return;
        //}

        //// Trash, Leaflet, TrashBag만 삭제 가능
        //if (heldItem is Trash || heldItem is Leaflet || heldItem is TrashBag)
        //{
        //    Destroy(heldItem.gameObject);
        //    player.현재_들고있는_아이템 = null; // 아이템 해제 (실제 변수명에 맞게 수정)
        //    Debug.Log("폐기물투입구에 아이템을 정상적으로 폐기함");
        //}
        //else
        //{
        //    Debug.Log("이 아이템은 폐기물투입구에 폐기할 수 없습니다.");
        //}
    }

    public void 상호작용_유지(float 유지시간)
    {
        // 필요시 구현
    }

    public void 상호작용_종료()
    {
        // 필요시 구현
    }

    private void OnTriggerEnter(Collider other)
    {
        // trash 태그가 붙은 오브젝트만 삭제
        if (other.CompareTag("Trash"))
        {
            Destroy(other.gameObject);
            Debug.Log("폐기물투입구에 들어온 Trash 태그 오브젝트 삭제");
        }
    }

}
