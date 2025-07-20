public interface IInteractable
{
    상호작용_타입 상호작용_종류 { get; }
    상호작용_방식 상호작용_방식 { get; }
    void 상호작용_시작();
    void 상호작용_유지(float 유지시간); // 누르고 있을 때만 사용
    void 상호작용_종료(); // 누르기 끝났을 때만 사용
}
public enum 상호작용_타입
{
    일반상호작용,
    아이템획득,
    획득불가아이템
}
public enum 상호작용_방식
{
    즉시,
    누르고_있기
}