using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;



public class Drill : BaseNPC
{


    float coolTime = 0f;
    [SerializeField] private float 대기시간 = 10f;
    [SerializeField] private float 행동_재사용시간 = 10f;
    [SerializeField] private GameObject 설치물;

    // 이하 임시 변수

    // 내부에 ProblemType 추가
    public override ProblemType problemType => ProblemType.Villain;
    public override void OnEnable()
    {
        base.OnEnable();
        // 시작 상태 설정
        if (목적지.Any())
        {
            TransitionTo(NPCState.Move);
            목적지_위치 = 목적지위치_반환(Index);
        }
        else
        {
            TransitionTo(NPCState.Stop);
        }
    }

    public override void Awake()
    {
        base.Awake();
    }

    public override void Update()
    {
        base.Update();
        coolTime -= Time.deltaTime;
    }

    public override void 이동_트리거()
    {
        base.이동_트리거();
    }

    public override void 이동_갱신()
    {
        if (목적지 == null || navMeshAgent == null || Index >= 목적지.Count)
        {
            Debug.Log("목적지 | navMeshAgent 가 없어 이동을 중단합니다.");
            TransitionTo(NPCState.Stop);
            return;
        }

        navMeshAgent.SetDestination(목적지_위치);

        if (도착확인()) // 목적지에 도달했는지 확인
        {
            if (coolTime <= 0f)
            {
                behaviorAgent.SetVariableValue("Behavior", true);
                return;
            }

            float rnd = Random.Range(0f, 1f);
            if (rnd < 0.5f) // Wander 조건
            {
                behaviorAgent.SetVariableValue("Wander", true);
                목적지_위치 = 목적지위치_반환(Index); // 맵 확인하고, 그 맵의 랜덤 위치를 목적지 설정
                return;
            }
            

            다음_목적지();

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
        base.배회_트리거();
    }

    public override void 배회_갱신()
    {
        // 최종 목적지에 도착하고, 도착할 시 일정확률로 행동 발동.
        navMeshAgent.SetDestination(목적지_위치);

        if (도착확인())
        {
            float rnd = Random.Range(0f, 1f);

            if (coolTime <= 0) // 행동 조건
            {
                behaviorAgent.SetVariableValue("Behavior", true);
                return;
            }

            if (rnd < 0.5f)
            {
                목적지_위치 = 목적지위치_반환(Index);
            }
            else
            {
                다음_목적지();
            }
        
        }

    }

    public override void 배회_종료()
    {

    }

    public override void 행동_트리거()
    {
        navMeshAgent.isStopped = true;
        // 대사 출력
        // 여기군 여기야!
        ProblemManager.인스턴스.RegisterVillainNPC(this);
        anim.SetTrigger("isDrill");
        StartCoroutine(DrillAndWait());
    }

    public override void 행동_갱신()
    {

    }

    public override void 행동_종료()
    {
        navMeshAgent.isStopped = false;
        coolTime = 행동_재사용시간; // 15초 동안 행동 불가
        // 민원 급증
    }

    IEnumerator DrillAndWait()
    {
        yield return new WaitUntil(() =>
        {
            var str = anim.GetCurrentAnimatorStateInfo(0);
            return str.normalizedTime >= 1f;
        });

        //Instantiate()

        yield return StartCoroutine(대기());

        behaviorAgent.SetVariableValue("Behavior", false);
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

    public override void 처치시()
    {

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
            behaviorAgent.SetVariableValue("Wander", true);
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
}