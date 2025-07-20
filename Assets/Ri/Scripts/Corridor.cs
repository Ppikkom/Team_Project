using System.Collections.Generic;
using UnityEngine;
using Unity.Behavior;
using Unity.VisualScripting;
using Unity.AI.Navigation;
using UnityEngine.AI;

[RequireComponent(typeof(Collider))]
public class Corridor : MonoBehaviour
{
    [SerializeField] private Queue<BehaviorGraphAgent> _waitQueue = new();
    private BehaviorGraphAgent _currentAgent;
    private NavMeshModifier _navModifier;
    private Collider col;

    [SerializeField] private string _areaname;
    [SerializeField] private float defaultCost = 1f;
    [SerializeField] private float blockedCost = 2f;

    private int areaIndex;
    void Awake()
    {
        _navModifier = GetComponent<NavMeshModifier>();
        col = GetComponent<Collider>();
        areaIndex = NavMesh.GetAreaFromName(_areaname);
        if (areaIndex < 0) return;
    }

    private void OnTriggerEnter(Collider other)
    {
        var agent = other.GetComponent<BehaviorGraphAgent>();
        if (agent == null) return;

        if (_currentAgent == null)
        {
            var navAgent = agent.GetComponent<NavMeshAgent>();
            // 비어 있으면 바로 통과 허가
            _currentAgent = agent;
            agent.SetVariableValue("CanCorridorMove", true);
            var npc = agent.GetComponent<BaseNPC>();
            if(npc.NPC종류 == NPCType.Normal) 
                npc.목적지_위치 = npc.목적지위치_반환(npc.Index);


            
            NavMesh.SetAreaCost(areaIndex, blockedCost); // 전역 설정
            navAgent.SetAreaCost(areaIndex, defaultCost); 
            //other.gameObject.GetComponent<NavMeshAgent>().SetAreaCost(areaIndex, defaultCost); // 본인 예외
        }
        else
        {
            // 이미 누군가 통과 중이면 대기열에 등록
            _waitQueue.Enqueue(agent);
            var npc = agent.GetComponent<BaseNPC>();
            if(npc.NPC종류 == NPCType.Normal) 
                npc.목적지_위치 = npc.목적지위치_반환(npc.Index);
            agent.SetVariableValue("CanCorridorMove", false);
        }
    }

    void Update()
    {
        
        if (_currentAgent == null) return;
        while (_waitQueue.Count > 0)
        {
            var cur = _currentAgent;
            cur.SetVariableValue("CanCorridorMove", true);
            if (!col.bounds.Contains(cur.transform.position))
            {
                _currentAgent = _waitQueue.Dequeue();
                _currentAgent.SetVariableValue("CanCorridorMove", true);
                var npc = _currentAgent.GetComponent<BaseNPC>();
                if(npc.NPC종류 == NPCType.Normal) 
                npc.목적지_위치 = npc.목적지위치_반환(npc.Index);

                var nextNav = _currentAgent.GetComponent<NavMeshAgent>();
                nextNav.SetAreaCost(areaIndex, defaultCost);
            }

            else
                break;
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        var agent = other.GetComponent<BehaviorGraphAgent>();
        if (agent == null || agent != _currentAgent) return;

        if (_waitQueue.Count > 0)
        {
            _currentAgent = _waitQueue.Dequeue();
            _currentAgent.SetVariableValue("CanCorridorMove", true);
            var npc = _currentAgent.GetComponent<BaseNPC>();
            if(npc.NPC종류 == NPCType.Normal) 
                npc.목적지_위치 = npc.목적지위치_반환(npc.Index);

            var nextNav = _currentAgent.GetComponent<NavMeshAgent>();
            nextNav.SetAreaCost(areaIndex, defaultCost);
        }
        else
        {
            _currentAgent.SetVariableValue("CanCorridorMove", true);
            _currentAgent = null;
            NavMesh.SetAreaCost(areaIndex, defaultCost);
            //Debug.Log($"areaIndex : {NavMesh.GetAreaCost(areaIndex)}");
        }

    }
}
