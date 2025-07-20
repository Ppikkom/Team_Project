using System.Collections;
using UnityEngine;

[RequireComponent(typeof(TrashBag))]
public class TrashBagProblem : BaseProblem
{
    protected override void Start()
    {
        base.Start();
        gameObject.name = "쓰레기봉투";
        problemType = ProblemType.Trash;
    }

    // TrashBagProblem.cs 내부
    protected override void OnEnable()
    {
        base.OnEnable(); // ProblemManager에 다시 등록됨

        // TrashBag의 UI 카운트도 복원 시도
        var trashBag = GetComponent<TrashBag>();
        if (trashBag != null)
        {
            trashBag.TryCountOnProblemReappear();
        }
    }


    protected override IEnumerator MinusCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);

            ReputationManager.인스턴스?.문제방치();
        }
    }

    public override bool CanBeSolvedWith(BaseItem item)
    {
        return false; 
    }

}
