using System.Collections;
using UnityEngine;

public class Graffiti : BaseProblem
{
    [SerializeField] private int 사용횟수 = 0;
    private const int 최대사용횟수 = 3;
    private Renderer 내렌더러;

    [SerializeField] private GameObject[] 랜덤그래피티오브젝트들;

    // 6월 18일 추가 : 감점 코루틴 추가
    //private Coroutine Coroutine;

    private void Awake()
    {
        내렌더러 = GetComponent<Renderer>();
        if (내렌더러 == null)
            내렌더러 = GetComponentInChildren<Renderer>();

        // 랜덤 자식 오브젝트 활성화
        if (랜덤그래피티오브젝트들 != null && 랜덤그래피티오브젝트들.Length > 0)
        {
            // 모두 비활성화
            foreach (var obj in 랜덤그래피티오브젝트들)
                if (obj != null) obj.SetActive(false);

            // 랜덤 하나만 활성화
            int idx = Random.Range(0, 랜덤그래피티오브젝트들.Length);
            if (랜덤그래피티오브젝트들[idx] != null)
                랜덤그래피티오브젝트들[idx].SetActive(true);
        }
    }


    protected override void OnEnable()
    {
        base.OnEnable(); // 부모 OnEnable 실행 (선택)

        if (Coroutine == null)
        {
            Coroutine = StartCoroutine(MinusCoroutine());
        }
    }

    private void OnDestroy()
    {
        if (Coroutine != null)
        {
            StopCoroutine(Coroutine);
            Coroutine = null;
        }

        //if (ReputationManager.인스턴스 != null)
        //{
        //    ReputationManager.인스턴스.점수추가(20);
        //}
    }

    //IEnumerator MinusCoroutine()
    //{
    //    while (true)
    //    {
    //        yield return new WaitForSeconds(10f);
    //        if (ReputationManager.인스턴스 != null)
    //        {
    //            ReputationManager.인스턴스.문제방치();
    //            Debug.Log("그래피티로 인한 평판 -1");
    //        }
    //    }
    //}

    public override bool CanBeSolvedWith(BaseItem item)
    {
        return item is WaterMop;
    }

    public override void 상호작용_시작()
    {
        var 플레이어 = Object.FindAnyObjectByType<PlayerController>();
        if (플레이어 == null) return;

        if (플레이어.현재_들고있는_아이템 is WaterMop)
        {
            사용횟수++;

            float 투명도 = Mathf.Clamp01(1f - 0.2f * 사용횟수);
            Color color = 내렌더러.material.color;
            color.a = 투명도;
            내렌더러.material.color = color;

            Debug.Log($"낙서 청소 {사용횟수}/3");

            if (사용횟수 >= 최대사용횟수)
            {
                OnSolved(); // BaseProblem의 해결 처리
                Debug.Log("낙서 제거 완료!");
            }
        }
        else
        {
            Debug.Log("물걸레로만 낙서를 닦을 수 있습니다.");
        }
        //if (사용횟수 >= 최대사용횟수)
        //{
        //    OnSolved(); // BaseProblem의 해결 처리
        //}
    }
}
