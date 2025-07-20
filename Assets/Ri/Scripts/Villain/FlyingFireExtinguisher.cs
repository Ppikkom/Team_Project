using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;


public class FlyingFireExtinguisher : BaseNPC
{

    // 목적지
    private GameObject target;
    [SerializeField] private Vector3 targetRange;
    [SerializeField] LayerMask targetLayer;
    [SerializeField] LayerMask mapLayer;

    // 이하 임시 변수
    [SerializeField] private float 대기시간 = 10f;
    [SerializeField] bool hasFireExtinguisher = false; // 소화기를 가지고 있는지.
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
        // 목적지 도착 후 소화기 위치 찾기


    }

    public override void 이동_갱신()
    {
        navMeshAgent.SetDestination(목적지_위치);

        // 해당 층 소화기 있는지 확인.
        // 있으면 그 층 소화기 직행 아니면 랜덤 층 소화기

        if (도착확인())
        {

            // 그 층에서 소화기 위치이면 트리거 활성화 후, Wander로 변경;
            //if () // 

            float rnd = Random.Range(0f, 1f);
            if (rnd < 0.5f || hasFireExtinguisher) // Wander 조건
            {
                behaviorAgent.SetVariableValue("Wander", true);
                //목적지_위치 = GetColliderPosition(Index);
            }
            else
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
        // 장소 랜덤 // 
        목적지_위치 = mapData[Random.Range(0, mapData.Length)].랜덤_지역좌표();
    }

    public override void 배회_갱신()
    {
        navMeshAgent.SetDestination(목적지_위치);

        if (hasFireExtinguisher)
        {
            // 근처에 있는 승객들 튕겨져 나가게
            // 콜라이더 크기안에 있는 오브젝트 조사해서
            // 중심과 비교하여 벡터 방향 설정 / 그 방향으로 튕기게.
            Vector3 center = transform.position + Vector3.up * targetRange.y;
            Collider[] targets = Physics.OverlapBox(center, targetRange, Quaternion.identity, targetLayer);

            float pushForce = 1f;   // 원하는 튕겨낼 힘

            foreach (var hit in targets)
            { // 방향 수정필요
                var rb = hit.attachedRigidbody;
                Vector3 dir = (hit.transform.position - transform.position).normalized;
                dir.y = 1f;
                rb.AddForce(dir * pushForce, ForceMode.Impulse);
            }
        }


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
        StartCoroutine(대기());

    }

    public override void 행동_갱신()
    {

    }

    public override void 행동_종료()
    {
        Debug.Log($"{gameObject.name} 행동_종료 실행");
    }

    IEnumerator 대기()
    {
        float timer = 0;
        while (timer < 대기시간)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // 소화기 
        hasFireExtinguisher = true;
        // 인스펙터 소화기 비활성화?

        다음_목적지();
        behaviorAgent.SetVariableValue("Behavior", false);
    }

    public override void 처치시()
    {
        //if (hasFireExtinguisher)
        // 소화기 드롭

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
            behaviorAgent.SetVariableValue("Behavior", false);
        }
        else // 다음 목적지가 없을 때.
        {
            // 이동 상태 및 행동을 하기 전
            // 해당 층 분류와 이름으로 소화기를 찾아서 있으면 소화기로 직행
            // 아니면 소화기가 이름인 콜라이더를 전부 찾아서 랜덤한 값 사용

            if (!hasFireExtinguisher)
            {
                if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 3f, mapLayer)) // 이걸로 부모 확인 가능
                {
                    // 해당 층에 있는 소화기 접근
                    if (System.Enum.TryParse(hit.transform.parent.name, out 장소_이름 층))
                    {
                        var v = mapData[(int)층].GetColliderByName("소화기");
                        목적지_위치 = v.bounds.center;
                    }
                    else Debug.Log($"[소화기 탐색 실패] {hit.transform.parent.name} 은 enum에 존재하지 않습니다.");
                }
                else
                {
                    // 랜덤한 층에 있는 소화기 접근
                    // 랜덤 숫자 해서 그 층에 소화기 있으면 거기; 아니면 반복
                    Collider col;
                    do
                    {
                        col = mapData[Random.Range(0, mapData.Length)].GetColliderByName("소화기");
                    } while (col == null);
                    목적지_위치 = col.bounds.center;
                }
                behaviorAgent.SetVariableValue("Wander", false);
            }
        }
    }

    private bool 도착확인() => GetDistanceToWaypoint() <= navMeshAgent.stoppingDistance;

    private float GetDistanceToWaypoint()
    {
        Vector3 targetPosition = 목적지_위치;
        Vector3 agentPosition = transform.position;
        agentPosition.y = targetPosition.y; // 이거 문제 생길 수도 있을듯
        return Vector3.Distance(agentPosition, targetPosition);
    }

    #if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        var col = GetComponent<Collider>();
        Vector3 halfExtents = targetRange;
        Vector3 boxCenter = new Vector3(col.bounds.center.x, col.bounds.min.y, col.bounds.center.z) + Vector3.up * halfExtents.y; // 이거 점점 내려가는 이유?
        Vector3 size = halfExtents;
        Gizmos.DrawWireCube(boxCenter, size * 2);
    }
#endif

    public override void OnDisable()
    {
        base.OnDisable();
    }

}