public interface INPCState
{
    void OnEnter(BaseNPC npc);    // 상태 진입 시 1회 호출
    void Execute(BaseNPC npc);    // 매 프레임(혹은 의도한 타이밍) 호출
    void OnExit(BaseNPC npc);     // 상태 이탈 시 1회 호출
}