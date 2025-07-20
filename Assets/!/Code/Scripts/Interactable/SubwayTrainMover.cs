using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SubwayTrainMover : MonoBehaviour
{
    [Header("열차 오브젝트")]
    public GameObject trainObject;

    [Header("열차 트리거 콜라이더")]
    public Collider trainCollider;

    [Header("탑승시 부모로 삼을 오브젝트 (비워두면 열차 오브젝트)")]
    public Transform parentOnBoard;

    [Header("이동 포인트")]
    public Transform spawnPoint;
    public Transform stopPoint;
    public Transform destroyPoint;

    [Header("이동 속도")]
    public float moveSpeed = 30f;
    public float minSpeed = 5f;
    public float maxSpeed = 30f;

    [Header("정차 시간(초)")]
    public float stopDuration = 5f;

    [Header("문 열기/닫기 대기 시간(초)")]
    public float waitBeforeOpen = 1f;
    public float waitAfterClose = 1f;

    [Header("열차 자동 스폰 설정")]
    public float initialSpawnDelay = 5f;
    public float spawnInterval = 10f;

    private SubwayDoor trainDoor;
    private bool isTrainActive = false;

    private HashSet<Transform> attachedObjects = new HashSet<Transform>();
    private bool isPlayerOnTrain = false; // 플레이어 탑승 여부
    private bool playerWasOnTrain = false; // 플레이어가 한 번이라도 들어온 적이 있는지
    private float playerExitTime = -1f;    // 플레이어가 마지막으로 나간 시간

    [Header("스크린 도어")]
    public NavMeshObstacle obstacle;

    private void Start()
    {
        if (trainObject != null)
        {
            trainObject.SetActive(false);
            trainDoor = trainObject.GetComponentInChildren<SubwayDoor>(true);
            if (trainDoor != null)
                trainDoor.enabled = false;
        }
        StartCoroutine(AutoSpawnRoutine());
    }

    private void Update()
    {
        // 플레이어 오브젝트를 찾아서
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && trainCollider != null)
        {
            Collider playerCol = player.GetComponent<Collider>();
            if (playerCol != null)
            {
                bool nowOnTrain = trainCollider.bounds.Intersects(playerCol.bounds);
                if (nowOnTrain)
                {
                    isPlayerOnTrain = true;
                    playerWasOnTrain = true;
                    playerExitTime = -1f; // 타이머 리셋
                }
                else
                {
                    isPlayerOnTrain = false;
                }
            }
        }
    }

    private IEnumerator AutoSpawnRoutine()
    {
        yield return new WaitForSeconds(initialSpawnDelay);

        while (true)
        {
            SpawnAndMoveTrain();
            yield return StartCoroutine(WaitForTrainToBeDeactivated());
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private IEnumerator WaitForTrainToBeDeactivated()
    {
        while (isTrainActive)
            yield return null;
    }

    [ContextMenu("열차 이동 시작")]
    public void SpawnAndMoveTrain()
    {
        if (isTrainActive)
        {
            Debug.LogWarning("이미 열차가 움직이고 있습니다.");
            return;
        }

        if (trainObject == null || spawnPoint == null || stopPoint == null || destroyPoint == null)
        {
            Debug.LogError("필수 값이 할당되지 않았습니다.");
            return;
        }

        // 열차 위치 초기화 및 활성화
        trainObject.transform.position = spawnPoint.position;
        trainObject.transform.rotation = spawnPoint.rotation;
        trainObject.SetActive(true);
        isTrainActive = true;

        if (trainDoor != null)
            trainDoor.enabled = false;

        StartCoroutine(MoveRoutine());
    }

    private IEnumerator MoveRoutine()
    {
        // 정차 위치까지 이동
        yield return StartCoroutine(MoveToWithSpeedControl(stopPoint.position, accelerate: false));
        yield return new WaitForSeconds(waitBeforeOpen);

        // 문 열기
        if (trainDoor != null)
        {
            trainDoor.enabled = true;
            trainDoor.Open();

            NPCManager.인스턴스.지하철_소환(trainDoor.reverseOpenDirection);

            yield return new WaitForSeconds(GetDoorMoveDuration());

            obstacle.enabled = false;
        }

        // 플레이어가 한 번이라도 들어온 적이 있으면, 나간 후 1초 대기
        if (playerWasOnTrain)
        {
            // 플레이어가 열차에 있으면 대기
            while (isPlayerOnTrain)
            {
                yield return null;
            }

            // 플레이어가 나간 후 1초 대기
            float waitStart = Time.time;
            while (Time.time - waitStart < 1f)
            {
                // 1초 대기 중에 다시 들어오면 대기 루프 재시작
                if (isPlayerOnTrain)
                {
                    // 다시 들어오면 대기 루프 재시작
                    while (isPlayerOnTrain)
                        yield return null;
                    waitStart = Time.time;
                }
                yield return null;
            }
        }
        else
        {
            // 기존 로직: 플레이어가 한 번도 들어온 적 없으면 그냥 대기
            while (isPlayerOnTrain)
            {
                yield return null;
            }
        }

        yield return new WaitForSeconds(stopDuration);

        // 문 닫기
        if (trainDoor != null)
        {
            obstacle.enabled = true;

            trainDoor.Close();
            yield return new WaitForSeconds(GetDoorMoveDuration());
            trainDoor.enabled = false;

            GetComponentInChildren<TrainTrigger>().컴포넌트_비활성화();
        }

        yield return new WaitForSeconds(waitAfterClose);

        // 출발
        yield return StartCoroutine(MoveToWithSpeedControl(destroyPoint.position, accelerate: true));

        trainObject.SetActive(false);
        trainObject.transform.position = spawnPoint.position;
        trainObject.transform.rotation = spawnPoint.rotation;
        isTrainActive = false;

        // 다음 열차를 위해 상태 초기화
        playerWasOnTrain = false;
        playerExitTime = -1f;
    }

    private IEnumerator MoveToWithSpeedControl(Vector3 target, bool accelerate)
    {
        Vector3 start = trainObject.transform.position;
        float startDistance = Vector3.Distance(start, target);

        while (Vector3.Distance(trainObject.transform.position, target) > 0.01f)
        {
            float currentDistance = Vector3.Distance(trainObject.transform.position, target);
            float t = Mathf.Clamp01(currentDistance / startDistance);

            float speed = accelerate
                ? Mathf.Lerp(minSpeed, maxSpeed, 1f - t)
                : Mathf.Lerp(minSpeed, maxSpeed, t);

            trainObject.transform.position = Vector3.MoveTowards(
                trainObject.transform.position, target, speed * Time.deltaTime);
            yield return null;
        }
        trainObject.transform.position = target;
    }

    private float GetDoorMoveDuration()
    {
        if (trainDoor == null) return 1f;
        return trainDoor.openDistance / Mathf.Max(trainDoor.moveSpeed, 0.01f);
    }

    public bool IsTrainAlive()
    {
        return isTrainActive;
    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[SubwayTrainMover] OnTriggerEnter: {other.name}");

        if (trainCollider != null && other == trainCollider)
            return;

        if (other.CompareTag("NPC")) return;

        // 플레이어가 들어오면 플래그 설정
        if (other.CompareTag("Player"))
        {
            isPlayerOnTrain = true;
            playerWasOnTrain = true;
            playerExitTime = -1f;
        }

        Transform parent = parentOnBoard != null ? parentOnBoard : trainObject.transform;

        if (other.transform != parent && other.attachedRigidbody != null)
        {
            if (other.transform.parent != parent)
            {
                attachedObjects.Add(other.transform);
                other.transform.SetParent(parent, true);

                // 콜라이더를 잠시 껐다 켜서 트리거 상태를 리셋
                var colliders = other.GetComponentsInChildren<Collider>();
                foreach (var col in colliders)
                {
                    col.enabled = false;
                }
                foreach (var col in colliders)
                {
                    col.enabled = true;
                }
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (trainCollider != null && other == trainCollider)
            return;

        // 플레이어가 나가면 플래그 해제 및 타이머 시작
        if (other.CompareTag("Player"))
        {
            isPlayerOnTrain = false;
            playerExitTime = Time.time;
        }

        if (attachedObjects.Contains(other.transform))
        {
            other.transform.SetParent(null, true);
            attachedObjects.Remove(other.transform);
        }
    }
}
