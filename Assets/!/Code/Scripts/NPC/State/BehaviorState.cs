using UnityEngine;
using Unity.Behavior;
public class BehaviorState : INPCState
{
    float Timer;
    public void OnEnter(BaseNPC npc)
    {
        npc.행동_트리거();
    }

    public void Execute(BaseNPC npc)
    {
        npc.행동_갱신();
    }

    public void OnExit(BaseNPC npc)
    {
        npc.행동_종료();
    }
}