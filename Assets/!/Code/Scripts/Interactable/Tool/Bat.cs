using UnityEngine;

public class Bat :BaseItem
{
    [SerializeField] private float 공격범위 = 1.5f;
    [SerializeField] private float 공격반경 = 0.7f;
    [SerializeField] private LayerMask 공격대상레이어;
    [SerializeField] private float 공격쿨타임 = 0.5f;

    private float 최근공격시간;

    private void Start()
    {
        아이템_이름 = "삼단봉";
        아이템_설명 = "진상 퇴치를 위한 기본 장비입니다.";
        장착_위치 = new Vector3(0f, -0.1f, 0f);
        장착_회전 = new Vector3(0f, 0f, 0f);
    }

    public override void 아이템_장착()
    {
        base.아이템_장착();
        gameObject.SetActive(true);
    }

    public override void 아이템_해제()
    {
        // 삼단봉은 해제해도 떨어지지 않음
        gameObject.SetActive(false);
    }

    public override void 도구사용()
    {
        if (Time.time - 최근공격시간 < 공격쿨타임) return;
        최근공격시간 = Time.time;

        Vector3 위치 = GameObject.FindAnyObjectByType<PlayerController>().RightHand.position;
        Vector3 방향 = GameObject.FindAnyObjectByType<PlayerController>().카메라.transform.forward;

        if (Physics.SphereCast(위치, 공격반경, 방향, out RaycastHit hit, 공격범위, 공격대상레이어))
        {
            Debug.Log($"삼단봉 타격: {hit.collider.name}");
            BaseNPC npc = hit.collider.GetComponentInParent<BaseNPC>();
            if (npc != null)
            {
                npc.피격(10f); // [추가] NPC에 피해 적용
            }
        }

    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        // 시작 위치와 방향 (공격 기준)
        Vector3 위치 = GameObject.FindAnyObjectByType<PlayerController>()?.RightHand.position ?? transform.position;
        Vector3 방향 = GameObject.FindAnyObjectByType<PlayerController>()?.카메라.transform.forward ?? transform.forward;

        Gizmos.color = Color.red;

        // 시작 지점 구 표시
        Gizmos.DrawWireSphere(위치, 공격반경);

        // 끝 지점 구 표시
        Vector3 끝위치 = 위치 + 방향 * 공격범위;
        Gizmos.DrawWireSphere(끝위치, 공격반경);

        // 중간을 잇는 선 표시 (공격 방향)
        Gizmos.DrawLine(위치, 끝위치);
    }
}
