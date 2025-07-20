using UnityEngine;

public class StopState : INPCState
{
    public void OnEnter(BaseNPC npc)
    {
        Debug.Log("Stop 상태 입장");
        npc.멈춤_트리거();
    }

    public void Execute(BaseNPC npc)
    {
        npc.멈춤_갱신();
    }

    public void OnExit(BaseNPC npc)
    {
        npc.멈춤_종료();
    }
}