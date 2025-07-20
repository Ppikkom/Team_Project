using UnityEngine;
using UnityEngine.InputSystem;

public class WaterMop : BaseCleaningTool
{
    public float 청소범위 = 2f;
    public LayerMask 상호작용레이어;

    public float 쿨타임 = 1.5f;

    private float 마지막사용시간 = -Mathf.Infinity;


    [Header("머티리얼")]
    public Material 기본메트리얼;
    public Material 중간더러움메트리얼; // 5회 사용 후
    public Material 완전더러움메트리얼; // 10회 사용 후
    private Renderer mopRenderer;

    [Header("메트리얼 변경 대상(자식 오브젝트)")]
    public GameObject 메트리얼대상오브젝트;
    private Renderer targetRenderer;

    [Header("효과음")]
    public AudioClip 사용효과음;
    public AudioClip 세척효과음;

    protected override void Awake()
    {
        base.Awake();
        아이템_이름 = "물걸레";
        아이템_설명 = "낙서를 닦을 수 있는 물걸레입니다.";

        장착_위치 = new Vector3(0f, -0.01f, 0f);
        장착_회전 = new Vector3(0f, 270f, 90f);

        if (초기위치 != null)
        {
            transform.position = 초기위치.position;
            transform.rotation = 초기위치.rotation;
        }
        // 자식 오브젝트의 Renderer를 가져옴
        if (메트리얼대상오브젝트 != null)
            targetRenderer = 메트리얼대상오브젝트.GetComponent<Renderer>();
        else
            targetRenderer = GetComponentInChildren<Renderer>();

        if (targetRenderer != null && 기본메트리얼 != null)
            targetRenderer.material = 기본메트리얼;
    }


    public override void 도구사용()
    {
        float 현재시간 = Time.time;

        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, 청소범위, 상호작용레이어))
        {
            // 1. Sink를 보고 있다면 세척
            if (hit.collider.GetComponentInParent<Sink>() != null)
            {
                Debug.Log("🧽 Sink를 향해 물걸레 사용! → 세척 시도");
                세척(); // 현재사용횟수 초기화
                if (targetRenderer != null && 기본메트리얼 != null)
                    targetRenderer.material = 기본메트리얼;

                if (audioSource != null && 세척효과음 != null)
                    audioSource.PlayOneShot(세척효과음);

                return; // 여기서 종료 (공격 아님)
            }

            // 2. Graffiti를 보고 있다면 청소
            if (hit.collider.CompareTag("Graffiti"))
            {
                Graffiti graffiti = hit.collider.GetComponentInParent<Graffiti>();
                if (graffiti != null)
                {
                    // ❌ 사용 불가능한 경우
                    if (!사용가능())
                    {
                        Debug.Log("❌ 물걸레는 더러워져 사용할 수 없습니다. 세척해주세요.");
                        return;
                    }

                    // ⏳ 쿨타임
                    if (현재시간 - 마지막사용시간 < 쿨타임)
                    {
                        Debug.Log("⏳ 물걸레 쿨타임 중...");
                        return;
                    }

                    // ✅ 정상적인 사용 로직
                    마지막사용시간 = 현재시간;
                    현재사용횟수++;

                    // 머티리얼 변경
                    if (targetRenderer != null)
                    {
                        if (현재사용횟수 >= 10 && 완전더러움메트리얼 != null)
                            targetRenderer.material = 완전더러움메트리얼;
                        else if (현재사용횟수 >= 5 && 중간더러움메트리얼 != null)
                            targetRenderer.material = 중간더러움메트리얼;
                        else if (기본메트리얼 != null)
                            targetRenderer.material = 기본메트리얼;
                    }

                    Debug.Log($"물걸레로 그래피티 청소 시도");
                    graffiti.상호작용_시작();

                    if (audioSource != null && 사용효과음 != null)
                        audioSource.PlayOneShot(사용효과음);
                }
            }
        }
        // Sink, Graffiti 둘 다 아니면 아무 동작도 하지 않음
    }


}
