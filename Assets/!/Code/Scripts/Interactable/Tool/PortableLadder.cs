using UnityEngine;

public class PortableLadder : BaseItem
{
    [Header("사다리 이동 속도")]
    [SerializeField] private float ladderMoveSpeed = 3f;

    private Collider ladderCollider;
    private bool isPlaced = true;
    private Vector3 originalScale;

    //[Header("효과음")]
    //public AudioClip 장착효과음;
    //public AudioClip 해제효과음;

    //private AudioSource audioSource;

    void FixedUpdate()
    {
        // 중력 2배 적용 예시
        GetComponent<Rigidbody>().AddForce(Physics.gravity * 2, ForceMode.Acceleration);
    }

    protected override void Awake()
    {
        base.Awake();
        audioSource = GetComponent<AudioSource>();

        아이템_이름 = "휴대용 사다리";
        아이템_설명 = "들고 다닐 수 있는 사다리입니다.";

        장착_위치 = new Vector3(0.3f, 0f, 0.5f);
        장착_회전 = new Vector3(0f, 0f, 0f);

        if (초기위치 != null)
        {
            transform.position = 초기위치.position;
            transform.rotation = 초기위치.rotation;
        }

        ladderCollider = GetComponent<Collider>();
        if (ladderCollider != null)
            ladderCollider.isTrigger = true;
        originalScale = transform.localScale;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isPlaced) return;

        var player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.SetLadderMode(true, ladderMoveSpeed);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isPlaced) return;

        var player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.SetLadderMode(false, 0f);
        }
    }

    public override void 아이템_장착()
    {
        base.아이템_장착();

        //if (audioSource != null && 장착효과음 != null)
        //    audioSource.PlayOneShot(장착효과음);

        isPlaced = false;
        if (ladderCollider != null)
            ladderCollider.enabled = false; // 들고 있을 때는 충돌 비활성화
        transform.localScale = originalScale;
    }

    public override void 아이템_해제()
    {
        base.아이템_해제();

        //if (audioSource != null && 해제효과음 != null)
        //    audioSource.PlayOneShot(해제효과음);

        isPlaced = true;
        if (ladderCollider != null)
            ladderCollider.enabled = true; // 내려놓으면 충돌 활성화
        transform.localScale = originalScale;

        // 항상 똑바로 세우기 (Y축 회전값을 0으로 고정)
        transform.rotation = Quaternion.Euler(0, 0, 0);

        // 필요하다면 약간 바닥 위로 올려주기
        var pos = transform.position;
        transform.position = new Vector3(pos.x, pos.y + 0.01f, pos.z);
    }


    public override bool 던질수있는가 => false;
}
