using System.Collections;
using UnityEngine;

public class Trash : BaseItem
{
    private Coroutine ScoreMinusCoroutine;

    //[Header("효과음")]
    //public AudioClip 장착효과음;
    //public AudioClip 해제효과음;

    //private AudioSource audioSource;

    private void Start()
    {
        아이템_이름 = "쓰레기";
        아이템_설명 = "버려진 쓰레기입니다.";

        // 필요에 따라 장착_위치, 장착_회전 설정

        ScoreMinusCoroutine = StartCoroutine(ScoreMinus());

        audioSource = GetComponent<AudioSource>();
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
                //GameplayUIController.인스턴스?.킬로그알림추가("평판 -1");
                //Debug.Log("쓰레기 존재 → 평판 -1");
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

        // 쓰레기가 삭제될 때 10점 증가
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
        //if (audioSource != null && 장착효과음 != null)
        //    audioSource.PlayOneShot(장착효과음);

        base.아이템_장착();
        gameObject.SetActive(true);

    }

    public override void 아이템_해제()
    {
        //if (audioSource != null && 해제효과음 != null)
        //    audioSource.PlayOneShot(해제효과음);

        base.아이템_해제();
        gameObject.SetActive(true);
        버려졌는가 = true;

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ManagerRoom"))
        {
            ManagerRoomCollider.InsideTrashObjects.Add(gameObject);
            Debug.Log($"[Trash] 관리실 진입: {gameObject.name}");
            ManagerRoomCollider.PrintInsideTrashObjects();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ManagerRoom"))
        {
            ManagerRoomCollider.InsideTrashObjects.Remove(gameObject);
            Debug.Log($"[Trash] 관리실 이탈: {gameObject.name}");
            ManagerRoomCollider.PrintInsideTrashObjects();
        }
    }
}
