using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;


public class TrainTrigger : MonoBehaviour
{
    [SerializeField] List<GameObject> _List = new List<GameObject>();
    public IReadOnlyList<GameObject> Passengers => _List;
    
    void OnTriggerEnter(Collider col)
    {
        if (col.TryGetComponent<BaseNPC>(out var npc))
        {
            var go = col.gameObject;

            if (!_List.Contains(go))
            {
                _List.Add(go);
            }

        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.TryGetComponent<BaseNPC>(out var npc))
        {
            var go = col.gameObject;

            if (_List.Contains(go))
            {
                _List.Remove(go);
            }

        }
    }

    public void 컴포넌트_비활성화()
    {
        if (_List.Count == 0) return;
        for (int i = _List.Count - 1; i >= 0; i--)
        {
            var v = _List[i];
            v.gameObject.SetActive(false);
            _List.RemoveAt(i);
        }
    }
}