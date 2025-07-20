using UnityEngine;

public class FireExtinguisher : BaseItem
{
    [Header("효과음")]
    public AudioClip 사용효과음;

    protected override void Awake()
    {
        if (초기위치 != null)
        {
            transform.position = 초기위치.position;
            transform.rotation = 초기위치.rotation;
        }
    }

    public override void 아이템_장착()
    {
        base.아이템_장착();
        transform.localScale = new Vector3(0.21f, 0.23f, 0.22f); // 또는 원하는 정상 스케일
        gameObject.SetActive(true);
    }

    public override void 도구사용()
    {
        // 카메라 앞에 Fire가 있는지 확인
        var 플레이어 = Object.FindAnyObjectByType<PlayerController>();
        if (플레이어 == null) return;

        Camera cam = 플레이어.카메라.GetComponent<Camera>();
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (Physics.SphereCast(ray, 0.4f, out RaycastHit hit, 2f, 플레이어.상호작용_레이어))
        {
            var fire = hit.collider.GetComponentInParent<Fire>();
            if (fire != null)
            {
                // Attack 버튼을 누를 때마다 상호작용 시작/유지/종료를 적절히 호출
                fire.상호작용_시작();
                if (audioSource != null && 사용효과음 != null)
                    audioSource.PlayOneShot(사용효과음);
                fire.상호작용_유지(Time.deltaTime); // 유지시간은 상황에 맞게 조정
                // 필요시 fire.상호작용_종료(); 호출
            }
        }
    }
}
