using UnityEngine;

public class MoveState : INPCState
{
    public void OnEnter(BaseNPC npc)
    {
        npc.이동_트리거();
    }

    public void Execute(BaseNPC npc)
    {
        npc.이동_갱신();
    }

    public void OnExit(BaseNPC npc)
    {
        npc.이동_종료();
    }
}