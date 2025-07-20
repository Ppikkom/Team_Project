using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
// 한 번만에 쓰레기 투기 맥스

public class IillegalDumping : BaseNPC
{

    [SerializeField] float 행동_재사용시간;
    float 대기시간 = 10;
    float coolTime = 0;
    bool 투기_트리거 = false;
    Collider target;

    [SerializeField] private LayerMask mapLayer;
    [SerializeField] private float maxDistance = 3f;

    // 그냥 큰거1번이면 어떨까

    // 내부에 ProblemType 추가
    public override ProblemType problemType => ProblemType.Villain;

    public override void Awake()
    {
        // base에 State 초기화 있음.
        base.Awake();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        
        
        // NPCState를 초기화 및 사용하기 위해 선언.
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


    public override void Update()
    {
        // base에 상태가 전이되었을 때 바꾸는 내용이 담겨있음.
        base.Update();

        coolTime -= Time.deltaTime;
    }

    public override void 이동_트리거()
    {
        base.이동_트리거();
    }

    public override void 이동_갱신()
    {
         // 이 상태가 유지될 때마다 호출
        navMeshAgent.SetDestination(목적지_위치);
        
        // 목적지에 도착했는 지 확인.
        if (도착확인())
        {
            if (투기_트리거 == false && 목적지[Index]._name.Contains("쓰레기통"))
            {
                behaviorAgent.SetVariableValue("Behavior", true);
                return;
            }
            다음_상태();
        }

    }

    public override void 멈춤_트리거()
    {
        base.멈춤_트리거();

    }

    public override void 멈춤_갱신()
    {
        GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
    }

    public override void 멈춤_종료()
    {
        base.멈춤_종료();
    }
    public override void 배회_트리거()
    {
        base.배회_트리거();
        anim.SetBool("isWalking", true);
    }

    public override void 배회_갱신()
    {
        navMeshAgent.SetDestination(목적지_위치);

        if (도착확인())
        {
            다음_상태();
        }
    }

    public override void 배회_종료()
    {

    }

    public override void 행동_트리거()
    {
        anim.SetBool("isWalking", true);
        base.행동_트리거();
        if (!대사표시중)
        {
            //StartCoroutine(대사표시("이 쓰레기통에 불을 붙이면..."));
        }
        ProblemManager.인스턴스.RegisterVillainNPC(this);
        navMeshAgent.isStopped = true; // 행동을 할 때, 멈춤.
        anim.SetTrigger("isThrow");
        StartCoroutine(ThrowAndWait());
    }

    public override void 행동_갱신()
    {

    }

    public override void 행동_종료()
    {
        navMeshAgent.isStopped = false;
        navMeshAgent.stoppingDistance = 1f;
    }

    IEnumerator ThrowAndWait()
    {
        yield return new WaitUntil(() =>
        {
            var str = anim.GetCurrentAnimatorStateInfo(0);
            return str.normalizedTime >= 1f;
        });

        if (목적지[Index]._name.Contains("쓰레기통"))
        {
            target.transform.parent.GetComponentInChildren<TrashCan>().AddFullStack();
        }

        yield return StartCoroutine(대기());

        behaviorAgent.SetVariableValue("Behavior", false);
        투기_트리거 = true;
        다음_상태();
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

    public override void 처치시()
    {
        //ReputationManager.인스턴스.점수추가(10);
        //NPCManager.인스턴스.NPC반환(gameObject);
    }


    private void 다음_상태()
    {
        float rnd = Random.Range(0f, 1f);
        if (rnd < 0.3f && 투기_트리거 == false) // Wander 조건
        {
            behaviorAgent.SetVariableValue("Wander", true);
            목적지_위치 = 랜덤좌표(true); // 맵 확인하고, 그 맵의 랜덤 위치를 목적지 설정
        }
        else if (rnd < 0.7 && coolTime <= 0f && 투기_트리거 == false) // 행동 조건  && 투기_트리거 == false
        {

            navMeshAgent.stoppingDistance = 1.5f; // 물체에 붙지 않게 설정
            behaviorAgent.SetVariableValue("Wander", false);

            foreach (var v in mapData[현재_층].mapData)
            {
                if (v.이름.Contains("쓰레기통"))
                {
                    목적지_위치 = RandomPoint(v.collider);
                    target = v.collider;
                    return;
                }
            }

            var 쓰레기통위치 = mapData.SelectMany(m => m.mapData).Where(z => z.이름.Contains("쓰레기통")).ToArray();
            var chosen = 쓰레기통위치[Random.Range(0, 쓰레기통위치.Length)];
            target = chosen.collider;
            목적지_위치 = RandomPoint(chosen.collider);

            // 나머지 주석
            // 목적지_위치 = 목적지위치_반환(Index);
        }
        
        다음_목적지();
    }

    private void 다음_목적지()
    {
        if (투기_트리거 == true)
        {
            Index = 목적지.Count - 2;
            목적지_위치 = 목적지위치_반환(Index);
            behaviorAgent.SetVariableValue("Wander", false);
        }
        if (Index + 1 < 목적지.Count)
        {
            Index++;
            목적지_위치 = 목적지위치_반환(Index);
            behaviorAgent.SetVariableValue("Wander", false);
        }
        else
        {
            목적지_위치 = 랜덤좌표();
            behaviorAgent.SetVariableValue("Wander", true);
        }
    }

    // 해당 층 랜덤 좌표 / true이면 현재 층에서 랜덤 좌표
    private Vector3 랜덤좌표(bool flag = false)
    {
        Collider gateC;
        if (flag == false)
            현재_층 = Random.Range(0, 6);
            
        gateC = mapData[현재_층].mapData[Random.Range(0, mapData[현재_층].mapData.Length)].collider;
            
        
        return RandomPoint(gateC);
    }

    private bool 도착확인() => 거리비교() <= navMeshAgent.stoppingDistance;
    

    // 현재 목적지와 이 오브젝트의 거리를 비교하여 리턴하는 함수
    private float 거리비교()
    {
        Vector3 targetPosition = 목적지_위치;
        Vector3 agentPosition = transform.position;
        //Debug.Log(targetPosition + " " + agentPosition);
        agentPosition.y = targetPosition.y; // 이거 문제 생길 수도 있을듯
        return Vector3.Distance(agentPosition, targetPosition);
    }

    // base에 StopAllCoroitine이 작성되어있음.
    public override void OnDisable()
    {
        base.OnDisable();
    }
}