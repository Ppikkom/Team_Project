using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Unity.Mathematics;

public class TimeStop : BaseNPC
{
    // 목적지
    private GameObject target;
    [SerializeField] private Vector3 targetRange;
    [SerializeField] LayerMask targetLayer;

    // 이하 임시 변수
    [SerializeField] private float 대기시간 = 10f;

    // 내부에 ProblemType 추가
    public override ProblemType problemType => ProblemType.Villain;
    public override void OnEnable()
    {
        base.OnEnable();
        // 목적지가 없는 상황은 고려하지 않음.
        // 시작 상태 설정
        if (목적지.Any())
        {
            TransitionTo(NPCState.Move); // 이거 없어도 기본값이 Move라서 상관없을드드듯?
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
        // 지하 1층까지 도달한 후,
        // 해당 층 혹은 일정한 범위 안에 있는 NPC 랜덤으로 쫒아감.
        // OverlapBox로 일반 NPC찾기
        // 랜덤으로 NPC 설정 // 목적지

        base.이동_트리거();

    }

    public override void 이동_갱신()
    {
        navMeshAgent.SetDestination(목적지_위치);


        // 이거 조건 잘 생각해야댐
        if (target == null || 도착확인())
        {
            다음_목적지();
        }

        // 이동 완료 조건 - 추적을 따로 넣어야하나.
        if (target != null || (transform.position - target.transform.position).magnitude < navMeshAgent.stoppingDistance)
        {
            navMeshAgent.isStopped = true;
            behaviorAgent.SetVariableValue("Behavior", true);
            return;
        }
    }

    public override void 멈춤_트리거()
    {
        base.멈춤_트리거();
    }

    public override void 멈춤_종료()
    {
        base.멈춤_종료();
    }

    public override void 배회_트리거()
    {
        // 장소 랜덤 // 
        base.배회_트리거();
        목적지_위치 = mapData[UnityEngine.Random.Range(0, mapData.Length)].랜덤_지역좌표();
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

    public override void 행동_트리거()
    {
        // 대사 출력
        // 타이무... 스토푸!!!

        // 일정 시간 지나면 설정 범위만큼 정지.

        // 행동 시작하자마자 경고 호출
        base.행동_트리거();
        StartCoroutine(대기());
        ProblemManager.인스턴스.RegisterVillainNPC(this);
    }

    public override void 행동_갱신()
    {

    }

    public override void 행동_종료()
    {
        // 사라짐
        NPCManager.인스턴스.NPC반환(gameObject);
    }

    IEnumerator 대기()
    {
        float timer = 0;
        while (timer < 대기시간)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        // 
        Vector3 pos = transform.position + new Vector3(0, targetRange.y, 0) - new Vector3(0, transform.position.y, 0);
        Collider[] targets = Physics.OverlapBox(pos, targetRange, Quaternion.identity, targetLayer);
        foreach (var v in targets)
        {
            v.gameObject.GetComponent<BaseNPC>().SetBlackBoardValueBoolean("HasVillianAbility", true);
        }
        behaviorAgent.SetVariableValue("Behavior", false);
    }

    public override void 처치시()
    {
        //ReputationManager.인스턴스.점수추가(10);

        //NPCManager.인스턴스.NPC반환(gameObject);
    }

    private void 목표물_찾기()
    {
        Vector3 pos = transform.position + new Vector3(0, targetRange.y, 0) - new Vector3(0, transform.position.y, 0);
        Collider[] targets = Physics.OverlapBox(pos, targetRange, Quaternion.identity, targetLayer);

        // 목표물이 없으면 배회 상태로 전이
        if (targets == null || targets.Length == 0)
        {
            Debug.LogWarning("목표물이 없습니다. 배회 상태로 변경합니다.");
            behaviorAgent.SetVariableValue("Wander", true);
            return;
        }

        behaviorAgent.SetVariableValue("Wander", false);
        int rnd = UnityEngine.Random.Range(0, targets.Length);
        target = targets[rnd].gameObject;
        목적지_위치 = targets[rnd].transform.position;
    }

    private void 다음_목적지()
    {
        if (Index + 1 < 목적지.Count)
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

        Gizmos.color = Color.cyan;
        Vector3 halfExtents = targetRange;
        Vector3 pos = transform.position + new Vector3(0, halfExtents.y, 0) - new Vector3(0, transform.position.y / 2, 0);
        Vector3 size = halfExtents * 2f;
        Gizmos.DrawWireCube(pos, size);
    }
#endif

    public override void OnDisable()
    {
        base.OnDisable();
    }
}
