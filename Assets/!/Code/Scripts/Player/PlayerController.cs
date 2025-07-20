using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    CharacterController 컨트롤러;

    [Header("카메라 관련")]
    public GameObject 카메라;
    private Vector3 카메라_원위치;
    private Vector3 카메라_타겟위치;
    [SerializeField] float 시선_감도 = 2f;
    [SerializeField] float 상하_시선제한 = 80f;
    float 현재_상하_시선 = 0f;
    [SerializeField] private float 카메라_최대_기울기 = 3f; // 최대 Z축 기울기(도)
    [SerializeField] private float 카메라_기울기_속도 = 8f; // 복귀 속도
    private float 현재_카메라_기울기 = 0f;
    [SerializeField] private float 카메라_기울기_민감도 = 1.5f; // 입력값 대비 기울기 민감도(조절용)
    [SerializeField] private bool 카메라_흔들림_사용 = true;

    [Header("이동 관련")]
    [SerializeField] float 기본_이동_속도 = 3f;
    [SerializeField] float 달리기_속도 = 6f;
    float 현재_이동_속도;


    [Header("앉기 관련")]
    [SerializeField] private float 앉기_높이 = 1f;
    [SerializeField] private float 앉기_이동속도 = 8f;
    //[SerializeField] private float 앉기_RightHand_오프셋 = 0.2f; // 인스펙터에서 조절
    private Vector3 RightHand_원위치;

    [Header("점프 관련")]
    [SerializeField] float 중력 = -9.81f;
    [SerializeField] float 점프_힘 = 5f;
    [SerializeField] float 점프_중력_가속 = 2f;


    [Header("던지기 관련")]
    [SerializeField] private float 던지는힘 = 5f;
    [SerializeField] private float 회전힘 = 15f;

    [Header("상호작용 관련")]
    public BaseItem 현재_들고있는_아이템;
    [SerializeField] public float 상호작용_거리 = 2f;
    [SerializeField] float 상호작용_반경 = 0.4f;
    [SerializeField] public LayerMask 상호작용_레이어;
    bool 상호작용_중 = false;
    float 상호작용_누름_시간 = 0f;
    IInteractable 현재_상호작용_객체;
    [SerializeField] public Transform RightHand;
    [SerializeField] private Bat 삼단봉;
    private bool 삼단봉활성화됨 = true;

    private bool 공격_상호작용_중 = false;
    private Fire 공격_대상_불 = null;
    private float 공격_누름_시간 = 0f;


    Vector3 속도;

    InputSystem_Actions inputActions;
    Vector2 움직임_입력;
    Vector2 시선_입력;
    Vector3 이동_방향;

    Transform 위치;

    //사다리 관련
    private bool isOnLadder = false;
    private float ladderSpeed = 0f;
    //private bool isLadderJumping = false;

    private bool 이동_제한됨 = false;

    [Header("이동 효과음")]
    public AudioClip 걷는소리;
    public AudioClip 뛰는소리;
    public float 걷는소리_간격 = 0.5f;   // 걷기 소리 반복 간격(초)
    public float 뛰는소리_간격 = 0.35f;  // 뛰기 소리 반복 간격(초)
    public AudioClip 점프소리;    // 추가: 점프 사운드
    public AudioClip 착지소리;    // 추가: 착지 사운드
    private bool 이전_지면상태 = true;

    private AudioSource 이동AudioSource;
    private AudioSource 점프AudioSource; // 점프/착지 전용
    private float 이동소리_타이머 = 0f;
    private bool 이전_이동중 = false;
    private bool 이전_달리기 = false;


    void Awake()
    {
        위치 = transform;
        이동_방향 = Vector3.zero;
        inputActions = new InputSystem_Actions();
        컨트롤러 = GetComponent<CharacterController>();

        이동AudioSource = gameObject.AddComponent<AudioSource>();
        이동AudioSource.loop = false;
        이동AudioSource.playOnAwake = false;
        이동AudioSource.volume = 0.5f; // 볼륨 강제 지정

        점프AudioSource = gameObject.AddComponent<AudioSource>();
        점프AudioSource.loop = false;
        점프AudioSource.playOnAwake = false;
        점프AudioSource.volume = 0.5f; // 볼륨 강제 지정

        inputActions.Player.Move.performed += 이동시작;
        inputActions.Player.Move.canceled += 이동끝;
        inputActions.Player.Look.performed += 시선이동시작;
        inputActions.Player.Look.canceled += 시선이동끝;
        inputActions.Player.Interact.started += _ => 상호작용_버튼_누름();
        inputActions.Player.Interact.canceled += _ => 상호작용_버튼_뗌();
        inputActions.Player.Attack.started += _ => 현재_들고있는_아이템?.도구사용();
        inputActions.Player.Attack.started += _ => Attack_상호작용_시작();
        inputActions.Player.Attack.canceled += _ => Attack_상호작용_종료();


        inputActions.Player.Throw.started += _ => 아이템_던지기();

        inputActions.Player.Bat.started += _ => 삼단봉_();

        inputActions.Player.Run.started += _ => 현재_이동_속도 = 달리기_속도;
        inputActions.Player.Run.canceled += _ => 현재_이동_속도 = 기본_이동_속도;
        현재_이동_속도 = 기본_이동_속도;

        카메라_원위치 = 카메라.transform.localPosition;
        카메라_타겟위치 = 카메라_원위치;
        inputActions.Player.SitDown.started += _ => 앉기();
        inputActions.Player.SitDown.canceled += _ => 앉기_해제();

            if (RightHand != null)
        RightHand_원위치 = RightHand.localPosition;

        // 마우스 포인터 끄기
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Start()
    {
        // ---- 옵션 시스템에서 흔들림 옵션 받아오기 ---- 이태영
        if (OptionManager.인스턴스 != null)
        {
            카메라_흔들림_사용 = OptionManager.인스턴스.화면흔들림허용;
            OptionManager.인스턴스.옵션변경이벤트 += 옵션변경반영;
        }
    }

    void OnDestroy()
    {
        // ---- 이벤트 해제 (메모리 누수 방지) ---- 이태영
        if (OptionManager.인스턴스 != null)
            OptionManager.인스턴스.옵션변경이벤트 -= 옵션변경반영;
    }

    // ---- 옵션 변경시 호출될 함수 추가 ---- 이태영
    private void 옵션변경반영()
    {
        if (OptionManager.인스턴스 != null)
            카메라_흔들림_사용 = OptionManager.인스턴스.화면흔들림허용;
    }

    private void 이동시작(InputAction.CallbackContext context)
    {
        움직임_입력 = context.ReadValue<Vector2>();
    }
    private void 이동끝(InputAction.CallbackContext context)
    {
        움직임_입력 = Vector2.zero;
    }
    private void 시선이동시작(InputAction.CallbackContext context)
    {
        시선_입력 = context.ReadValue<Vector2>();
    }
    private void 시선이동끝(InputAction.CallbackContext context)
    {
        시선_입력 = Vector2.zero;
    }

    void OnEnable() => inputActions.Player.Enable();
    void OnDisable()
    {
        inputActions.Player.Disable();

        // 이벤트 구독 해제
        inputActions.Player.Move.performed -= 이동시작;
        inputActions.Player.Move.canceled -= 이동끝;
        inputActions.Player.Look.performed -= 시선이동시작;
        inputActions.Player.Look.canceled -= 시선이동끝;

        inputActions.Player.Run.started -= _ => 현재_이동_속도 = 달리기_속도;
        inputActions.Player.Run.canceled -= _ => 현재_이동_속도 = 기본_이동_속도;

    }





    void Update()
    {
        if (isOnLadder)
        {
            LadderMove();
            시선이동();
            상호작용();
            디버그();
            카메라.transform.localPosition = Vector3.Lerp(카메라.transform.localPosition, 카메라_타겟위치, Time.deltaTime * 앉기_이동속도);
            StopMoveSound();
            return;
        }

        if (공격_상호작용_중 && 공격_대상_불 != null)
        {
            공격_누름_시간 += Time.deltaTime;
            공격_대상_불.상호작용_유지(Time.deltaTime);
            if (공격_누름_시간 >= 3f)
            {
                Attack_상호작용_종료();
            }
        }

        이동();
        시선이동();
        상호작용();
        디버그();

        카메라.transform.localPosition = Vector3.Lerp(카메라.transform.localPosition, 카메라_타겟위치, Time.deltaTime * 앉기_이동속도);

        UpdateMoveSound(); // 추가: 이동 효과음 관리
    }

    private void UpdateMoveSound()
    {
        // 이동 입력이 없거나 이동 제한, 공중에 있으면 소리 정지 및 타이머 리셋
        bool isMoving = 움직임_입력.sqrMagnitude > 0.01f && !이동_제한됨 && 컨트롤러.isGrounded;
        if (!isMoving)
        {
            이동소리_타이머 = 0f;
            이전_이동중 = false;
            이전_달리기 = false;
            StopMoveSound();
            return;
        }

        // 달리기 중인지 판별
        bool isRunning = Mathf.Approximately(현재_이동_속도, 달리기_속도);
        AudioClip targetClip = isRunning ? 뛰는소리 : 걷는소리;
        float interval = isRunning ? 뛰는소리_간격 : 걷는소리_간격;

        // 이동 시작 시 바로 한 번 재생
        if (!이전_이동중 || 이전_달리기 != isRunning)
        {
            if (targetClip != null)
                이동AudioSource.PlayOneShot(targetClip, 0.3f); // 볼륨 0.5로 재생
            이동소리_타이머 = 0f;
            이전_이동중 = true;
            이전_달리기 = isRunning;
            return;
        }

        // 타이머로 반복 재생
        이동소리_타이머 += Time.deltaTime;
        if (이동소리_타이머 >= interval)
        {
            if (targetClip != null)
                이동AudioSource.PlayOneShot(targetClip, 0.3f); // 볼륨 0.5로 재생
            이동소리_타이머 = 0f;
        }
    }


    private void StopMoveSound()
    {
        if (이동AudioSource.isPlaying)
            이동AudioSource.Stop();
    }



    public void 이동()
    {
        if (!컨트롤러.enabled || 이동_제한됨)
            return;

        // 이동 속도 제한 적용
        float 적용_이동_속도 = 현재_이동_속도;
        if (현재_들고있는_아이템 != null && 현재_들고있는_아이템.이동속도_제한값.HasValue)
            적용_이동_속도 = 현재_들고있는_아이템.이동속도_제한값.Value;

        // 이동 방향 계산
        이동_방향 = transform.right * 움직임_입력.x + transform.forward * 움직임_입력.y;
        이동_방향 = 이동_방향.normalized * 적용_이동_속도;

        // 중력 적용
        if (!컨트롤러.isGrounded)
        {
            if (isOnLadder)
            {
                속도.y += (중력 * 0.2f) * Time.deltaTime;
            }
            else
            {
                속도.y += 중력 * Time.deltaTime;
                속도.y -= 점프_중력_가속 * Time.deltaTime;
            }
        }
        else
        {
            if (속도.y < 0)
            {
                속도.y = -2f;
            }

            // 점프 입력 처리
            if (inputActions.Player.Jump.triggered)
            {
                속도.y = Mathf.Sqrt(점프_힘 * -2f * 중력);
                // 점프 사운드
                if (점프AudioSource != null && 점프소리 != null)
                    점프AudioSource.PlayOneShot(점프소리);
            }
        }

        // 착지 사운드: 이전 프레임에서 공중, 이번 프레임에서 지면
        if (!이전_지면상태 && 컨트롤러.isGrounded)
        {
            if (점프AudioSource != null && 착지소리 != null)
                점프AudioSource.PlayOneShot(착지소리);
        }
        이전_지면상태 = 컨트롤러.isGrounded;

        // 최종 이동벡터 계산
        Vector3 이동벡터 = 이동_방향;
        이동벡터.y = 속도.y;

        // 이동 적용
        컨트롤러.Move(이동벡터 * Time.deltaTime);
    }



    public void 시선이동()
    {
        // 일시정지 상태면 시선 입력 처리 중단
        if (PauseMenuController.인스턴스 != null && PauseMenuController.인스턴스.일시정지상태)
        {
            Debug.Log("일시정지 상태: 시선 입력 처리 중단");
            return;
        }

        float 좌우_회전 = 시선_입력.x * 시선_감도;
        위치.Rotate(Vector3.up * 좌우_회전);

        float 상하_회전 = 시선_입력.y * 시선_감도;
        현재_상하_시선 = Mathf.Clamp(현재_상하_시선 - 상하_회전, -상하_시선제한, 상하_시선제한);

        if (카메라_흔들림_사용)
        {
            // --- Z축 기울기 계산 (시선 + 이동 입력 모두 반영) ---
            float 입력값 = 시선_입력.x + 움직임_입력.x;
            float 입력_정규화 = Mathf.Clamp01(Mathf.Abs(입력값) * 카메라_기울기_민감도);
            float 목표_기울기 = -Mathf.Sign(입력값) * 입력_정규화 * 카메라_최대_기울기;

            // 입력이 없으면 0으로 복귀
            if (Mathf.Approximately(입력값, 0f))
                목표_기울기 = 0f;

            현재_카메라_기울기 = Mathf.Lerp(현재_카메라_기울기, 목표_기울기, Time.deltaTime * 카메라_기울기_속도);
        }
        else
        {
            현재_카메라_기울기 = 0f;
        }

        카메라.transform.localRotation = Quaternion.Euler(현재_상하_시선, 0f, 현재_카메라_기울기);
    }



    private void 앉기()
    {
        isSitting = true;
        카메라_타겟위치 = 카메라_원위치 + Vector3.down * 앉기_높이;
    }

    private void 앉기_해제()
    {
        isSitting = false;
        카메라_타겟위치 = 카메라_원위치;
    }

    public void SetMoveBlocked(bool blocked)
    {
        이동_제한됨 = blocked;
    }

    #region 상호작용
    private void 상호작용()
    {
        if (상호작용_중 && 현재_상호작용_객체 != null && 현재_상호작용_객체.상호작용_방식 == 상호작용_방식.누르고_있기)
        {
            상호작용_누름_시간 += Time.deltaTime;
            현재_상호작용_객체.상호작용_유지(상호작용_누름_시간);

            // 3초 이상 누르면 완료
            if (상호작용_누름_시간 >= 3f)
            {
                현재_상호작용_객체.상호작용_종료();
                상호작용_중 = false;
                상호작용_누름_시간 = 0f;
                현재_상호작용_객체 = null;
            }
        }
    }
    void 상호작용_버튼_누름()
    {
        Ray ray = new Ray(카메라.transform.position, 카메라.transform.forward);

        if (Physics.SphereCast(ray, 상호작용_반경, out RaycastHit hit, 상호작용_거리, 상호작용_레이어))
        {
            Debug.Log("▶ 감지된 오브젝트: " + hit.collider.name);


            if (hit.collider.gameObject == 현재_들고있는_아이템?.gameObject)
                return;

            var interactable = hit.collider.GetComponentInParent<IInteractable>();

            // WaterMop 사용 제한: Graffiti에만 사용 가능
            if (현재_들고있는_아이템 is WaterMop)
            {
                // Graffiti 스크립트가 붙어있는지 확인
                if (hit.collider.GetComponentInParent<Graffiti>() == null)
                {
                    Debug.Log("🚫 WaterMop은 Graffiti에만 사용할 수 있습니다.");
                    return;
                }
            }

            if (interactable != null)
            {
                Debug.Log("🎯 감지된 인터랙터블: " + interactable.GetType().Name);

                // Sink 상호작용 처리

                if (interactable is Sink)
                {
                    Debug.Log("🚫 Sink는 E 키로 상호작용할 수 없습니다.");
                    return;
                }

                // 기존 로직 유지
                if (interactable is TrashCan 쓰레기통 && 현재_들고있는_아이템 is Trash 쓰레기)
                {
                    아이템_놓기();
                    return;
                }

                if (interactable is BaseItem 새로운아이템)
                {
                    if (현재_들고있는_아이템 != null)
                        아이템_놓기();

                    아이템_획득(새로운아이템);
                    return;
                }

                //if (hit.collider.CompareTag("Graffiti") && 현재_들고있는_아이템 is WaterMop)
                //{
                //    return;
                //}

                // 이 조건이 충족되면 puke(구토)를 인식해도 return 되어 상호작용_시작()이 호출되지 않아서 잠시 주석 처리
                //if (hit.collider.CompareTag("Puke") && 현재_들고있는_아이템 is Mop)
                //{
                //    return;
                //}

                if (interactable.상호작용_방식 == 상호작용_방식.즉시)
                {
                    interactable.상호작용_시작();
                }
                else if (interactable.상호작용_방식 == 상호작용_방식.누르고_있기)
                {
                    상호작용_중 = true;
                    상호작용_누름_시간 = 0f;
                    현재_상호작용_객체 = interactable;
                    interactable.상호작용_시작();
                }
            }
            else if (현재_들고있는_아이템 != null)
            {
                아이템_놓기();
            }
        }
        else if (현재_들고있는_아이템 != null)
        {
            아이템_놓기();
        }
    }




    void 상호작용_버튼_뗌()
    {
        if (상호작용_중 && 현재_상호작용_객체 != null &&
            현재_상호작용_객체.상호작용_방식 == 상호작용_방식.누르고_있기)
        {
            // 3초 전에 뗐다면 실패 처리
            현재_상호작용_객체.상호작용_종료();
            상호작용_중 = false;
            상호작용_누름_시간 = 0f;
            현재_상호작용_객체 = null;
        }
    }
    #endregion

    [SerializeField] LayerMask 바닥_레이어; // 인스펙터에서 Ground만 선택
    private bool isSitting = false;


    private void RightHand_바닥위치_보정(float 최소높이 = 0.5f)
    {
        if (RightHand == null) return;

        RaycastHit hit;
        Vector3 origin = RightHand.position;
        // 바닥까지 Raycast
        if (Physics.Raycast(origin, Vector3.down, out hit, 2f, 바닥_레이어))
        {
            float 거리 = origin.y - hit.point.y;
            if (거리 < 최소높이)
            {
                // RightHand를 바닥 위로 올림
                Vector3 보정 = Vector3.up * (최소높이 - 거리);
                RightHand.position += 보정;
            }
        }
    }

    private void Attack_상호작용_시작()
    {
        // 카메라 앞에 Fire가 있는지 확인
        Ray ray = new Ray(카메라.transform.position, 카메라.transform.forward);
        if (Physics.SphereCast(ray, 상호작용_반경, out RaycastHit hit, 상호작용_거리, 상호작용_레이어))
        {
            var fire = hit.collider.GetComponentInParent<Fire>();
            if (fire != null && 현재_들고있는_아이템 is FireExtinguisher)
            {
                공격_상호작용_중 = true;
                공격_대상_불 = fire;
                공격_누름_시간 = 0f;
                fire.상호작용_시작();
            }
        }
    }

    private void Attack_상호작용_종료()
    {
        if (공격_상호작용_중 && 공격_대상_불 != null)
        {
            공격_대상_불.상호작용_종료();
            공격_상호작용_중 = false;
            공격_대상_불 = null;
            공격_누름_시간 = 0f;
        }
    }


    public void 아이템_획득(BaseItem item)
    {
        if (isSitting)  RightHand_바닥위치_보정();

        if (현재_들고있는_아이템 != null)
        {
            // 기존 아이템 바닥에 놓기
            현재_들고있는_아이템.transform.SetParent(null);
            현재_들고있는_아이템.transform.position = transform.position + transform.forward;
            현재_들고있는_아이템.아이템_해제();
        }

        // 새 아이템 장착
        현재_들고있는_아이템 = item;
        // 아이템 획득 시
        if (RightHand != null)
        {
            item.transform.SetParent(RightHand, false); // 반드시 false로!
            item.transform.localPosition = item.장착_위치;
            item.transform.localRotation = Quaternion.Euler(item.장착_회전);
        }

        item.아이템_장착();
        // 인벤토리 추가 등 기타 처리


    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ManagerRoom"))
        {
            if (현재_들고있는_아이템 != null && 현재_들고있는_아이템.CompareTag("Trash"))
            {
                ManagerRoomCollider.InsideTrashObjects.Remove(현재_들고있는_아이템.gameObject);
                Debug.Log($"[PlayerController] 플레이어가 Trash 아이템을 들고 관리실에서 나감: {현재_들고있는_아이템.name}");
                ManagerRoomCollider.PrintInsideTrashObjects();
            }
        }
    }

    public void 아이템_놓기()
    {
        if (현재_들고있는_아이템 == null) return;

        현재_들고있는_아이템.transform.parent = null;

        // RightHand 위치에서 바로 떨어트림
        Vector3 놓는위치 = RightHand != null ? RightHand.position : transform.position;

        // 필요하다면 약간 위로 올려주기 (충돌 방지)
        놓는위치 += Vector3.up * 0.05f;

        현재_들고있는_아이템.transform.position = 놓는위치;

        // Rigidbody 초기화
        Rigidbody rb = 현재_들고있는_아이템.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        현재_들고있는_아이템.아이템_해제();
        현재_들고있는_아이템 = null;
    }






    public void 아이템_던지기()
    {
        if (현재_들고있는_아이템 == null) return;
        if (!현재_들고있는_아이템.던질수있는가) return;
        if (현재_들고있는_아이템 == 삼단봉) return;

        현재_들고있는_아이템.transform.SetParent(null);

        // RightHand 위치에서 던지기
        Vector3 던질위치 = RightHand != null ? RightHand.position : transform.position;

        // 필요하다면 약간 위로 올려주기 (충돌 방지)
        던질위치 += Vector3.up * 0.05f;

        현재_들고있는_아이템.transform.position = 던질위치;

        // Rigidbody 초기화 및 던지기
        Rigidbody rb = 현재_들고있는_아이템.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false; // 먼저 kinematic 해제
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(카메라.transform.forward * 던지는힘, ForceMode.VelocityChange);
            Vector3 회전축 = Random.onUnitSphere;
            rb.AddTorque(회전축 * 회전힘, ForceMode.VelocityChange);
        }

        현재_들고있는_아이템.아이템_해제();
        현재_들고있는_아이템 = null;
    }



    void 삼단봉_()
    {
        if (현재_들고있는_아이템 == 삼단봉)
        {
            // 삼단봉 비활성화 (사라지게만 함)
            삼단봉.gameObject.SetActive(false);
            현재_들고있는_아이템 = null;
            삼단봉활성화됨 = false;
            return;
        }

        // 다른 아이템을 들고 있다면 놓기
        if (현재_들고있는_아이템 != null)
        {
            아이템_놓기();
        }

        // 삼단봉 다시 장착
        현재_들고있는_아이템 = 삼단봉;
        삼단봉.gameObject.SetActive(true);
        삼단봉.transform.SetParent(RightHand);
        삼단봉.transform.localPosition = 삼단봉.장착_위치;
        삼단봉.transform.localRotation = Quaternion.Euler(삼단봉.장착_회전);
        삼단봉.아이템_장착();
        삼단봉활성화됨 = true;
    }

    public void SetLadderMode(bool on, float speed)
    {
        isOnLadder = on;
        ladderSpeed = speed;
        if (on)
        {
            // 사다리 모드 진입: 중력 영향 제거, 컨트롤러 비활성화
            속도.y = 0f;
        }
        else
        {
        }
        Debug.Log($"사다리 모드: {(on ? "활성화" : "비활성화")}, 속도: {speed}");
    }
    private void LadderMove()
    {
        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();
        Vector3 move = transform.right * input.x + transform.up * input.y;
        move = move.normalized * ladderSpeed;

        // 점프 입력 처리: 점프 버튼을 누르면 위로 점프 힘만큼 이동 (한 프레임만)
        if (inputActions.Player.Jump.triggered)
        {
            속도.y = Mathf.Sqrt(점프_힘 * -2f * 중력);
        }
        else if (Mathf.Abs(input.y) > 0.01f)
        {
            // W/S(위/아래) 입력이 있을 때는 중력 미적용
            속도.y = 0f;
        }
        else
        {
            // 입력 없으면 약한 중력 적용
            속도.y += (중력 * 0.2f) * Time.deltaTime;
        }

        // 점프 후에는 중력 정상 적용 (점프 후 W/S 입력 전까지)
        if (속도.y > 0f && !inputActions.Player.Jump.triggered && Mathf.Abs(input.y) < 0.01f)
        {
            속도.y += 중력 * Time.deltaTime;
        }

        // 바닥에 닿으면 속도 리셋
        if (컨트롤러.isGrounded && 속도.y < 0f)
        {
            속도.y = -2f;
        }

        move.y += 속도.y;
        컨트롤러.Move(move * Time.deltaTime);
    }


    #region 디버그
    private void 디버그()
    {
        레이캐스트_디버그();
    }
    private void 레이캐스트_디버그()
    {
        Ray ray = new Ray(카메라.transform.position, 카메라.transform.forward);

        // 항상 빨간색 선으로 레이캐스트 표시
        Debug.DrawRay(ray.origin, ray.direction * 상호작용_거리, Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit, 상호작용_거리, 상호작용_레이어))
        {
            // 충돌 지점까지 녹색 선으로 표시
            Debug.DrawLine(ray.origin, hit.point, Color.green);
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Ray ray = new Ray(카메라.transform.position, 카메라.transform.forward);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(카메라.transform.position, 0.1f); // 레이 시작점
        Gizmos.DrawRay(ray);

        if (Physics.Raycast(ray, out RaycastHit hit, 상호작용_거리, 상호작용_레이어))
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(hit.point, 상호작용_반경); // 충돌 지점
        }
    }
    #endregion
}
