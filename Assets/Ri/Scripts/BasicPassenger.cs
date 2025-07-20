using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BasicPassenger : BaseNPC
{
    
    float DistanceThreshold = 0.5f;

    [SerializeField] private LayerMask mapLayer;
    [SerializeField] private float maxDistance = 3f;
    [SerializeField] private GameObject trashPrefab;
    [Range(0, 1)]
    [SerializeField] private float 쓰레기_확률;


    // 임시
    private float waitTime = 3f;
    private Collider target;
    public override void Awake()
    {
        base.Awake();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        컴포넌트_활성화();
        transform.rotation = Quaternion.Euler(0, -180, 0);

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
            TransitionTo(NPCState.Move);
        }
        else
        {
            TransitionTo(NPCState.Stop);
        }
    }

    public override void Update()
    {
        base.Update();
        //navMeshAgent.speed = 2f;
    }

    public override void 이동_트리거()
    {
        behaviorAgent.SetVariableValue("IsOverListRange", false);
        anim.SetBool("isWalking", true);
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
        
        if (Vector3.Distance(navMeshAgent.destination, 목적지_위치) > 0.1f)
        {
            navMeshAgent.SetDestination(목적지_위치);
        }


        if (도착확인()) // 목적지에 도달했는지 확인
        {
            if (mapData[목적지[Index]._floor].분류_확인(목적지[Index]._name, 장소_이름.상호작용)) // 행동으로 가야할듯
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

    public override void 멈춤_갱신()
    {
        GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
    }

    public override void 멈춤_종료()
    {
        base.멈춤_종료();
    }

    // 행동 트리거
    public override void 행동_트리거()
    {
        anim.SetBool("isWalking", false);
        if (!대사표시중 && 현재대사 != null && 현재대사.Length > 0)
        {
            string 대사 = 현재대사[Random.Range(0, 현재대사.Length)];
            StartCoroutine(대사표시(대사));
        }
        navMeshAgent.isStopped = true;
        anim.SetTrigger("isThrow");
        StartCoroutine(ThrowAndWait());
    }

    public override void 행동_갱신()
    {
        if (target == null) return;
        Vector3 NPC방향 = target.transform.position - transform.position;
        NPC방향.y = 0;
        if (NPC방향.sqrMagnitude < 0.0001f) return;

        Quaternion npcRot = Quaternion.LookRotation(NPC방향);
        transform.rotation = Quaternion.Slerp(transform.rotation, npcRot, Time.deltaTime * 2f);

    }

    IEnumerator ThrowAndWait()
    {
        yield return new WaitUntil(() =>
        {
            var str = anim.GetCurrentAnimatorStateInfo(0);
            return str.normalizedTime >= 1f;
        });

        TrashCan trash = target.transform.parent.GetComponentInChildren<TrashCan>();

        if (목적지[Index]._name.Contains("쓰레기통") && trash.currentStack < trash.maxStack)
        {
            trash.AddStack();
        }
        else
        {
            Instantiate(trashPrefab, transform.position + new Vector3(Random.Range(0.35f, 1f), 0, Random.Range(0.35f, 1f)), Quaternion.identity);
        }


        yield return StartCoroutine(대기());

        behaviorAgent.SetVariableValue("Behavior", false);
        다음_목적지();
    }

    IEnumerator 대기()
    {
        float timer = 0f;
        while (timer < waitTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        
 
    }

    public override void 행동_종료()
    {
        navMeshAgent.isStopped = false;
        navMeshAgent.stoppingDistance = 0.5f;
    }

    public override void 배회_트리거()
    {
        anim.SetBool("isWalking", true);
    }

    public override void 배회_갱신()
    {
        navMeshAgent.SetDestination(목적지_위치);

        if (도착확인())
        {
            if (Random.Range(0f, 1f) < 0.5f)
            {
                목적지_위치 = 목적지위치_반환(Index); // 맵 확인하고, 그 맵의 랜덤 위치를 목적지 설정
            }
            else
            {
                behaviorAgent.SetVariableValue("Wander", false);
                다음_목적지();
                return;
            }

        }
    }

    public override void 처치시()
    {
        Debug.Log("중립 NPC 처리됨!");
        ReputationManager.인스턴스.점수추가(-50);
    }

    private void 다음_목적지()
    {
        if (Index + 1 < 목적지.Count)
        {
            Index++;

            while (mapData[목적지[Index]._floor].분류_확인(목적지[Index]._name, 장소_이름.상호작용)) //
            {
                if (Random.Range(0f, 1f) < 쓰레기_확률)
                {
                    // 대칭 조사
                    float rnd = Random.Range(0f, 1f);
                    if (목적지[Index]._name == "쓰레기통1" && rnd < 0.5f)
                            ChangeDestination(Index, 2, "쓰레기통2");
                    else if(목적지[Index]._name == "상행_쓰레기통1" && rnd < 0.5f)
                        ChangeDestination(Index, 5, "상행_쓰레기통2");
                    else if(목적지[Index]._name == "하행_쓰레기통1" && rnd < 0.5f)
                        ChangeDestination(Index, 5, "하행_쓰레기통2");


                    target = mapData[목적지[Index]._floor].GetColliderByName(목적지[Index]._name);
                    navMeshAgent.stoppingDistance = 2f;
                    break;
                }
                Index += 1;
            }

            목적지_위치 = 목적지위치_반환(Index);
        }
        else // 목적지에 도달했을 때.
        {
            컴포넌트_비활성화();
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
