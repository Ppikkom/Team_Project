using System.Collections;
using UnityEngine;

// 6월 16일 수정 : 원래의 Puke는 MonoBehaviour, IInteractable를 상속하지만, 이를 BaseProblem에 상속시킨 뒤 그 BaseProblem를 상속
public class Puke : BaseProblem
{


    public override 상호작용_방식 상호작용_방식 => 상호작용_방식.즉시;


    [SerializeField] private int 사용횟수 = 0;
    private const int 최대사용횟수 = 3;
    private Renderer 내렌더러;

    private void Awake()
    {
        내렌더러 = GetComponent<Renderer>();
        if (내렌더러 == null)
            내렌더러 = GetComponentInChildren<Renderer>();
        GameplayUIController.인스턴스?.킬로그알림추가("누군가 지하철 역에 구토를 했어요!");
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
        if(Coroutine != null)
        {
            StopCoroutine(Coroutine);
            Coroutine = null;
        }
    }

    public override bool CanBeSolvedWith(BaseItem item)
    {
        return item is Mop;
    }

    public override void 상호작용_시작()
    {
        var 플레이어 = FindAnyObjectByType<PlayerController>();
        if (플레이어 == null) return;

        if (!(플레이어.현재_들고있는_아이템 is Mop mop)) return;

        // 사용 불가(더러움, 최대 사용 등) 체크
        if (!mop.사용가능())
        {
            Debug.Log("❌ 대걸레는 더러워져 사용할 수 없습니다. 세척해주세요.");
            return;
        }

        // 쿨타임 체크
        // if (!mop.쿨타임체크())
        // {
        //     Debug.Log("⏳ 대걸레 쿨타임 중...");
        //     return;
        // }

        // 사용 처리
        mop.마지막사용시간 = Time.time;
        mop.현재사용횟수++;
        mop.머티리얼업데이트();

        사용횟수++;

        float 투명도 = Mathf.Clamp01(1f - 0.2f * 사용횟수);
        if (내렌더러 != null)
        {
            Color color = 내렌더러.material.color;
            color.a = 투명도;
            내렌더러.material.color = color;
        }

        Debug.Log($"구토 청소 {사용횟수}/3");

        if (사용횟수 >= 최대사용횟수)
        {
            OnSolved(); // BaseProblem의 해결 처리
        }
    }

}
