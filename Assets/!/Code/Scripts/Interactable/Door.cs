using UnityEngine;
public class Door : MonoBehaviour, IInteractable
{
    public 상호작용_타입 상호작용_종류 => 상호작용_타입.일반상호작용;
    public 상호작용_방식 상호작용_방식 => 상호작용_방식.즉시;

    private bool 열린상태 = false;
    public Collider mainCollider; // 충돌용
    public Collider interactionCollider; // 상호작용 감지용

    void Awake()
    {
        // 두 콜라이더를 구분해서 가져오기 (Tag, 이름, 순서 등으로 구분)
        var colliders = GetComponents<Collider>();
        foreach (var col in colliders)
        {
            if (col.isTrigger)
                interactionCollider = col;
            else
                mainCollider = col;
        }
        if (mainCollider == null || interactionCollider == null)
            Debug.LogWarning("문에 필요한 Collider가 모두 추가되어 있는지 확인하세요!");
    }

    public void 상호작용_시작()
    {
        열린상태 = !열린상태;
        if (mainCollider != null)
            mainCollider.isTrigger = 열린상태; // 열리면 트리거, 닫히면 충돌

        Debug.Log(열린상태 ? "문이 열렸습니다" : "문이 닫혔습니다");
    }


    public void 상호작용_유지(float 유지시간) { }
    public void 상호작용_종료() { }
}
