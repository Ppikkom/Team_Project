using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Unity.Mathematics;

public class Saimin : BaseNPC
{
    // 이하 임시 변수
    private float playTime = 3f; // 조건 달성 변수
    float Timer = 0f;

    float skillRange = 4f; // 최대 범위
    float viewAngle = 90f; // 각도
    Transform player;

    Vector3 origin;
    Vector3 dir;
    [SerializeField] LayerMask layer;

    // 내부에 ProblemType 추가
    public override ProblemType problemType => ProblemType.Villain;

    public override void OnEnable()
    {
        base.OnEnable(); // 최면은 고려해야할수도?
        player = GameObject.Find("Player").GetComponent<Transform>();
        behaviorAgent.SetVariableValue("Behavior", true);
        SpawnPosition(2f);
    }

    public override void Awake()
    {
        base.Awake();
    }

    public override void Update()
    {
        base.Update();

        navMeshAgent.SetDestination(player.transform.position);

        if ((transform.position - player.transform.position).magnitude < navMeshAgent.stoppingDistance)
        {
            navMeshAgent.isStopped = true;
            //anim.SetBool("isWalking", false);
        }
        else
        {
            navMeshAgent.isStopped = false;
            //anim.SetBool("isWalking", true);
        }
    }

    public override void 이동_트리거()
    {
        base.이동_트리거();
    }

    public override void 이동_갱신()
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

    public override void 배회_트리거()
    {
        base.배회_트리거();
    }

    public override void 배회_갱신()
    {

    }

    public override void 배회_종료()
    {

    }

    public override void 행동_트리거()
    {
        base.행동_트리거();
        // 대사 출력
        // 후후후....
        ProblemManager.인스턴스?.RegisterVillainNPC(this);
    }

    public override void 행동_갱신()
    {
        // 일시정지 상태면 return;


        if (Timer >= playTime)
        {
            // 행동 종료 및 플레이어 공포 디버프 제공
            Debug.Log("조건 달성");
        }


        if (PlayerInRangeAndSight())
        {
            Debug.Log("조건 충족 중");
            Timer += Time.deltaTime;
        }
        else
            Timer = 0;
    }

    public override void 행동_종료()
    {

    }

    public override void 처치시()
    {
        //ReputationManager.인스턴스.점수추가(10);

        //NPCManager.인스턴스.NPC반환(gameObject);
    }

    private void SpawnPosition(float distance)
    {
        // 플레이어의 바로 앞
        // 땅바닥을 보고 있을때도 고려해야함.
        var cam = Camera.main.transform;

        Vector3 hF = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 spawnPos = cam.position + hF * distance;

        Vector3 toPlayer = cam.position - spawnPos;
        toPlayer.y = 0;
        Quaternion spawnRot = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);

        transform.position = spawnPos;
        transform.rotation = spawnRot;
    }

    private bool PlayerInRangeAndSight()
    {
        // 뷰포트 체크
        Vector3 vp = Camera.main.WorldToViewportPoint(transform.position);
        if (vp.z < 0 || vp.x < 0 || vp.x > 1 || vp.y < 0 || vp.y > 1)
            return false;

        // 장애물 체크도 해야할 듯?

        Vector3 dir = player.transform.position - transform.position;
        float dist = dir.magnitude;

        if (dist > skillRange) return false; // 시야각에 따르게 계산해야할지도?

        // 반시야갹 확인
        float halfFOV = viewAngle * 0.5f;
        float angleToPlayer = Vector3.Angle(transform.forward, dir.normalized);
        if (angleToPlayer > halfFOV) return false;

        return true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward * skillRange;
        Gizmos.DrawLine(origin, origin + direction);
        Gizmos.DrawSphere(origin + direction, 0.1f);

        Gizmos.color = Color.yellow;
        Vector3 leftDir = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward;
        Gizmos.DrawLine(origin, origin + leftDir * skillRange);
        Gizmos.DrawLine(origin, origin + rightDir * skillRange);
    }

}
