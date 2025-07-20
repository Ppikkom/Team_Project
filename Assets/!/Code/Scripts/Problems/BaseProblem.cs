using System.Collections;
using UnityEngine;

// 문제 종류 : ProblemType
// 진상 문제는 Human으로, 일반 문제는 각 문제 별 tag와 적용된 스크립트에 따라 정리
public enum ProblemType
{
    Puke, // 토사물
    Trash, // 쓰레기
    TrashCan, // 쓰레기통
    Leaflet, // 전단지
    Graffiti, // 그래피티
    Villain, // 진상 npc
    NPC, // 일반 npc
    Fire // 불
}

// 문제 타입 및 생성 위치 초기화 설정 : ProblemConfig
public class ProblemConfig
{
    public ProblemType type; // 문제 종류 : ProblemType
    public Vector3 spawnPosition; // 생성 위치 : Vector3
}

// 문제 기본 동작 정의 클래스 : BaseProblem
// 일반 문제 및 진상 문제 Prefab에 적용된 프리펩이 상속받아 공통 로직 공유
// 공통 로직은 상호작용 처리, 점수 처리, 문제 해결 처리 포함
public class BaseProblem : MonoBehaviour, IInteractable
{
    // IInteractable 상속으로 구현
    #region 인터페이스 구현
    public virtual 상호작용_타입 상호작용_종류 => 상호작용_타입.일반상호작용;
    public virtual 상호작용_방식 상호작용_방식 => 상호작용_방식.즉시;
    #endregion

    #region 필드 및 상태
    public ProblemType problemType; // 문제 종류
    public Vector3 spawnPosition; // 생성 위치
    public string poolTag; // 오브젝트 풀 태그
    public bool isSolved = false; // 문제 해결 여부
    protected Coroutine Coroutine; // 점수 코루틴 작동 부분
    #endregion

    #region 유니티 생명주기
    // 감점 코루틴 시작 :: void Start()
    protected virtual void Start()
    {
        if (ProblemManager.인스턴스 != null)
        {
            ProblemManager.인스턴스.RegisterProblem(this);
        }

        StartMinusCoroutine();
    }

    // 문제 등장 UI 등록 :: void
    protected virtual void OnEnable()
    {

        // 감점 코루틴 자동 시작
        StartMinusCoroutine();
    }

    // 문제 등장 UI 해제 :: void
    protected virtual void OnDisable()
    {

        // 감점 코루틴 정지
        StopMinusCoroutine();

        // 문제 등록 해제
        if (!isSolved && ProblemManager.인스턴스 != null)
        {
            ProblemManager.인스턴스.UnregisterProblem(this);
        }
    }

    // 문제 파괴 :: void
    private void OnDestroy()
    {
        // 이미 해결된 문제는 중복 해제하지 않도록
        if (!isSolved && ProblemManager.인스턴스 != null)
        {
            ProblemManager.인스턴스.UnregisterProblem(this);
        }
    }
    #endregion

    #region 문제 초기화
    // 초기화(설정: ProblemConfig) :: void
    public virtual void Initialize(ProblemConfig config)
    {
        problemType = config.type;
        spawnPosition = config.spawnPosition;
        transform.position = spawnPosition;
        gameObject.name = $"문제_{problemType}_{spawnPosition.x:F1}_{spawnPosition.z:F1}";
        poolTag = problemType.ToString();
        isSolved = false;
    }

    // 문제 등록 여부 체크 :: void
    private void TryRegisterSelf()
    {
        if (ProblemManager.인스턴스 == null) return;

        if (!ProblemManager.인스턴스.activeProblems.Contains(this))
        {
            ProblemManager.인스턴스.RegisterProblem(this);
        }
    }

    #endregion

    #region 상호작용 처리
    // IInteractable 상속으로 구현
    public virtual void 상호작용_시작()
    {
        var 플레이어 = FindAnyObjectByType<PlayerController>();
        if (플레이어 == null) return;

        if (CanBeSolvedWith(플레이어.현재_들고있는_아이템))
        {
            OnSolved();
        }
    }

    public virtual void 상호작용_유지(float 유지시간) { }
    public virtual void 상호작용_종료() { }

    public virtual bool CanBeSolvedWith(BaseItem item)
    {
        return true;
    }
    #endregion

    #region 문제 해결 처리

    // 해결시() :: void
    public virtual void OnSolved()
    {
        if (isSolved) return; // 중복 호출 방지
        isSolved = true;

        // 해결 시 점수 감소 정지 후, 점수 증가
        StopMinusCoroutine(); 
        PlusCoroutine();

        // UI에 문제 해결 알림
        if (GameplayUIController.인스턴스 != null)
        {
            if (problemType == ProblemType.Villain)
            {
                GameplayUIController.인스턴스.jinsangDisCount();
            }
            else
            {
                GameplayUIController.인스턴스.normalDisCount();
            }
        }

        var manager = ProblemManager.인스턴스;
        if (manager != null)
            manager.NotifyProblemSolved(this);
    }
    #endregion

    #region 점수 코루틴
    // 점수 감소 코루틴
    protected virtual IEnumerator MinusCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);
            if (ReputationManager.인스턴스 != null)
            {
                ReputationManager.인스턴스.문제방치();
            }
        }
    }

    // 점수 증가 코루틴
    protected virtual void PlusCoroutine()
    {
        if (ReputationManager.인스턴스 != null)
        {
            ReputationManager.인스턴스.점수추가(20);
        }
    }

    // 점수 감소 시작
    protected virtual void StartMinusCoroutine()
    {
        if (Coroutine == null)
            Coroutine = StartCoroutine(MinusCoroutine());
    }

    // 점수 감소 정지
    protected virtual void StopMinusCoroutine()
    {
        if (Coroutine != null)
        {
            StopCoroutine(Coroutine);
            Coroutine = null;
        }
    }
    #endregion
}