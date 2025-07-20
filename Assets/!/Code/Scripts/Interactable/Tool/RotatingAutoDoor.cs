using UnityEngine;

public class RotatingAutoDoor : MonoBehaviour
{
    [Header("문 그래픽 오브젝트(회전 대상)")]
    public Transform doorGraphic;

    [Header("문 콜라이더(문이 열릴 때 꺼짐)")]
    public Collider doorCollider;

    [Header("문 회전 설정")]
    public float openAngle = 90f; // 열릴 때 회전 각도
    public float openSpeed = 120f; // 초당 회전 속도(도/초)
    public float closeDelay = 0.5f; // 플레이어가 떠난 뒤 닫히기까지 대기 시간

    [Header("회전축 (기본 Y축)")]
    public Vector3 rotationAxis = Vector3.up;

    [Header("효과음")]
    public AudioClip openSound;
    public AudioClip closeSound;

    private AudioSource audioSource;

    private Quaternion closedRot;
    private Quaternion openRot;
    private bool isOpen = false;
    private bool isPlayerOnTrigger = false;
    private float closeTimer = 0f;

    // 효과음 중복 방지용 상태
    private bool wasOpen = false;
    private bool wasClosed = true;

    void Start()
    {
        if (doorGraphic == null) doorGraphic = transform;
        closedRot = doorGraphic.localRotation;
        openRot = closedRot * Quaternion.AngleAxis(openAngle, rotationAxis.normalized);

        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isOpen)
        {
            doorGraphic.localRotation = Quaternion.RotateTowards(
                doorGraphic.localRotation, openRot, openSpeed * Time.deltaTime);
            if (doorCollider != null) doorCollider.enabled = false;

            // 열림 효과음 (처음 열릴 때 1회만)
            if (!wasOpen)
            {
                if (audioSource != null && openSound != null)
                    audioSource.PlayOneShot(openSound);
                wasOpen = true;
                wasClosed = false;
            }
        }
        else
        {
            doorGraphic.localRotation = Quaternion.RotateTowards(
                doorGraphic.localRotation, closedRot, openSpeed * Time.deltaTime);
            // 닫힌 각도에 거의 도달하면 콜라이더 켜기
            if (doorCollider != null && Quaternion.Angle(doorGraphic.localRotation, closedRot) < 1f)
                doorCollider.enabled = true;

            // 닫힘 효과음 (거의 닫힐 때 1회만)
            if (!wasClosed && Quaternion.Angle(doorGraphic.localRotation, closedRot) < 10f)
            {
                if (audioSource != null && closeSound != null)
                    audioSource.PlayOneShot(closeSound);
                wasClosed = true;
                wasOpen = false;
            }

        }

        // 플레이어가 트리거에서 떠난 후 딜레이 후 닫힘
        if (!isPlayerOnTrigger && isOpen)
        {
            closeTimer += Time.deltaTime;
            if (closeTimer >= closeDelay)
            {
                isOpen = false;
                closeTimer = 0f;
            }
        }
    }

    // 바닥 트리거 콜라이더에 부착
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerOnTrigger = true;
            isOpen = true;
            closeTimer = 0f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerOnTrigger = false;
            closeTimer = 0f;
        }
    }
}
