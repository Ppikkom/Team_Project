using System.Collections;
using UnityEngine;

// 전단지 프리펩 ProblemManager 등록용 스크립트
// BaseItem을 상속받는 전단지(Leaftlet.cs) 프리팹을 해당 스크립트와 연결된 빈 오브젝트에 연결해서 사용
public class LeafletProblem : BaseProblem
{
    [Header("스폰될 리플렛 프리팹")]
    [SerializeField] private GameObject leafletPrefab;

    private Leaflet leafletInstance;

    public override void Initialize(ProblemConfig config)
    {
        base.Initialize(config); // 위치, 타입 초기화

        if (leafletPrefab != null && leafletInstance == null)
        {
            GameObject leafletGO = Instantiate(leafletPrefab, transform);
            leafletInstance = leafletGO.GetComponent<Leaflet>();
            if (Coroutine == null)
            {
                Coroutine = StartCoroutine(MinusCoroutine());
            }
        }
    }

    public override bool CanBeSolvedWith(BaseItem item)
    {
        return false;
    }

    protected override void PlusCoroutine()
    {
        if (ReputationManager.인스턴스 != null)
        {
            ReputationManager.인스턴스.점수추가(10);
            Debug.Log("+");
        }
    }


    public override void 상호작용_시작() { }

    private void Update()
    {
        // 리플렛이 제거되었으면 문제 해결로 간주
        if (leafletInstance == null || !leafletInstance.gameObject.activeInHierarchy)
        {
            if (Coroutine != null)
            {
                StopCoroutine(Coroutine);
                Coroutine = null;
            }

            OnSolved();
        }
    }
}
