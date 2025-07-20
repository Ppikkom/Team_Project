using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Unity.Behavior;



public class Rogue : BaseNPC
{
    // 목적지
    private GameObject target;
    [SerializeField] private Vector3 targetRange;
    [SerializeField] LayerMask targetLayer;

    // 이하 임시 변수
    [SerializeField] private float 대기시간 = 5f;
    int rogueCnt = 0;

    // 내부에 ProblemType 추가
    public override ProblemType problemType => ProblemType.Villain;

    public override void OnEnable()
    {
        base.OnEnable();
        // 목적지가 없는 상황은 고려하지 않음.

        // 스폰

        // 시작 상태 설정
        if (목적지.Any())
        {
            TransitionTo(NPCState.Move);
            목적지_위치 = 목적지위치_반환(Index);
        }
        else
        {
            behaviorAgent.SetVariableValue("Stop", true);
        }
    }

    public override void Awake()
    {
        base.Awake();
    }

    public override void Update()
    {
        base.Update();
    }

    public override void 이동_트리거()
    {
        base.이동_트리거();
    }

    public override void 이동_갱신()
    {
        if (target == null)
            navMeshAgent.SetDestination(목적지_위치);
        else if (target != null)
            navMeshAgent.SetDestination(target.transform.position);

        if (target == null || 도착확인())
        {
            다음_목적지();
            return;
        }

        if (target != null || (transform.position - target.transform.position).magnitude < navMeshAgent.stoppingDistance)
        {
            behaviorAgent.SetVariableValue("Behavior", true);
            return;
        }

    }

    public override void 배회_트리거()
    {
        base.배회_트리거();
        목적지_위치 = mapData[Random.Range(0, mapData.Length)].랜덤_지역좌표();
    }

    public override void 배회_갱신()
    {
        navMeshAgent.SetDestination(목적지_위치);

        if (도착확인())
        {
            다음_목적지();
        }
    }

    public override void 배회_종료()
    {
        
    }

    public override void 멈춤_트리거()
    {
        base.멈춤_트리거();

    }

    public override void 멈춤_종료()
    {
        base.멈춤_종료();
    }

    public override void 행동_트리거()
    {
        base.행동_트리거();
        target.GetComponent<BehaviorGraphAgent>().SetVariableValue("HasVillianAbility", true);
        ProblemManager.인스턴스.RegisterVillainNPC(this);
        anim.SetTrigger("isStealing");
        StartCoroutine(StealAndWait());
    }

    public override void 행동_갱신()
    {
        Vector3 NPC방향 = target.transform.position - transform.position;
        NPC방향.y = 0;
        if (NPC방향.sqrMagnitude < 0.0001f) return;

        Quaternion npcRot = Quaternion.LookRotation(NPC방향);
        transform.rotation = Quaternion.Slerp(transform.rotation, npcRot, Time.deltaTime * 2f);

    }

    public override void 행동_종료()
    {
        base.행동_종료();
    }

    IEnumerator StealAndWait()
    {
        yield return new WaitUntil(() =>
        {
            var str = anim.GetCurrentAnimatorStateInfo(0);
            return str.IsName("도둑질");
        });
        yield return new WaitUntil(() =>
        {
            var str = anim.GetCurrentAnimatorStateInfo(0);
            return str.normalizedTime >= 1f;
        });
        rogueCnt++;
        yield return StartCoroutine(대기());

        target.GetComponent<BehaviorGraphAgent>().SetVariableValue("HasVillianAbility", false);
        behaviorAgent.SetVariableValue("Behavior", false);
        navMeshAgent.isStopped = false;
        다음_목적지();
    }

    IEnumerator 대기()
    {
        float timer = 0;
        while (timer < 대기시간)
        {
            timer += Time.deltaTime;
            yield return null;
        }
    }

    public override void 피격(float 피해)
    {
        target.GetComponent<BehaviorGraphAgent>().SetVariableValue("HasVillianAbility", false);
        base.피격(피해);
    }

    public override void 처치시()
    {
        //ReputationManager.인스턴스.점수추가(10);

        //NPCManager.인스턴스.NPC반환(gameObject);
    }

    private void 목표물_찾기()
    {
        Vector3 center = transform.position + Vector3.up * targetRange.y;
        Collider[] targets = Physics.OverlapBox(center, targetRange, Quaternion.identity, targetLayer);

        // 목표물이 없으면 배회 상태로 전이
        if (targets == null || targets.Length == 0)
        {
            Debug.LogWarning("목표물이 없습니다. 배회 상태로 변경합니다.");
            behaviorAgent.SetVariableValue("Wander", true);
            return;
        }

        behaviorAgent.SetVariableValue("Wander", false);
        int rnd = Random.Range(0, targets.Length);
        target = targets[rnd].gameObject;
        목적지_위치 = targets[rnd].transform.position;
    }

    private void 다음_목적지()
    {
        if (rogueCnt >= 3)
        {
            // 그대로 사라지게?
            NPCManager.인스턴스.NPC반환(gameObject);
        }

        float rnd = Random.Range(0f, 1f);

        if (Index + 1 < 목적지.Count && rnd < 0.5f)
        {
            Index++;
            목적지_위치 = 목적지위치_반환(Index);
            behaviorAgent.SetVariableValue("Wander", false);
            behaviorAgent.SetVariableValue("Behavior", false);
        }
        else
        {
            목표물_찾기();
            behaviorAgent.SetVariableValue("Behavior", false);
        }
    }


    private bool 도착확인() => GetDistanceToWaypoint() <= navMeshAgent.stoppingDistance;

    private float GetDistanceToWaypoint()
    {
        Vector3 targetPosition = 목적지_위치;
        Vector3 agentPosition = transform.position;
        //Debug.Log(targetPosition + " " + agentPosition);
        agentPosition.y = targetPosition.y; // 이거 문제 생길 수도 있을듯
        return Vector3.Distance(agentPosition, targetPosition);
    }
        
    #if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);
        Gizmos.color = Color.cyan;
        var col = GetComponent<Collider>();
        Vector3 halfExtents = targetRange;
        Vector3 boxCenter = new Vector3(col.bounds.center.x, col.bounds.min.y, col.bounds.center.z) + Vector3.up * halfExtents.y; // 이거 점점 내려가는 이유?
        //Vector3 pos = transform.position + new Vector3(0, targetRange.y, 0) - new Vector3(0, transform.localScale.y * 2, 0);
        //transform.position - new Vector3(0, transform.position.y / 2, 0); // 
        Vector3 size = halfExtents;
        Gizmos.DrawWireCube(boxCenter, size * 2);
    }
#endif

    public override void OnDisable()
    {
        base.OnDisable();
    }
}