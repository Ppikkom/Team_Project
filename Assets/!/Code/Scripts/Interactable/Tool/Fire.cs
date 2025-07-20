using UnityEngine;

public class Fire : BaseProblem
{
    private bool isBeingExtinguished = false;
    private float extinguishTimer = 0f;
    private const float extinguishTime = 3f;

    public override 상호작용_타입 상호작용_종류 => 상호작용_타입.일반상호작용;
    public override 상호작용_방식 상호작용_방식 => 상호작용_방식.누르고_있기;

    public override void 상호작용_시작()
    {
        // 소화기를 들고 있는지 확인
        var 플레이어 = Object.FindAnyObjectByType<PlayerController>();
        if (플레이어 != null && 플레이어.현재_들고있는_아이템 is FireExtinguisher)
        {
            isBeingExtinguished = true;
            extinguishTimer = 0f;
        }
        else
        {
            isBeingExtinguished = false;
        }
    }

    public override void 상호작용_유지(float 유지시간)
    {
        if (!isBeingExtinguished) return;

        extinguishTimer += 유지시간;
        Debug.Log($"🔥 소화 중... {extinguishTimer:F2} / {extinguishTime}초");

        if (extinguishTimer >= extinguishTime)
        {
            Destroy(gameObject); // 3초 후 fire 오브젝트 삭제
        }
    }

    public override void 상호작용_종료()
    {
        isBeingExtinguished = false;
        extinguishTimer = 0f;
    }
}
