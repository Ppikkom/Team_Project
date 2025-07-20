using UnityEngine;

public class ToiletPaper : BaseItem
{
    public new 상호작용_방식 상호작용_방식 => 상호작용_방식.즉시;
    [SerializeField] GameObject 휴지; // 던질 프리팹
    [SerializeField] float throwForce = 10f;

    public override void 상호작용_시작()
    {
        // 플레이어 앞에서 Tissue 프리팹 생성 후 힘을 가해 던짐
        Transform playerCam = Camera.main.transform;
        GameObject tissue = GameObject.Instantiate(
            휴지,
            playerCam.position + playerCam.forward * 1f,
            Quaternion.identity
        );
        Rigidbody rb = tissue.GetComponent<Rigidbody>();
        if (rb != null)
            rb.AddForce(playerCam.forward * throwForce, ForceMode.Impulse);

        ReputationManager.인스턴스.점수추가(5);
        Debug.Log($"{아이템_이름}을(를) 던졌습니다!");
    }
}