using UnityEngine;

public abstract class BaseItem : MonoBehaviour, IInteractable
{
    public virtual 상호작용_타입 상호작용_종류 => 상호작용_타입.아이템획득;
    public virtual 상호작용_방식 상호작용_방식 => 상호작용_방식.즉시;

    [SerializeField] protected string 아이템_이름;
    [SerializeField] protected string 아이템_설명;

    public Vector3 장착_위치 = new Vector3(0, 0, 0.3f);
    public Vector3 장착_회전 = new Vector3(0, 0, 0);
    //public Vector3 장착_스케일 = Vector3.one;


    [Header("효과음")]
    public AudioClip 장착효과음;
    public AudioClip 해제효과음;
    protected AudioSource audioSource;

    public virtual float? 이동속도_제한값 => null;

    public bool 버려졌는가 { get; set; } = false;

    public Vector3 원래_스케일;

    [Header("도구 초기 위치")]
    public Transform 초기위치;

    public virtual bool 던질수있는가 => true;


    protected virtual void Awake()
    {
        원래_스케일 = transform.localScale;
        audioSource = GetComponent<AudioSource>();
    }

    public virtual void 상호작용_시작()
    {
        // 아이템 상호작용 기본 동작
        Debug.Log($"{아이템_이름} 획득 시도");
        // 플레이어에게 자기 자신을 넘김
        var 플레이어 = Object.FindAnyObjectByType<PlayerController>();
        if (플레이어 != null)
        {
            플레이어.아이템_획득(this);
        }
    }

    // 즉시 상호작용이므로 아래 메서드는 비워둬도 됨
    public virtual void 상호작용_유지(float 유지시간) { }
    public virtual void 상호작용_종료() { }

    public virtual void 아이템_장착()
    {
        //ManagerRoomCollider.InsideTrashObjects.Remove(gameObject);


        if (audioSource != null && 장착효과음 != null)
            audioSource.PlayOneShot(장착효과음);

        var colliders = GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true; // 물리 계산도 안 하게
        }
        transform.localScale = 원래_스케일;

    }

    public virtual void 아이템_해제()
    {

        if (audioSource != null && 해제효과음 != null)
            audioSource.PlayOneShot(해제효과음);

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        // 해제 시 모든 콜라이더를 찾아 활성화
        var colliders = GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = true;
        }
        transform.localScale = 원래_스케일;

    }
    public virtual void 도구사용()
    {
        // 기본 아이템은 아무 행동도 하지 않음
    }

}