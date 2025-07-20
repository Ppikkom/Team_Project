using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;



public class ChokChok : BaseNPC
{
        // 상태 패턴

    // 이하 임시 변수

    public override void OnEnable()
    {
        base.OnEnable();

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

    }

    public override void 이동_갱신()
    {
        //navMeshAgent.SetDestination(목적지);
        
    }

    public override void 멈춤_종료()
    {

    }

    public override void 배회_트리거()
    {
        
    }

    public override void 배회_갱신()
    {
        // 최종 목적지에 도착하고, 도착할 시 일정확률로 행동 발동.
    }

    public override void 배회_종료()
    {

    }

    public override void 행동_트리거()
    {
        // 대사 출력
        // 후후후 촉촉하게 해주지.
        
        // 범위 안에 OverlapBox로 일반 NPC 저장? 혹은 바로 실행
    }

    public override void 행동_갱신()
    {
        
    }

    public override void 행동_종료()
    {
        // 민원 급증
        // 만약 저장된 변수에 NPC가 있다면 평판 감소 및 레드 카드? 부여
        // 트리거에서 받은 변수로 해당 오브젝트에다가 공포 상태 부여
    }

    public override void 처치시()
    {
        //ReputationManager.인스턴스.점수추가(10);

        //NPCManager.인스턴스.NPC반환(gameObject);
    }
    
}