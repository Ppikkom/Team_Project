using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Behavior;
using System.Collections;



public class ThrowUp : MonoBehaviour
{
    // 일정시간 멈추게끔

    [SerializeField] private List<GameObject> _List = new List<GameObject>();
    public IReadOnlyList<GameObject> Passengers => _List;
    [SerializeField] private float freezeDuration = 3f;
    private Collider col;

    void Awake()
    {
        col = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.TryGetComponent<BaseNPC>(out var npc) && npc.NPC종류 == NPCType.Normal)
        {
            var go = other.gameObject;

            if (!_List.Contains(go))
            {
                _List.Add(go);
                StartCoroutine(FreezeNpc(go));
                //go.GetComponent<BehaviorGraphAgent>().SetVariableValue("HasVillianAbility", true);

            }

        }
    }

    // 토사물이 비활성화 되면, 바로 비활성화
    void OnDestroy()
    {
        foreach (var v in _List)
        {
            v.GetComponent<BaseNPC>().SetBlackBoardValueBoolean("HasVillianAbility", false);
        }
    }

    IEnumerator FreezeNpc(GameObject _go)
    {
        _go.GetComponent<BehaviorGraphAgent>().SetVariableValue("HasVillianAbility", true);
        yield return new WaitForSeconds(freezeDuration);
        _List.Remove(_go);
        _go.GetComponent<BehaviorGraphAgent>().SetVariableValue("HasVillianAbility", false);
    }

}