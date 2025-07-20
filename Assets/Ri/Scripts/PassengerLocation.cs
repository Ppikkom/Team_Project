using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Behavior;



public class PasssengerLocation : MonoBehaviour
{
    [SerializeField] private List<GameObject> _List = new List<GameObject>();
    //private List<GameObject> _villainList = new List<GameObject>();
    public IReadOnlyList<GameObject> Passengers => _List;

    private void OnTriggerEnter(Collider other)
    {

        if (other.TryGetComponent<BaseNPC>(out var npc))
        {
            var go = other.gameObject;
            if (npc.NPC종류 == NPCType.Normal) {
                if (!_List.Contains(go)) _List.Add(go);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<BaseNPC>(out var npc))
        {
            var go = other.gameObject;
            if (npc.NPC종류 == NPCType.Normal)
            {
                _List.Remove(go);
                //go.GetComponent<BehaviorGraphAgent>().SetVariableValue("Behavior", false);
            }
                
        }
    }

    public void SetBehaviorAgentBool(string name, bool b)
    {
        foreach (var v in _List)
        {
            v.GetComponent<BehaviorGraphAgent>().SetVariableValue(name, b);
        }
    }
}