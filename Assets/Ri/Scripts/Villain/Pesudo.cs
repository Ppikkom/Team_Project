using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Unity.Behavior;


public class Pesudo : BaseNPC
{
    // 목적지
    private GameObject target;
    [SerializeField] private Vector3 targetRange;
    [SerializeField] LayerMask targetLayer;

    // 이하 임시 변수
    [SerializeField] private float 대기시간 = 10f;
    [SerializeField] private float 회전속도 = 5f; // 회전 속도

    [SerializeField] float 행동_재사용시간 = 10f;
    float curCoolTime = 0f;
    Vector3 NPC방향; // 이 오브젝트가 바라봄.
    Quaternion npcRot;

    Vector3 obj방향;
    Quaternion objRot;

    public override void OnEnable()
    {
        base.OnEnable();
        curCoolTime = 행동_재사용시간;
        if (목적지.Any())
        {
            
            float rnd = Random.Range(0f, 1f);
            if (목적지[0]._name.Contains("상행") || 목적지[0]._name.Contains("하행")) // 지하철 -> 1번 || 6번 출구
            {
                목적지_위치 = 근처위치_반환();

                if (rnd < 0.5f && 목적지[목적지.Count - 1]._name == "출구") ChangeDestination(목적지.Count - 1, 1, "출구");
                else ChangeDestination(목적지.Count - 1, 0, "출구");
            }
            else // 1 || 6번 출구 -> 지하철
            {
                목적지_위치 = 목적지위치_반환(Index);
                if (rnd < 0.5f)
                {
                    ChangeDestination(목적지.Count - 3, 5, "하행");
                    ChangeDestination(목적지.Count - 2, 5, "하행_쓰레기통1");
                    ChangeDestination(목적지.Count - 1, 5, "하행_지하철");
                }
                else
                {
                    ChangeDestination(목적지.Count - 3, 5, "상행");
                    ChangeDestination(목적지.Count - 2, 5, "상행_쓰레기통1");
                    ChangeDestination(목적지.Count - 1, 5, "상행_지하철");
                }
            }

            TransitionTo(NPCState.Move);
            
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
        curCoolTime -= Time.deltaTime;
    }

    public override void 이동_트리거()
    {
        base.이동_트리거();
    }

    public override void 이동_갱신()
    {
        if(target == null)
            navMeshAgent.SetDestination(목적지_위치);
        else
            navMeshAgent.SetDestination(target.transform.position);


        // 이거 조건 잘 생각해야댐
        if (target == null && 도착확인())
        {
            다음_목적지();
            return;
        }


        
        // 이동 완료 조건 - 추적을 따로 넣어야하나.
        if (target != null && (transform.position - target.transform.position).magnitude < navMeshAgent.stoppingDistance)
        {
            navMeshAgent.isStopped = true;
            behaviorAgent.SetVariableValue("Behavior", true);
            return;
        }

    }

    public override void 배회_트리거()
    {
        base.배회_트리거();
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
        
        // 이 불경한자가!!!
        target.GetComponent<BaseNPC>().SetBlackBoardValueBoolean("HasVillianAbility", true);
        // 타겟의 방향 변경
        anim.SetTrigger("isSermon");
        StartCoroutine(SerMonAndAttack());
    }

    public override void 행동_갱신()
    {
        if ((transform.position - target.transform.position).magnitude < navMeshAgent.stoppingDistance)
        {
            navMeshAgent.isStopped = true;
        }
        else navMeshAgent.SetDestination(target.transform.position);
            
        if (도착확인())
        {
            anim.SetBool("isWalking", false);
        }

        // 빌런이 승객을 바라보도록
        NPC방향 = target.transform.position - transform.position;
        NPC방향.y = 0;
        if (NPC방향.sqrMagnitude < 0.0001f) return;

        npcRot = Quaternion.LookRotation(NPC방향);

        transform.rotation = Quaternion.Slerp(transform.rotation, npcRot, Time.deltaTime * 회전속도);

        // 승객이 빌런을 바라보도록
        obj방향 = transform.position - target.transform.position;
        obj방향.y = 0;
        if (obj방향.sqrMagnitude < 0.0001f) return;

        objRot = Quaternion.LookRotation(obj방향);
        target.transform.rotation = Quaternion.Slerp(target.transform.rotation, objRot, Time.deltaTime * 회전속도);

    }

    public override void 행동_종료()
    {
        base.행동_종료();
        // 민원 급증
    }

    IEnumerator SerMonAndAttack()
    {

        yield return new WaitUntil(() =>
        {
            var str = anim.GetCurrentAnimatorStateInfo(0);
            return str.IsName("설교");
        });

        yield return new WaitUntil(() =>
        {
            var str = anim.GetCurrentAnimatorStateInfo(0);
            return str.normalizedTime >= 1f;
        });
        anim.SetBool("isWalking", false);
        anim.SetTrigger("isAssault");
        yield return StartCoroutine(AttackAndWait());

        navMeshAgent.stoppingDistance = 1f;
        behaviorAgent.SetVariableValue("Behavior", false);
        curCoolTime = 행동_재사용시간;
        다음_목적지();
    }

    IEnumerator AttackAndWait()
    {
        yield return new WaitUntil(() =>
        {
            var str = anim.GetCurrentAnimatorStateInfo(0);
            return str.IsName("폭행");
        });

        yield return new WaitUntil(() =>
        {
            var str = anim.GetCurrentAnimatorStateInfo(0);
            return str.normalizedTime >= 0.4f;
        });
        NPCManager.인스턴스.NPC반환(target);
        yield return StartCoroutine(대기());

    }

    public override void 피격(float 피해)
    {
        target.GetComponent<BehaviorGraphAgent>().SetVariableValue("HasVillianAbility", false);
        base.피격(피해);
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

    public override void 멈춤_트리거()
    {
        base.멈춤_트리거();

    }

    public override void 멈춤_종료()
    {
        base.멈춤_종료();
    }

    public override void 처치시()
    {
        //ReputationManager.인스턴스.점수추가(10);
        target.GetComponent<BaseNPC>().SetBlackBoardValueBoolean("HasVillianAbility", false);
        //NPCManager.인스턴스.NPC반환(gameObject);
    }

    private void 목표물_찾기()
    {
        Vector3 center = transform.position + Vector3.up * targetRange.y;
        Collider[] targets = Physics.OverlapBox(center, targetRange, Quaternion.identity, targetLayer);

        if (targets == null || targets.Length == 0 || curCoolTime > 0f)
        {
            behaviorAgent.SetVariableValue("Wander", true);
            목적지_위치 = mapData[현재_층].랜덤_지역좌표();
            return;
        }

        // 목표물이 있을 때
        behaviorAgent.SetVariableValue("Wander", false);
        int rnd = Random.Range(0, targets.Length);
        navMeshAgent.stoppingDistance = 1.5f;
        target = targets[rnd].gameObject;
        목적지_위치 = targets[rnd].transform.position;
    }

    private void 다음_목적지()
    {
        float rnd = Random.Range(0f, 1f);
        if (Index + 1 < 목적지.Count && rnd < 0.65f)
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

    public override void OnDisable()
    {
        base.OnDisable();
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
}