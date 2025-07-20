using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashCan : MonoBehaviour, IInteractable
{
    public 상호작용_타입 상호작용_종류 => 상호작용_타입.일반상호작용;
    public 상호작용_방식 상호작용_방식 => 상호작용_방식.즉시;
    public int 깍는점수 = 10;
    public bool 했나 = false;

    [Header("최대 스택(체력)")]
    public int maxStack = 5;
    [Header("현재 스택")]
    public int currentStack = 0;

    [Header("쓰레기봉투 프리팹")]
    public GameObject trashBagPrefab;

    [Header("불 프리팹")]
    public GameObject firePrefab;

    [Header("불 위치 오프셋")]
    public Vector3 fireOffset = new Vector3(0, 1, 0);

    [Header("불난 상태")]
    public bool isOnFire = false;

    private GameObject fireObject;
    private bool isFull = false;
    public GameObject 꽉찼어요;
    private bool trashBagGiven = false;

    [Header("쓰레기 차오르기")]
    public GameObject trashFillObject; 
    public Transform bottomPosition; 
    public Transform fullPosition;


    [Header("효과음")]
    public AudioClip trashBagGiveSound;
    private AudioSource audioSource;
    public AudioClip fireLoopSound; // 추가: 불 효과음
    public AudioClip trashInSound;
    private AudioSource fireAudioSource; // 추가: 불 효과음 전용

    private bool isGivingTrashBag = false; // 추가: 봉투 지급 중 여부


    void Start()
    {
        if (isOnFire)
            ActivateFire();
        UpdateFullState();
        UpdateTrashFill();

        audioSource = GetComponent<AudioSource>();

        if (fireAudioSource == null)
        {
            fireAudioSource = gameObject.AddComponent<AudioSource>();
            fireAudioSource.loop = true;
            fireAudioSource.playOnAwake = false;
        }
    }

    void Update()
    {
        if (isOnFire && fireObject == null)
        {
            isOnFire = false;
        }
    }

    void OnValidate()
    {
        if (isOnFire)
            ActivateFire();
        else
            DeactivateFire();
    }

    // 쓰레기통이 꽉찼는지 상태 갱신
    void UpdateFullState()
    {
        isFull = currentStack >= maxStack;
        SetChildObjectsActive(isFull);
        UpdateTrashFill();
    }

    void UpdateTrashFill()
    {
        if (trashFillObject == null || bottomPosition == null || fullPosition == null)
            return;

        if (currentStack <= 0 || currentStack >= maxStack)
        {
            // 쓰레기가 없거나 최대치에 도달하면 비활성화
            trashFillObject.SetActive(false);
            return;
        }

        // 쓰레기가 1개 이상이고 최대치 미만이면 활성화
        trashFillObject.SetActive(true);

        // 0부터 maxStack까지의 비율 계산 (0~1)
        float fillRatio = (float)currentStack / (float)maxStack;
        
        // 위치 보간 (바닥에서 풀까지)
        Vector3 targetPosition = Vector3.Lerp(bottomPosition.position, fullPosition.position, fillRatio);
        trashFillObject.transform.position = targetPosition;

        // 스케일 보간 (bottomPosition과 fullPosition의 스케일을 자동으로 사용)
        Vector3 bottomScale = bottomPosition.localScale;
        Vector3 fullScale = fullPosition.localScale;
        Vector3 targetScale = Vector3.Lerp(bottomScale, fullScale, fillRatio);
        trashFillObject.transform.localScale = targetScale;
    }

    // IInteractable 구현
    public void 상호작용_시작()
    {
        if (isFull && !isGivingTrashBag)
        {
            isGivingTrashBag = true; // 봉투 지급 중으로 설정
            StartCoroutine(GiveTrashBagToPlayerAfterSound());

            if (GameplayUIController.인스턴스 != null)
            {
                GameplayUIController.인스턴스.normalDisCount();
            }

            var problem = GetComponentInParent<TrashCanProblem>();
            if (problem != null)
            {
                problem.OnTrashBagGiven();
            }
        }
    }

    private IEnumerator GiveTrashBagToPlayerAfterSound()
    {
        // 플레이어 이동 막기
        var player = Object.FindAnyObjectByType<PlayerController>();
        if (player != null)
            player.SetMoveBlocked(true);

        // 효과음 재생
        if (audioSource != null && trashBagGiveSound != null)
            audioSource.PlayOneShot(trashBagGiveSound);

        // 효과음 길이만큼 대기
        float waitTime = (trashBagGiveSound != null) ? trashBagGiveSound.length : 0f;
        yield return new WaitForSeconds(waitTime);

        // 아이템 지급
        if (player != null && trashBagPrefab != null)
        {
            GameObject bag = Instantiate(trashBagPrefab);
            var trash = bag.GetComponentInParent<BaseItem>();
            if (trash != null)
            {
                player.아이템_획득(trash);
            }
            // 이동 다시 허용
            player.SetMoveBlocked(false);
        }

        // 효과음이 끝난 뒤에 상태 해제
        currentStack = 0;
        isFull = false;
        trashBagGiven = true;
        SetChildObjectsActive(false);
        isGivingTrashBag = false; // 봉투 지급 완료 후 해제
    }





    public void 상호작용_유지(float 유지시간) { }
    public void 상호작용_종료() { }

    void GiveTrashBagToPlayer()
    {
        var player = Object.FindAnyObjectByType<PlayerController>();
        if (player != null && trashBagPrefab != null)
        {
            GameObject bag = Instantiate(trashBagPrefab);
            var trash = bag.GetComponentInParent<BaseItem>();
            if (trash != null)
            {
                player.아이템_획득(trash);
            }

            // 효과음 재생
            if (audioSource != null && trashBagGiveSound != null)
                audioSource.PlayOneShot(trashBagGiveSound);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isFull)
        {
            //Debug.Log("쓰레기통이 꽉 차서 아이템을 버릴 수 없습니다.");
            return;
        }

        if (other.TryGetComponent<Trash>(out Trash 쓰레기))
        {
            if (쓰레기.버려졌는가)
            {
                //// 점수 증가
                //if (ReputationManager.인스턴스 != null)
                //    ReputationManager.인스턴스.점수추가(깍는점수);

                // 쓰레기 투입 시
                var problem = 쓰레기.GetComponentInParent<TrashProblem>();
                if (problem != null && !problem.isSolved)
                {
                    problem.OnSolved();
                }

                // 쓰레기 투입 효과음 재생
                if (audioSource != null && trashInSound != null)
                    audioSource.PlayOneShot(trashInSound);
                Debug.Log("쓰레기 들어옴: " + 쓰레기.name);
                Destroy(쓰레기.gameObject);
                currentStack++;
                UpdateFullState();

                return; // 추가: 중복 처리 방지
            }
        }
        else if (other.TryGetComponent<Leaflet>(out Leaflet leaflet))
        {
            if (leaflet.버려졌는가)
            {
                //// 점수 증가
                //if (ReputationManager.인스턴스 != null)
                //    ReputationManager.인스턴스.점수추가(깍는점수);

                var problem = leaflet.GetComponentInParent<LeafletProblemB>();
                if (problem != null && !problem.isSolved)
                {
                    problem.OnSolved();
                }

                if (audioSource != null && trashInSound != null)
                    audioSource.PlayOneShot(trashInSound);
                Destroy(leaflet.gameObject);
                currentStack++;
                UpdateFullState();
            }
        }
    }


    void SetChildObjectsActive(bool active)
    {
        if (꽉찼어요 != null)
        {
            꽉찼어요.SetActive(active);
        }
    }

    // 불 상태를 외부에서 신호로 제어
    public void SetFire(bool on)
    {
        isOnFire = on;
        if (isOnFire)
            ActivateFire();
        else
            DeactivateFire();
    }

    void ActivateFire()
    {
        if (fireObject == null && firePrefab != null)
        {
            fireObject = Instantiate(firePrefab, transform.position + fireOffset, Quaternion.identity);
        }
        else if (fireObject != null)
        {
            fireObject.transform.position = transform.position + fireOffset;
            fireObject.SetActive(true);
        }

        // 불 효과음 루프 재생
        if (fireAudioSource != null && fireLoopSound != null)
        {
            fireAudioSource.clip = fireLoopSound;
            if (!fireAudioSource.isPlaying)
                fireAudioSource.Play();
        }
    }

    void DeactivateFire()
    {
        if (fireObject != null)
        {
            Destroy(fireObject);
            fireObject = null;
        }

        // 불 효과음 정지
        if (fireAudioSource != null && fireAudioSource.isPlaying)
        {
            fireAudioSource.Stop();
        }

        if (ReputationManager.인스턴스 != null)
        {
            ReputationManager.인스턴스.점수추가(20);
        }
    }

    public void AddStack()
    {
        if (isFull) return;

        currentStack++;
        if (currentStack >= maxStack)
        {
            isFull = true;
            SetChildObjectsActive(true);
            isGivingTrashBag = false; // 쓰레기통이 다시 꽉 차면 초기화
        }
        UpdateTrashFill();
    }

    public void AddFullStack()
    {
        currentStack = maxStack;
        isFull = true;
        SetChildObjectsActive(true);
        UpdateTrashFill();
        isGivingTrashBag = false; // 쓰레기통이 다시 꽉 차면 초기화
    }
}
