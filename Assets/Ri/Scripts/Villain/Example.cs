using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;



public class Example : BaseNPC
{

    // NavMesh를 이용하기 위한 선언
    Vector3 현재_목적지;


    // 예시
    // 1. bool 행동을했는가 = false; 변수를 선언
    // 2. 행동종료에다가 행동을했는가 = true;를 작성
    // 3. Update에 if(행동을했는가) 평판--; 를 하면 행동한 이 후, 점수 감소 완성!


    // 상태를 바꿀 떄, 아래 함수를 선언
    // behaviorAgent.SetVariableValue("Stop", true); / 예시
    // 첫 번째 인자는 행동 트리에 있는 변수를 string 값으로 적으면 됨.

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
            TransitionTo(NPCState.Move);
            현재_목적지 = 목적지위치_반환(Index);
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
    }

    public override void 이동_트리거()
    {
         // 이 상태로 진입할 때, 1회 호출
    }

    public override void 이동_갱신()
    {
         // 이 상태가 유지될 때마다 호출
        navMeshAgent.SetDestination(현재_목적지);
        
        // 목적지에 도착했는 지 확인.
        if (도착확인())
        {
            // 여기서 다음 목적지를 찾아서, 상태를 전이하던 유지하던 함.
            // 함수나 변수를 만들어서 조건을 다양화 할 수 있음.
            다음_목적지();
        }

    }

    public override void 멈춤_종료()
    {
        // 다른 상태로 전이될 때, 1회 호출
    }

    public override void 배회_트리거()
    {
        
    }

    public override void 배회_갱신()
    {
        navMeshAgent.SetDestination(현재_목적지);

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

    }

    public override void 행동_갱신()
    {

    }

    public override void 행동_종료()
    {
        
    }

    IEnumerator 대기()
    {
        float timer = 0;
        while (timer < 3f)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        // 

        behaviorAgent.SetVariableValue("Behavior", false);
    }

    public override void 처치시()
    {
        //ReputationManager.인스턴스.점수추가(10);
        //NPCManager.인스턴스.NPC반환(gameObject);
    }

    private void 다음_목적지()
    {
        if (Index + 1 < 목적지.Count)
        {
            Index++;
            현재_목적지 = 목적지위치_반환(Index);
            behaviorAgent.SetVariableValue("Wander", false);
            behaviorAgent.SetVariableValue("Behavior", false);
        }
        else
        {
            behaviorAgent.SetVariableValue("Behavior", false);
        }
    }

    private bool 도착확인() => 거리비교() <= navMeshAgent.stoppingDistance;
    

    // 현재 목적지와 이 오브젝트의 거리를 비교하여 리턴하는 함수
    private float 거리비교()
    {
        Vector3 targetPosition = 현재_목적지;
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