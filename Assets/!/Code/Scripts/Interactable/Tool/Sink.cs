using UnityEngine;

public class Sink : MonoBehaviour, IInteractable
{
    public 상호작용_타입 상호작용_종류 => 상호작용_타입.일반상호작용;
    public 상호작용_방식 상호작용_방식 => 상호작용_방식.즉시;

    public void 상호작용_시작()
    {
        var 플레이어 = FindAnyObjectByType<PlayerController>();

        if (플레이어 == null)
        {
            Debug.LogError("❗ 플레이어를 찾을 수 없습니다.");
            return;
        }

        if (플레이어.현재_들고있는_아이템 == null)
        {
            Debug.Log("❗ 플레이어가 현재 들고 있는 아이템이 없습니다.");
            return;
        }

        // 플레이어가 들고 있는 아이템이 BaseCleaningTool인지 확인
        if (플레이어.현재_들고있는_아이템 is BaseCleaningTool 청소도구)
        {
            // 더러워진 상태라면 세척
            if (청소도구.더러워졌는가)
            {
                청소도구.세척();
                Debug.Log($"💧 {청소도구.name}이(가) 세척되었습니다.");
            }
            else
            {
                Debug.Log($"ℹ️ {청소도구.name}은(는) 이미 깨끗합니다.");
            }
        }
        else
        {
            Debug.Log("💧 세척 가능한 청소 도구가 필요합니다.");
        }
    }

    public void 상호작용_유지(float 유지시간) { }
    public void 상호작용_종료() { }
}