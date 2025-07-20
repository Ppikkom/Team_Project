using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Trash))] 
public class TrashProblem : BaseProblem
{
    private Trash trash; 

    protected override void Start()
    {
        base.Start();
        gameObject.name = "쓰레기";
        problemType = ProblemType.Trash;

        trash = GetComponent<Trash>();
    }

    protected override IEnumerator MinusCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);

            ReputationManager.인스턴스?.문제방치();
        }
    }

    protected override void PlusCoroutine()
    {
        if (ReputationManager.인스턴스 != null)
        {
            ReputationManager.인스턴스.점수추가(10);
            Debug.Log("+");
        }
    }

    public override bool CanBeSolvedWith(BaseItem item)
    {
        return false;
    }
}
