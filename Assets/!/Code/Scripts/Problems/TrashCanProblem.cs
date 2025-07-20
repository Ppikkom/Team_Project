using System.Collections;
using UnityEngine;

public class TrashCanProblem : BaseProblem, IInteractable
{
    private Coroutine penaltyCoroutine;  // 감점 루틴 핸들
    private bool isPenaltyRunning = false;

    public TrashCan trashCanComponent;

    protected override void OnEnable()
    {
        base.OnEnable();        // BaseProblem MinusCoroutine 시작
        StartPenaltyRoutine();  // TrashCanProblem 전용 감점 루틴 시작
        if (!ProblemManager.인스턴스.activeProblems.Contains(this))
            ProblemManager.인스턴스.RegisterProblem(this);
    }

    protected override void OnDisable()
    {
        StopPenaltyRoutine();
        StopMinusCoroutine();

        if (!isSolved && GameplayUIController.인스턴스 != null)
        {
            GameplayUIController.인스턴스.normalDisCount(); // 직접 UI 감소시키는 부분
        }

        base.OnDisable();
    }


    public void StartPenaltyRoutine()
    {
        if (!isPenaltyRunning)
        {
            penaltyCoroutine = StartCoroutine(PenaltyRoutine());
            isPenaltyRunning = true;
        }
    }

    public void StopPenaltyRoutine()
    {
        if (penaltyCoroutine != null)
        {
            StopCoroutine(penaltyCoroutine);
            penaltyCoroutine = null;
            isPenaltyRunning = false;
        }
    }

    private IEnumerator PenaltyRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);
            ReputationManager.인스턴스?.문제방치();
        }
    }

    public void OnTrashBagGiven()
    {
        StopPenaltyRoutine();
        OnSolved(); // 여기서 해결 처리
        gameObject.SetActive(false); // 여기서 비활성화 처리
    }


    public override bool CanBeSolvedWith(BaseItem item)
    {
        return true;
    }

    public override void OnSolved()
    {
        if (isSolved) return;
        isSolved = true;

        StopMinusCoroutine();
        PlusCoroutine();

        GameplayUIController.인스턴스?.normalDisCount();
        ProblemManager.인스턴스?.NotifyProblemSolved(this);

        // 비활성화 또는 풀로 반환
        if (!string.IsNullOrEmpty(poolTag))
            ProblemPoolManager.Instance?.ReturnToPool(poolTag, gameObject);
        else
            Destroy(gameObject);
    }

    public override void 상호작용_시작()
    { 

        var player = FindAnyObjectByType<PlayerController>();
        if (player == null)
        {
            return;
        }

        if (!CanBeSolvedWith(player.현재_들고있는_아이템))
        {
            return;
        }

        player.아이템_놓기();
        OnSolved(); // OnSolved에서 비활성화 처리
    }
}
