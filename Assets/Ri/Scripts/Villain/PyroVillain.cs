using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Unity.Behavior;

public class PyroVillain : BaseNPC
{
    // 쓰레기통에 발화 물질을 던지는(?) 빌런


    public List<Vector3> 쓰레기통_좌표;
    private List<Collider> 쓰레기통;
    bool 방화_트리거 = false;
    [SerializeField] float 행동_재사용시간;
    float coolTime = 0f;

    // 땅 체크
    [SerializeField] private LayerMask mapLayer;
    [SerializeField] private float maxDistance = 3f;

    // 임시
    private float playTime = 5f;
    float Timer = 0f;
    private Collider target;

    // 내부에 ProblemType 추가
    public override ProblemType problemType => ProblemType.Villain;
    public override void OnEnable()
    {
        base.OnEnable();
        쓰레기통 = mapData.SelectMany(md => md.mapData).Where(room => room.이름.Contains("쓰레기통")).Select(room => room.collider).ToList();

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
        if (목적지 == null || navMeshAgent == null || Index >= 목적지.Count) // 목적지가 없을 때, 예외처리
        {
            Debug.Log("목적지 | navMeshAgent 가 없어 배회 상태로 변경합니다.");
            behaviorAgent.SetVariableValue("Wander", true);
            return;
        }

        // navMesh 이동
        navMeshAgent.SetDestination(목적지_위치);

        if (도착확인()) // 목적지에 도달했는지 확인
        {
            if (방화_트리거 == true) // 쓰레기통에 도착했을 때
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
        Timer -= Time.deltaTime;
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
        base.행동_트리거();
        if (!대사표시중)
        {
            StartCoroutine(대사표시("이 쓰레기통에 불을 붙이면..."));
        }
        // 오브젝트 활성화하면 될 듯?
        // 이 코드에서 NPC한테 영향을 주는 것보단 쓰레기통 스크립트에서 NPC한테 영향을 주는게 더 좋을듯.
        ProblemManager.인스턴스.RegisterVillainNPC(this);
        navMeshAgent.isStopped = true; // 행동을 할 때, 멈춤.
        StartCoroutine(FireAndWait());
    }

    public override void 행동_갱신()
    {
        //Debug.Log("방화범 행동 갱신 중");
    }

    IEnumerator FireAndWait()
    {
        yield return new WaitUntil(() =>
        {
            var str = anim.GetCurrentAnimatorStateInfo(0);
            return str.normalizedTime >= 1f;
        });

        if (목적지[Index]._name.Contains("쓰레기통"))
        {
            target.transform.parent.GetComponentInChildren<TrashCan>().SetFire(true);
        }

        yield return StartCoroutine(대기());

        behaviorAgent.SetVariableValue("Behavior", false);
        coolTime = 행동_재사용시간;
        다음_상태();
    }

    IEnumerator 대기()
    {
        float timer = 0f;
        while (timer < playTime) // 이건 나중에 애니매이션 재생시간으로 바꾸면 될 듯?.
        {
            timer += Time.deltaTime;
            yield return null;
        }
    }

    public override void 행동_종료()
    {
        Debug.Log("방화범 행동 종료");
        방화_트리거 = false;
        navMeshAgent.isStopped = false;
    }

    public override void 처치시()
    {
        //ReputationManager.인스턴스.점수추가(10);
        //NPCManager.인스턴스.NPC반환(gameObject);
    }

    private void 다음_상태()
    {
            // 0 ~ 0.3 배회 상태
            // 0.3 ~ 0.7 쓰레기통으로 이동 목표 설정
            // 0.7 ~ 1 다음 목적지 (다음 목적지가 없으면 배회 상태)
        float rnd = Random.Range(0f, 1f);
        if (rnd < 0.3f) // Wander 조건
        {
            behaviorAgent.SetVariableValue("Wander", true);
            목적지_위치 = 목적지위치_반환(Index); // 맵 확인하고, 그 맵의 랜덤 위치를 목적지 설정
        }
        else if (rnd < 0.7 && coolTime < 0f) // 
        {
            // 쓰레기통에 도달하기 전은 이동 상태 / 도달한 후에 행동 상태
            // 이미 불이 붙은 상태이면, 재탐색
            // 랜덤한 층의 쓰레기통의 좌표를 받아서 이동.

            for (int i = 0; i < 100; i++)
            {
                target = 쓰레기통[Random.Range(0, 쓰레기통.Count)];
                if (target.transform.parent.GetComponentInChildren<TrashCan>().isOnFire == false)
                {
                    목적지_위치 = RandomPoint(target);
                    방화_트리거 = true;
                    return;
                }   
            }
            Debug.LogError($"{name}에서 쓰레기통을 찾을 수 없거나, 전부 불이 붙은 상태입니다.");
            coolTime = 행동_재사용시간; // 다시 여기로 진입 못 하게 막음.
            // 무슨 상태인지 확인해야 할 수도 있음.
            return;
        }
        
        다음_목적지();
    }

    private void 다음_목적지()
    {
        if (Index + 1 < 목적지.Count) // 다음 목적지가 리스트에 있으면
        {
            Index++;
            목적지_위치 = 목적지위치_반환(Index);
            behaviorAgent.SetVariableValue("Wander", false);
        }
        else // 다음 목적지가 리스트에 없으면
        {
            목적지_위치 = 목적지위치_반환(Index);
            behaviorAgent.SetVariableValue("Wander", true);
        }
    }

    private bool 도착확인() => GetDistanceToWaypoint() <= navMeshAgent.stoppingDistance;

    private float GetDistanceToWaypoint()
    {
        Vector3 targetPosition = 목적지_위치;
        Vector3 agentPosition = transform.position;
        agentPosition.y = targetPosition.y;
        return Vector3.Distance(agentPosition, targetPosition);
    }

    public string GetMapName()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, maxDistance, mapLayer))
        {
            for (int i = 0; i < mapData.Length; i++)
            {
                if (mapData[i].HasCollider(hit.collider))
                {
                    return hit.collider.name;
                }
            }
        }

        return null;
    }


    public override void OnDisable()
    {
        base.OnDisable();
    }
}
