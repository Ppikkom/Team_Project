using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Leaflet))]
public class LeafletProblemB : BaseProblem
{
    private Leaflet leaflet;

    protected override void Start()
    {
        base.Start();
        gameObject.name = "전단지";
        problemType = ProblemType.Leaflet;

        leaflet = GetComponent<Leaflet>();
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
        }
    }

    public override bool CanBeSolvedWith(BaseItem item)
    {
        return false;
    }
}
