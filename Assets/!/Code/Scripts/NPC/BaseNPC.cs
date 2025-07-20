using UnityEngine;
using Unity.Behavior;
using System.Collections.Generic;
using UnityEngine.AI;
using System.Linq;
using TMPro;
using System.Collections;

public enum NPCType { Normal, Villain }

[System.Serializable]
public struct Destination
{
    public int _floor;
    public string _name;
}

[System.Serializable]
public class Monologue // [대사 추가] 대사 데이터 구조체
{
    public NPCType npcType;
    public string[] lines; // 일반 혼잣말
    public string[] hitLines; // 공격 시 대사
}

public class BaseNPC : MonoBehaviour
{
    public NPCType NPC종류 = NPCType.Normal;
    // 행동 그래프
    protected BehaviorGraphAgent behaviorAgent;
    // 네비메쉬
    protected NavMeshAgent navMeshAgent;
    // 맵데이터
    [SerializeField] protected MapData[] mapData;
    public List<Destination> 목적지;
    public int Index = 0;
    protected int 현재_층 = 1;
    public Vector3 목적지_위치;

    // 아니메
    protected Animator anim;

    // 상태_패턴
    protected Dictionary<NPCState, INPCState> _stateMap;
    protected INPCState _currentState;
    protected NPCState _currentKey;

    // [대사 추가] 대사 관련 변수
    [SerializeField] protected Monologue[] 대사목록;
    [SerializeField] protected TextMeshProUGUI 대사텍스트;
    [SerializeField] protected GameObject 대사캔버스;
    [SerializeField] protected float 최소대사간격 = 5f, 최대대사간격 = 15f;
    [SerializeField] protected float 표시거리 = 10f;
    protected string[] 현재대사;
    protected string[] 현재공격대사;
    protected Transform 플레이어;
    protected bool 대사표시중;
    protected Camera 메인카메라;

    // 추가: Ragdoll 부위 관리
    [SerializeField] protected Rigidbody[] ragdollRigidbodies; // 모든 Ragdoll Rigidbody
    [SerializeField] protected Collider[] ragdollColliders;   // 모든 Ragdoll Collider
    protected bool isRagdollActive = false;

    // 추가 영역 : type
    public virtual ProblemType problemType => ProblemType.NPC;

    [Header("NPC 발소리")]
    public AudioClip footstepClip;
    public float footstepInterval = 0.4f;
    public AudioSource footstepAudioSource; // 인스펙터에서 직접 할당
    private Coroutine footstepCoroutine;


    public virtual void Awake()
    {
        _stateMap = new Dictionary<NPCState, INPCState>
        {
            { NPCState.Move,     new MoveState()     },
            { NPCState.Wander,   new WanderState()   },
            { NPCState.Behavior, new BehaviorState() },
            { NPCState.Stop,     new StopState()     }
        };



        // Ragdoll 부위 초기화
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>(); // 자식 오브젝트에서 모든 Rigidbody 수집
        ragdollColliders = GetComponentsInChildren<Collider>();   // 자식 오브젝트에서 모든 Collider 수집
        SetupRagdoll(false); // 초기에는 Ragdoll 비활성화
    }

    public virtual void OnEnable()
    {
        behaviorAgent = GetComponent<BehaviorGraphAgent>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        mapData = FindObjectsByType<MapData>(FindObjectsSortMode.None).OrderBy(m =>m.transform.GetSiblingIndex()).ToArray();
        anim = GetComponentInChildren<Animator>();

        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;

        // [대사 추가] 대사 시스템 초기화
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            플레이어 = playerObj.transform;
        }
        else
        {
            //Debug.LogError($"{gameObject.name}에서 Player 태그를 가진 오브젝트를 찾을 수 없습니다!");
        }

