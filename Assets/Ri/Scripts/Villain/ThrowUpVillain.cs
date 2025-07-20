using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.AI;

public class ThrowUpVillain : BaseNPC
{
    // ExampleVillain은 토하는 빌런 / ThrowUp은 토사물용

    // 땅 체크
    [SerializeField] private LayerMask mapLayer;
    [SerializeField] private float maxDistance = 3f;
    // 구토 프리팹

    // 임시
    private float playTime = 5f;
    float Timer = 0f;
    float coolTime = 0f;
    [SerializeField] float 행동_재사용시간;
    [SerializeField] private GameObject 토사물;

    // 내부에 ProblemType 추가
    public override ProblemType problemType => ProblemType.Villain;

    public override void OnEnable()
    {
        base.OnEnable();

        coolTime = 행동_재사용시간;

        // 시작 상태 설정
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


            TransitionTo(NPCState.Move); // 이게 유효한 코드일까? 어차피 블랙보드의 상태에서 따라서 State가 결정되는데.
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

        // navMesh 이동
        navMeshAgent.SetDestination(목적지_위치);

        if (도착확인()) // 목적지에 도달했는지 확인
        {
            if (coolTime <= 0f)
            {
                behaviorAgent.SetVariableValue("Behavior", true);
                return;
            }
            
            if (Random.Range(0f, 1f) < 0.5f) // Wander 조건
            {
                behaviorAgent.SetVariableValue("Wander", true);
                목적지_위치 = 목적지위치_반환(Index); // 맵 확인하고, 그 맵의 랜덤 위치를 목적지 설정
                return;
            }

            다음_목적지();
        }
    }

    public override void 배회_트리거()
    {
        Timer = 5f;
        base.배회_트리거();
    }

    public override void 배회_갱신()
    {
        navMeshAgent.SetDestination(목적지_위치);

        if (도착확인())
        {
            if (coolTime <= 0f)
            {
                behaviorAgent.SetVariableValue("Behavior", true);
                return;
            }

            if (Random.Range(0f, 1f) < 0.5f)
            {
                목적지_위치 = 목적지위치_반환(Index);
            }
            else
            {
                다음_목적지();
            }

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

    public override void 행동_트리거()
    {
        base.행동_트리거();
        navMeshAgent.isStopped = true;
        StartCoroutine(시간때우기()); // 이거 대신 애니메이션 재생 넣으면 될듯?
    }

    public override void 행동_갱신()
    {
        // 애니메이션 끝나면 상태 및 목적지 바꾸기
        GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
    }

    IEnumerator 시간때우기()
    {
        float timer = 0f;
        while (timer < playTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }
                

        anim.SetTrigger("isVomiting");

        yield return new WaitUntil(() =>
        {
            var str = anim.GetCurrentAnimatorStateInfo(0);
            return str.IsName("주취자_구토");   // Animator Controller에서 실제 State 이름
        });


        yield return new WaitUntil(() =>
        {
            var str = anim.GetCurrentAnimatorStateInfo(0);
            return str.normalizedTime >= 0.8f;
        });

        

        Vector3 rayOrigin = transform.position + Vector3.up;
        Vector3 rayDir = (transform.forward + Vector3.down).normalized;
        Vector3 PukeSpawn = transform.position + Vector3.down * 0.5f; // 구토 기본 생성 위치를 초기화

        if (Physics.Raycast(rayOrigin, rayDir, out RaycastHit hit, 3f, mapLayer))
        {
            if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 1f, NavMesh.AllAreas))
            {
                PukeSpawn = navHit.position;
            }
            else // 이거 잘 안되면 위 조건문을 and 처리하고 else문에서 하면 될듯?
            {
                // NavMesh 샘플에 실패했을 땐 히트 포인트에 생성
                PukeSpawn = hit.point;
            }
        }
        else
        {
            // 레이가 닿지 않았다면 NPC 바로 아래에 생성
            Vector3 fallbackPos = transform.position + Vector3.down * 0.5f;
            Instantiate(토사물, fallbackPos, Quaternion.identity);
        }

        // ThrowUpVillain이 만든 type.puke는 activeProblems에 등록
        GameObject obj = ProblemPoolManager.Instance.SpawnFromPool("Puke", PukeSpawn, Quaternion.identity); // 문제를 ProblemPoolManager로 생성(오브젝트 풀링)
        if (obj != null)
        {
            BaseProblem problem = obj.GetComponent<BaseProblem>();
            if (problem != null)
            {
                ProblemConfig config = new ProblemConfig
                {
                    type = ProblemType.Puke,
                    spawnPosition = PukeSpawn
                };
                problem.Initialize(config);

                // problemManager에 토사물을 등록하는 부분, 일반 문제와 진상 문제로 구분
                ProblemManager.인스턴스.RegisterProblem(problem); // 오브젝트 생성 후 일반 문제 리스트 등록
                ProblemManager.인스턴스.RegisterVillainNPC(this); // 일반 문제 리스트 후 진상 문제 리스트 등록
            }
        }

        coolTime = 행동_재사용시간;
        behaviorAgent.SetVariableValue("Behavior", false);
        다음_목적지();
        
    }

    public override void 행동_종료()
    {
        Debug.Log("행동 종료");
        //ReputationManager.인스턴스.점수감소(10);
        navMeshAgent.isStopped = false;
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
            목적지_위치 = 목적지위치_반환(Index);
            behaviorAgent.SetVariableValue("Wander", false);
        }
        else
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
        //Debug.Log(targetPosition + " " + agentPosition);
        agentPosition.y = targetPosition.y; // 이거 문제 생길 수도 있을듯
        return Vector3.Distance(agentPosition, targetPosition);
    }

    public string GetMapName()
    {
        // 이거 테스트 필요.
        // 플레이어나 카메라 위치에서 아래 방향으로 레이 캐스트
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

     void OnDrawGizmosSelected()
    {
        // 레이 원점과 방향은 실제 스폰 로직과 동일하게 계산
        Vector3 rayOrigin = transform.position + Vector3.up;
        Vector3 rayDir    = (transform.forward + Vector3.down).normalized;
        float   maxDist   = 3f;

        // 레이 표시 (초록)
        Gizmos.color = Color.green;
        Gizmos.DrawLine(rayOrigin, rayOrigin + rayDir * maxDist);

        // 레이 히트 여부에 따라 스폰 위치 계산
        Vector3 spawnPos;
        if (Physics.Raycast(rayOrigin, rayDir, out RaycastHit hit, maxDist, mapLayer))
        {
            // NavMesh 위 위치 샘플링
            if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 1f, NavMesh.AllAreas))
                spawnPos = navHit.position;
            else
                spawnPos = hit.point;
        }
        else
        {
            // 레이가 닿지 않으면 NPC 바로 아래
            spawnPos = transform.position + Vector3.down * 0.5f;
        }

        // 스폰 위치 표시 (빨강)
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(spawnPos, 0.2f);
    }
}
