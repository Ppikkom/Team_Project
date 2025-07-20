using UnityEngine;

public class WanderState : INPCState
{
    public void OnEnter(BaseNPC npc)
    {
        npc.배회_트리거();
    }

    public void Execute(BaseNPC npc)
    {
        npc.배회_갱신();
    }

    public void OnExit(BaseNPC npc)
    {
        npc.배회_종료();
    }
}