        메인카메라 = Camera.main;
        대사캔버스.SetActive(false);
        var 대사 = 대사목록.FirstOrDefault(m => m.npcType == NPC종류);
        if (대사 != null && 대사.lines != null && 대사.lines.Length > 0)
        {
            현재대사 = 대사.lines;
            현재공격대사 = 대사.hitLines;
            StartCoroutine(혼잣말루틴());
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}에 {NPC종류} 타입의 대사가 없거나 비어 있습니다!");
        }

        // 테스트용 코드
        if (플레이어 != null) 플레이어 = playerObj.transform;
        else return;
    }

    public virtual void Update()
    {
        var newKey = ReadBlackboardState();
        // 기존 상태와 비교
        if (newKey != _currentKey)
            TransitionTo(newKey);
        // 상태 업데이트 및 실행
        _currentState?.Execute(this);

        // [대사 추가] 대사 UI가 카메라를 향하도록
        if (대사캔버스.activeSelf)
        {
            대사캔버스.transform.LookAt(대사캔버스.transform.position + 메인카메라.transform.rotation * Vector3.forward,
                                        메인카메라.transform.rotation * Vector3.up);
            텍스트크기갱신();
        }
    }



    // Ragdoll 활성화/비활성화
    protected void SetupRagdoll(bool enable)
    {
        isRagdollActive = enable;
        foreach (var rb in ragdollRigidbodies)
        {
            rb.isKinematic = !enable;
            rb.useGravity = enable;
            rb.constraints = RigidbodyConstraints.None;
        }
        foreach (var col in ragdollColliders)
        {
            col.enabled = enable;
        }

        Rigidbody rootRb = GetComponent<Rigidbody>();
        Collider rootCollider = GetComponent<Collider>();
        if (rootRb != null)
        {
            rootRb.isKinematic = enable;
            rootRb.useGravity = enable;
        }
        if (rootCollider != null)
        {
            rootCollider.enabled = !enable;
        }

        // Animator 확인 및 비활성화
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = !enable;
            animator.Rebind(); // 애니메이션 상태 초기화
            animator.StopPlayback(); // 재생 중인 애니메이션 중지
            Debug.Log($"{gameObject.name} Animator enabled set to: {!enable}");
        }
        else
        {
            Debug.Log($"{gameObject.name} No Animator component found on root");
        }

        // 자식 오브젝트에서도 Animator 검색
        Animator[] childAnimators = GetComponentsInChildren<Animator>();
        foreach (var anim in childAnimators)
        {
            anim.enabled = !enable;
            Debug.Log($"{anim.gameObject.name} Child Animator enabled set to: {!enable}");
        }

        /*
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = !enable;
            navMeshAgent.isStopped = true;
            Debug.Log($"NavMeshAgent enabled: {navMeshAgent.enabled}");
        }
        */
    }

    protected IEnumerator 혼잣말루틴() // 혼잣말 주기적 표시
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(최소대사간격, 최대대사간격));
            if (플레이어 != null && Vector3.Distance(transform.position, 플레이어.position) < 표시거리 && !대사표시중)
            {
                if (현재대사 != null && 현재대사.Length > 0)
                {
                    Debug.Log($"{gameObject.name} 대사 표시 조건 충족. 거리: {Vector3.Distance(transform.position, 플레이어.position)}");
                    string 대사 = 현재대사[Random.Range(0, 현재대사.Length)];
                    StartCoroutine(대사표시(대사));
                }
                else
                {
                    Debug.LogWarning($"{gameObject.name}의 현재대사가 비어 있습니다!");
                }
            }
            else
            {
                //Debug.Log($"대사 표시 조건 미충족: 플레이어={플레이어}, 거리={Vector3.Distance(transform.position, 플레이어?.position ?? Vector3.zero)}, 대사표시중={대사표시중}");
            }
        }
    }

    // [대사 추가] 대사 화면에 표시
    protected IEnumerator 대사표시(string 대사)
    {
        대사표시중 = true;
        대사캔버스.SetActive(true);
        대사텍스트.text = 대사;
        yield return new WaitForSeconds(3f);
        대사캔버스.SetActive(false);
        대사표시중 = false;
    }

    // 피격 메서드 (기존 유지, 필요 시 추가 로직)
    public virtual void 피격(float 피해)
    {
        if (현재공격대사 != null && 현재공격대사.Length > 0)
        {
            StopCoroutine(대사표시(null));
            string 공격대사 = 현재공격대사[Random.Range(0, 현재공격대사.Length)];
            StartCoroutine(대사표시(공격대사));
        }

        if (NPC종류 == NPCType.Villain)
        {
            ReputationManager.인스턴스.점수추가(100);
            StartCoroutine(슬로우모션(1f));
        }
        if (NPC종류 == NPCType.Normal)
        {
            ReputationManager.인스턴스.점수감소(20);
        }

        레그돌전환(피해);
    }

    protected virtual void 레그돌전환(float 힘)
    {
        if (isRagdollActive) return;

        // Test
        GetComponent<Rigidbody>().isKinematic = false;
        _currentState = null;
        navMeshAgent.enabled = false;

        SetupRagdoll(true);
        Debug.Log("Ragdoll activating...");

        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        Vector3 forceDirection = Vector3.forward;
        if (player != null)
        {
            forceDirection = player.forward; // 플레이어 전방 방향
            forceDirection += Vector3.up * 0.5f; // 약간의 상승 추가
        }
        forceDirection = forceDirection.normalized;

        float baseForce = 3f * 힘; 
        foreach (var rb in ragdollRigidbodies)
        {
            rb.AddForce(forceDirection * baseForce, ForceMode.Impulse);
            Debug.Log($"Force applied to {rb.name}: {forceDirection * baseForce}");
        }

        StartCoroutine(제거루틴(5f));
    }

    protected IEnumerator 제거루틴(float 시간) // 오브젝트 풀링 사용시 수정***
    {
        yield return new WaitForSeconds(시간);
        NPCManager.인스턴스.NPC반환(gameObject);
    }

    protected void 텍스트크기갱신() // 거리에 따라 텍스트 크기 조절
    {
        if (메인카메라 == null) return;
        float 거리 = Vector3.Distance(transform.position, 메인카메라.transform.position);
        float 최소크기 = 10f, 최대크기 = 20f, 최소거리 = 2f, 최대거리 = 10f;
        float 크기 = Mathf.Lerp(최대크기, 최소크기, (거리 - 최소거리) / (최대거리 - 최소거리));
        대사텍스트.fontSize = Mathf.Clamp(크기, 최소크기, 최대크기);
    }

    #region 이동
    // 미리 설정한 장소로 이동한다.
    public virtual void 이동_트리거()
    {
        // 이 상태로 입장할 때, 1회 호출
        anim.SetBool("isWalking", true);
        navMeshAgent.isStopped = false;
        PlayFootstepOnce();
    }

    public virtual void 이동_갱신()
    {
        // 이 상태가 유지될 때마다 호출
        StartFootstepLoop();
    }

    public virtual void 이동_종료()
    {
        // 다른 상태로 전이될 때, 1회 호출
        StopFootstepLoop();
    }
    #endregion

    #region 행동
    // 행동 트리거 (상속받아 각 NPC마다 다르게 구현)
    public virtual void 행동_트리거()
    {
        anim.SetBool("isWalking", false);
        
    }

    public virtual void 행동_갱신()
    {

    }

    public virtual void 행동_종료()
    {

    }
    #endregion

    #region 멈춤
    public virtual void 멈춤_트리거()
    {
        navMeshAgent.isStopped = true;
        anim.SetBool("isWalking", false);
    }

    public virtual void 멈춤_갱신()
    {

    }

    public virtual void 멈춤_종료()
    {
        navMeshAgent.isStopped = false;
    }
    #endregion

    #region 배회
    // 무작위적인 위치로 이동할 떄 사용
    public virtual void 배회_트리거()
    {   
        anim.SetBool("isWalking", true);
        navMeshAgent.isStopped = false;
        PlayFootstepOnce();
    }

    public virtual void 배회_갱신()
    {
        StartFootstepLoop();
    }

    public virtual void 배회_종료()
    {
        StopFootstepLoop();
    }
    #endregion

    public virtual void 초기화()
    {
        Debug.Log($"{name} NPC 초기화!");
    }

    public virtual void 처치시()
    {

    }

    public Vector3 목적지위치_반환(int index)
    {
        Collider gateC = mapData[목적지[index]._floor].GetColliderByName(목적지[index]._name);
        현재_층 = 목적지[index]._floor;
        return RandomPoint(gateC, index);
    }

    // 콜라이더 내 좌표 랜덤 반환
    public Vector3 RandomPoint(Collider col, int index = -1)
    {
        if (col == null)
        {
            Debug.LogError($"[{name}] floor:{목적지[index]._floor} 에서 “{목적지[index]._name}” 콜라이더를 찾을 수 없습니다!");
            return Vector3.zero;  
        } 

        Vector3 min = col.bounds.center - col.bounds.extents;
        Vector3 max = col.bounds.center + col.bounds.extents; // col.bounds.size / 2
        float heightOffset = col.bounds.extents.y + 0.5f;

        // 콜라이더 내, 랜덤으로 위치 설정 -> 아래 네비메쉬 있는지 확인 -> 없으면 반복실행
        for (int i = 0; i < 100; i++) // 100번 정도 시도.
        {
            float x = Random.Range(min.x, max.x);
            float z = Random.Range(min.z, max.z);
            Vector3 pos = new Vector3(x, min.y + heightOffset, z);

            if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                return hit.position;
        }

        Debug.LogError($"{name}에서 에러 발생");
        return transform.position;
    }



    public void SetBlackBoardValueBoolean(string name, bool b)
    {
        behaviorAgent.SetVariableValue<bool>(name, b);
    }

    public void SetAgentStopped(bool b)
    {
        // 멈추기 위한 함수.
        // 행동 트리와는 별개임.
        // 차라리 멈춤 트리거, 종료에 넣는게 더 좋을거 같음.
        navMeshAgent.isStopped = b;
    }

    public virtual void OnDisable()
    {
        StopAllCoroutines();
        StopFootstepLoop();
    }

    protected void TransitionTo(NPCState newKey)
    {
        _currentState?.OnExit(this);
        if (_stateMap.TryGetValue(newKey, out var next))
        {
            _currentKey = newKey;
            _currentState = next;
            _currentState.OnEnter(this); // OnEnter에 behaviorsetvalue 넣어야할까?
        }
        else
        {
            Debug.LogWarning($"No strategy for state {newKey}");
            _currentState = null;
        }
    }

    public void 컴포넌트_비활성화()
    {
        _currentState = null;
        navMeshAgent.enabled = false;
        behaviorAgent.enabled = false;
    }

    public void 컴포넌트_활성화()
    {
        navMeshAgent.enabled = true;
        behaviorAgent.enabled = true;
    }

    private NPCState ReadBlackboardState()
    {
        return behaviorAgent.GetValueOrDefault<NPCState>("NPCState");
    }

    // 슬로우 모션 효과를 1초 동안 적용하는 코루틴
    private IEnumerator 슬로우모션(float duration)
    {
        // 원래 시간 스케일 저장
        float originalTimeScale = Time.timeScale;

        // 슬로우 모션 적용 (0.1은 느린 속도, 조정 가능)
        Time.timeScale = 0.1f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale; // 물리 업데이트도 슬로우 모션에 맞게 조정

        yield return new WaitForSecondsRealtime(duration); // 실시간 대기 (timeScale에 영향받지 않음)

        // 원래 시간 스케일로 복귀
        Time.timeScale = originalTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale; // 물리 업데이트 복귀
    }

    #region 발소리관련
    protected void PlayFootstepOnce()
    {
        if (footstepClip != null && footstepAudioSource != null)
            footstepAudioSource.PlayOneShot(footstepClip);
    }

    protected void ChangeDestination(int _index, int _int, string _str)
    {
        var temp = 목적지[_index];
        temp._name = _str;
        temp._floor = _int;
        목적지[_index] = temp;
    }

    protected Vector3 근처위치_반환()
    {
        Vector3 bestPos = 목적지위치_반환(Index); // 초기값 / 제일 가까운 위치
        for (int i = 0; i < 100; i++)
        {
            Vector3 newPos = 목적지위치_반환(Index);
            if (Vector3.Distance(transform.position, newPos) < Vector3.Distance(transform.position, bestPos))
            {
                bestPos = newPos;
            }
        }
        return bestPos;
    }

    protected void StartFootstepLoop()
    {
        if (footstepCoroutine == null && footstepClip != null && footstepAudioSource != null)
            footstepCoroutine = StartCoroutine(FootstepLoop());
    }

    protected void StopFootstepLoop()
    {
        if (footstepCoroutine != null)
        {
            StopCoroutine(footstepCoroutine);
            footstepCoroutine = null;
        }
    }

    private IEnumerator FootstepLoop()
    {
        while (true)
        {
            footstepAudioSource.PlayOneShot(footstepClip);
            yield return new WaitForSeconds(footstepInterval);
        }
    }

    #endregion
}