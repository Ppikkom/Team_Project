using UnityEngine;

public class NPCExitTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<BaseNPC>(out var npc))
        {
            var go = other.gameObject;
            if (npc.Index == npc.목적지.Count - 1 && npc.목적지[npc.Index]._name.Contains("출구"))
            {
                npc.컴포넌트_비활성화();
                NPCManager.인스턴스.NPC반환(go);
            }
        }
    }
}
