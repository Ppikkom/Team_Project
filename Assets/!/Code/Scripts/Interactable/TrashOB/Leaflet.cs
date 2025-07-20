using System.Collections;
using UnityEngine;

public class Leaflet : BaseItem
{
    private Coroutine ScoreMinusCoroutine;

    [Header("랜덤 활성화 자식 오브젝트")]
    [SerializeField] private GameObject[] 랜덤자식오브젝트들;

    protected override void Awake()
    {
        // 5개 자식 모두 비활성화
        if (랜덤자식오브젝트들 != null && 랜덤자식오브젝트들.Length > 0)
        {
            foreach (var obj in 랜덤자식오브젝트들)
                if (obj != null) obj.SetActive(false);

            // 랜덤 하나만 활성화
            int idx = Random.Range(0, 랜덤자식오브젝트들.Length);
            if (랜덤자식오브젝트들[idx] != null)
                랜덤자식오브젝트들[idx].SetActive(true);
        }
    }

    private void Start()
    {
        아이템_이름 = "전단지";
        아이템_설명 = "지하철 벽에 붙어있는 불법 전단지입니다.";

        장착_위치 = new Vector3(-0.1f, 0f, -0.2f);
        장착_회전 = new Vector3(0f, 90f, 270f);

        ScoreMinusCoroutine = StartCoroutine(ScoreMinus());
    }

    private IEnumerator ScoreMinus()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);

            if (ManagerRoomCollider.InsideTrashObjects.Contains(gameObject))
                continue;

            //if (ReputationManager.인스턴스 != null)
            //{
            //    ReputationManager.인스턴스.문제방치();
            //    //GameplayUIController.인스턴스?.킬로그알림추가("평판 -1");
            //    //Debug.Log("전단지 존재 → 평판 -1");
            //}
        }
    }




    private void OnDestroy()
    {
        if (ScoreMinusCoroutine != null)
        {
            StopCoroutine(ScoreMinusCoroutine);
            ScoreMinusCoroutine = null;
        }

        //if (ReputationManager.인스턴스 != null)
        //{
        //    ReputationManager.인스턴스.점수추가(10);
        //}
    }

    public override void 상호작용_시작()
    {
        base.상호작용_시작();
    }

    public override void 아이템_장착()
    {
        base.아이템_장착();
        gameObject.SetActive(true);
    }

    public override void 아이템_해제()
    {
        base.아이템_해제();
        gameObject.SetActive(true);
        버려졌는가 = true;
        Invoke(nameof(버림상태초기화), 1.0f);
    }

    private void 버림상태초기화()
    {
        버려졌는가 = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ManagerRoom"))
        {
            ManagerRoomCollider.InsideTrashObjects.Add(gameObject);
            Debug.Log($"[Leaflet] 관리실 진입: {gameObject.name}");
            ManagerRoomCollider.PrintInsideTrashObjects();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ManagerRoom"))
        {
            ManagerRoomCollider.InsideTrashObjects.Remove(gameObject);
            Debug.Log($"[Leaflet] 관리실 이탈: {gameObject.name}");
            ManagerRoomCollider.PrintInsideTrashObjects();
        }
    }
}
