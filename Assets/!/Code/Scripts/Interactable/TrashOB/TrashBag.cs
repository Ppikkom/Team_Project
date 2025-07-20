using UnityEngine;
using System.Collections;

public class TrashBag : BaseItem
{
    public override float? 이동속도_제한값 => 3f;

    private Coroutine ScoreMinusCoroutine;
    private bool isInManagerRoom = false;
    private bool isCounted = false; // normalCount 중복 방지
    private bool isHeld = false;    // 손에 쥐고 있는지

    private void Start()
    {
        아이템_이름 = "쓰레기봉투";
        아이템_설명 = "쓰레기통에서 얻은 쓰레기봉투입니다.";

        장착_위치 = new Vector3(0f, -0.1f, 0.3f);
        장착_회전 = new Vector3(0f, 0f, 0f);

        ScoreMinusCoroutine = StartCoroutine(ScoreMinus());

        // TrashBag이 씬에 등장하면 normalCount
        if (!isCounted && GameplayUIController.인스턴스 != null)
        {
            GameplayUIController.인스턴스.normalCount();
            isCounted = true;
        }
    }

    private void OnEnable()
    {
        if (ScoreMinusCoroutine == null)
            ScoreMinusCoroutine = StartCoroutine(ScoreMinus());

        if (GameplayUIController.인스턴스 != null)
        {
            GameplayUIController.인스턴스.normalCount();
            isCounted = true;
        }
    }


    private void OnDisable()
    {
        if (ScoreMinusCoroutine != null)
        {
            StopCoroutine(ScoreMinusCoroutine);
            ScoreMinusCoroutine = null;
        }

        if (isCounted && GameplayUIController.인스턴스 != null)
        {
            GameplayUIController.인스턴스.normalDisCount();
            isCounted = false;
        }

        isHeld = false;
        isInManagerRoom = false;
    }

    public void TryCountOnProblemReappear()
    {
        if (!isCounted && !isHeld && !isInManagerRoom && GameplayUIController.인스턴스 != null)
        {
            GameplayUIController.인스턴스.normalCount();
            isCounted = true;
        }
    }



    private IEnumerator ScoreMinus()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);

            if (ManagerRoomCollider.InsideTrashObjects.Contains(gameObject))
                continue;

            if (ReputationManager.인스턴스 != null)
            {
                ReputationManager.인스턴스.문제방치();
            }
        }
    }

    private void OnDestroy()
    {
        if (ScoreMinusCoroutine != null)
        {
            StopCoroutine(ScoreMinusCoroutine);
            ScoreMinusCoroutine = null;
        }

        // 오브젝트가 삭제될 때 normalDisCount
        if (isCounted && GameplayUIController.인스턴스 != null)
        {
            GameplayUIController.인스턴스.normalDisCount();
            isCounted = false;
        }

        // 점수 지급: TrashBag이 삭제될 때 20점 추가
        if (ReputationManager.인스턴스 != null)
        {
            ReputationManager.인스턴스.점수추가(20);
        }
    }


    public override void 상호작용_시작()
    {
        base.상호작용_시작();
    }

    public override void 아이템_장착()
    {
        base.아이템_장착();
        gameObject.SetActive(true);

        // 손에 쥐면 카운트에서 제외
        if (isCounted && GameplayUIController.인스턴스 != null)
        {
            GameplayUIController.인스턴스.normalDisCount();
            isCounted = false;
        }
        isHeld = true;
    }

    public override void 아이템_해제()
    {
        base.아이템_해제();
        gameObject.SetActive(true);
        버려졌는가 = true;

        // 손에서 내려놓았고, 관리실 밖이면 카운트에 다시 포함
        if (!isInManagerRoom && !isCounted && GameplayUIController.인스턴스 != null)
        {
            GameplayUIController.인스턴스.normalCount();
            isCounted = true;
        }
        isHeld = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ManagerRoom"))
        {
            if (!isInManagerRoom)
            {
                isInManagerRoom = true;
                ManagerRoomCollider.InsideTrashObjects.Add(gameObject);
                Debug.Log($"[TrashBag] 관리실 진입: {gameObject.name}");
                ManagerRoomCollider.PrintInsideTrashObjects();

                // 관리실에 들어가면 normalDisCount
                if (isCounted && GameplayUIController.인스턴스 != null)
                {
                    GameplayUIController.인스턴스.normalDisCount();
                    isCounted = false;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ManagerRoom"))
        {
            if (isInManagerRoom)
            {
                isInManagerRoom = false;
                ManagerRoomCollider.InsideTrashObjects.Remove(gameObject);
                Debug.Log($"[TrashBag] 관리실 이탈: {gameObject.name}");
                ManagerRoomCollider.PrintInsideTrashObjects();

                // 관리실에서 나왔고, 손에 들고 있지 않으면 normalCount
                if (!isCounted && !isHeld && GameplayUIController.인스턴스 != null)
                {
                    GameplayUIController.인스턴스.normalCount();
                    isCounted = true;
                }
            }
        }
    }


}
