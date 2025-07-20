using UnityEngine;

public abstract class BaseCleaningTool : BaseItem
{
    [SerializeField] public int 최대사용횟수 = 10;
    [SerializeField] public int 현재사용횟수 = 0;

    public bool 더러워졌는가 => 현재사용횟수 >= 최대사용횟수;

    public void 세척()
    {
        현재사용횟수 = 0;
        Debug.Log($"🧽 {아이템_이름}이(가) 세척되어 깨끗해졌습니다!");
    }

    public bool 사용가능()
    {
        if (더러워졌는가)
        {
            Debug.Log($"❌ {아이템_이름}은(는) 더러워져서 사용할 수 없습니다.");
            return false;
        }
        return true;
    }
}