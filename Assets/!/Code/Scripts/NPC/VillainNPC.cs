using UnityEngine;
using System.Collections;

public class VillainNPC : BaseNPC
{
    [SerializeField] private string 빌런이름 = "Unknown Villain";
    [SerializeField] private string 문제행동 = "Making Trouble";
    [SerializeField] private string 위치 = "Unknown Location";
    [SerializeField] private float 행동주기 = 10f; // 문제 행동 주기(초)

    private bool 문제행동시작됨 = false;
    private Coroutine 점수감소코루틴;

    private void Start()
    {
        NPC종류 = NPCType.Villain;
        StartCoroutine(문제행동루틴());
    }

    public override void 행동_트리거()
    {
        if (!문제행동시작됨)
        {
            // 1. 처음 문제행동 시 알림 한 번만
            string 메시지 = $"{빌런이름} is {문제행동} at {위치}!";
            GameplayUIController.인스턴스.킬로그알림추가(메시지);
            Debug.Log($"{빌런이름} 문제 행동: {문제행동} at {위치}");

            // 2. 10초 후부터 10초마다 점수 감소 시작
            점수감소코루틴 = StartCoroutine(점수감소루틴());
            문제행동시작됨 = true;
        }
    }

    private IEnumerator 문제행동루틴()
    {
        while (true)
        {
            yield return new WaitForSeconds(행동주기);
            행동_트리거();
        }
    }

    private IEnumerator 점수감소루틴()
    {
        // 10초 대기 (알림 후)
        yield return new WaitForSeconds(10f);

        // 빌런이 처치될 때까지 10초마다 반복
        while (true)
        {
            ReputationManager.인스턴스.점수감소(5);
            yield return new WaitForSeconds(10f);
        }
    }

    public override void 처치시()
    {
        Debug.Log($"{빌런이름} 처리됨!");
        ReputationManager.인스턴스.점수추가(50);
        // Destroy(gameObject); // 빌런 제거
        gameObject.SetActive(false);
    }
}