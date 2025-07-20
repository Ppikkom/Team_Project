using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ProblemManager : MonoBehaviour
{
    #region 싱글톤 설정
    public static ProblemManager 인스턴스 { get; private set; } // 싱글톤 선언
    [Header("문제별 최대 생성 수")]
    public List<ProblemSpawnLimit> problemSpawnLimits = new List<ProblemSpawnLimit>();
    private Dictionary<ProblemType, int> spawnLimitLookup;

    // DontDestroyOnLoad가 존재하지 않아 매일마다 새로운 ProblemManager 업그레이드
    void Awake()
    {
        if (인스턴스 == null)
            인스턴스 = this;
        else
            Destroy(gameObject);

        timers = new Dictionary<ProblemType, float>();
        nextSpawnTimes = new Dictionary<ProblemType, float>();
        currentSpawned = new Dictionary<ProblemType, int>();
        spawnLimitLookup = new Dictionary<ProblemType, int>();

        foreach (var entry in problemSpawnLimits)
        {
            if (entry.type != ProblemType.Villain)
                spawnLimitLookup[entry.type] = entry.maxCount;
        }

        foreach (ProblemType type in System.Enum.GetValues(typeof(ProblemType)))
        {
            if (type != ProblemType.Villain)
            {
                timers[type] = 0f;
                nextSpawnTimes[type] = 0f;
                currentSpawned[type] = 0;
            }
        }
    }
    #endregion

    #region 필드 및 변수
    [Header("날짜 및 스폰 주기")]
    public int day = 1;
    public float minRespawnStart = 5f;
    public float minRespawnEnd = 1f;
    public float maxRespawnStart = 15f;
    public float maxRespawnEnd = 5f;

    [Header("카메라 영역")]
    [SerializeField] private Camera playerCamera; // 플레이어의 카메라를 등록하는 영역
    [SerializeField] private float viewMargin = 0.1f; // 카메라의 시야 범위 조절 영역

    [System.Serializable]
    public class ProblemSpawnGroup
    {
        public ProblemType type;
        public List<Transform> spawnPoints;
        public GameObject problemPrefab;
    }

    [Header("문제 타입별 스폰 그룹")]
    public List<BaseProblem> activeProblems = new List<BaseProblem>(); // 활성 문제 :: List | 위치와 프리펩을 등록한 일반 문제 발생 시 등록 리스트
    public List<BaseNPC> specialProblems = new List<BaseNPC>(); // 진상 문제 :: List
    public List<ProblemSpawnGroup> spawnGroups = new List<ProblemSpawnGroup>(); // 활성 영역 :: List | 게임 시작 전 스폰 위치 및 프리펩 저장 리스트
    private Dictionary<ProblemType, Coroutine> respawnCoroutines = new(); // 재생성 시 중복 방지  :: Dictionary

    private Dictionary<ProblemType, float> timers = new(); // 초기 생성 시간 관리
    private Dictionary<ProblemType, float> nextSpawnTimes = new(); // 재생성 시간 관리
    private Dictionary<ProblemType, int> currentSpawned = new();

    // 타이머 코루틴
    private Coroutine normalSpawnTimerCoroutine;

    // 외부에서도 접근 가능한 현재값
    public float minRespawnTime { get; private set; }
    public float maxRespawnTime { get; private set; }
    #endregion

    #region 타입별 최대 개수 설정 영역
    [System.Serializable]
    public class ProblemSpawnLimit
    {
        public ProblemType type;
        public int maxCount;
    }

    // ProblemPoolManager가 호출할 수 있는 상한치 조회 메서드
    public int GetSpawnLimit(ProblemType type)
    {
        return spawnLimitLookup.TryGetValue(type, out int limit) ? limit : 0;
    }

    // 현재 스폰된 개수 조회 메서드
    public int GetCurrentSpawnedCount(ProblemType type)
    {
        return currentSpawned.TryGetValue(type, out int count) ? count : 0;
    }
    #endregion

    #region 일반 문제 스폰 타이머 영역
    // 코루틴 실행용 함수
    public void StartSpawnTimerRoutine(int day)
    {
        if (normalSpawnTimerCoroutine != null)
            StopCoroutine(normalSpawnTimerCoroutine);

        normalSpawnTimerCoroutine = StartCoroutine(noramlSpawnRoutine(day));
    }

    // 실제 보간 계산 수행하는 코루틴
    private IEnumerator noramlSpawnRoutine(int day)
    {
        while (true)
        {
            float t = Mathf.Clamp01((day - 1) / 4f);

            minRespawnTime = Mathf.Lerp(minRespawnStart, minRespawnEnd, t);
            maxRespawnTime = Mathf.Lerp(maxRespawnStart, maxRespawnEnd, t);

            yield return new WaitForSeconds(1f); // 1초마다 갱신
        }
    }
    #endregion

    #region 진상 문제 영역
    private Dictionary<ProblemType, Coroutine> jinsangSpawnTimerCoroutine = new();

    // 진상 문제 스폰 타이머 영역
    public void jinsangSpawnStartTimer(ProblemType type, int count, float interval)
    {
        if (jinsangSpawnTimerCoroutine.ContainsKey(type))
            return;

        Coroutine c = StartCoroutine(jinsangSpawnRoutine(type, count, interval));
        jinsangSpawnTimerCoroutine[type] = c;
    }

    // ProblemManager.인스턴스.jinsangSpawnStartTimer(ProblemType.Human, n, nf); 식으로 외부 사용 가능
    private IEnumerator jinsangSpawnRoutine(ProblemType type, int count, float interval)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnProblem(type);
            yield return new WaitForSeconds(interval);
        }

        jinsangSpawnTimerCoroutine.Remove(type);
    }

    public void StopSpecialSpawnTimer(ProblemType type)
    {
        if (jinsangSpawnTimerCoroutine.ContainsKey(type))
        {
            StopCoroutine(jinsangSpawnTimerCoroutine[type]);
            jinsangSpawnTimerCoroutine.Remove(type);
        }
    }

    // 진상을 problemManager 리스트에 등록하는 영역
    public void RegisterVillainNPC(BaseNPC npc)
    {
        if (npc == null) return;

        // 1. NPC가 Villain 타입으로 명시되었는지 확인
        if (npc is BaseNPC baseNpc && baseNpc.problemType == ProblemType.Villain)
        {
            if (!specialProblems.Contains(baseNpc))
            {
                specialProblems.Add(baseNpc);
            }
        }
    }


    #endregion

    #region 초기화 및 시작
    void Start()
    {
        // GameManager로 날짜 관리 영역
        if (GameManager.인스턴스 != null)
        {
            day = GameManager.인스턴스.현재날짜;
        }

        StartSpawnTimerRoutine(day);

        // 타이머 초기화 영역
        foreach (ProblemType type in System.Enum.GetValues(typeof(ProblemType)))
        {
            if (type == ProblemType.TrashCan || type == ProblemType.Villain) continue;

            timers[type] = 0f;
            nextSpawnTimes[type] = Random.Range(minRespawnTime, maxRespawnTime);
        }

        StartCoroutine(DelayedInitialSpawn(10f));
    }

    // 문제 상황 최초 발생 시 연속 발생 방지 코루틴
    private IEnumerator DelayedInitialSpawn(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(SpawnInitialProblemsCoroutine());
    }
    #endregion

    #region 매 프레임 업데이트
    void Update()
    {
        foreach (var type in new List<ProblemType>(timers.Keys))
        {
            if (type == ProblemType.Villain || type == ProblemType.Puke) continue;
            if (IsProblemTypeActive(type)) continue;

            timers[type] += Time.deltaTime;

            if (timers[type] >= nextSpawnTimes[type])
            {
                SpawnProblem(type);
                timers[type] = 0f;
                nextSpawnTimes[type] = Random.Range(minRespawnTime, maxRespawnTime);
            }
        }

    }
    #endregion

    #region 문제 생성 로직
    // 문제 생성(ProblemType) :: void
    public void SpawnProblem(ProblemType type)
    {
        if (!spawnLimitLookup.ContainsKey(type))
        {
            if (type != ProblemType.TrashCan)
                return;
            return;
        }

        int currentCount = currentSpawned[type];

        if (currentCount >= spawnLimitLookup[type])
        {
            return;
        }

        if (IsProblemTypeActive(type) || respawnCoroutines.ContainsKey(type))
            return;

        ProblemSpawnGroup group = spawnGroups.Find(g => g.type == type);
        if (group == null || group.spawnPoints.Count == 0 || group.problemPrefab == null)
        {
            return;
        }

        Transform spawnPoint = GetRandomSpawnPoint(group);
        if (spawnPoint == null || IsProblemAt(spawnPoint.position))
            return;

        string poolTag = type.ToString();
        GameObject obj = ProblemPoolManager.Instance.SpawnFromPool(poolTag, spawnPoint.position, spawnPoint.rotation);
        if (obj == null)
        {
            return;
        }

        BaseProblem problem = obj.GetComponent<BaseProblem>();
        if (problem != null)
        {
            ProblemConfig config = new ProblemConfig
            {
                type = type,
                spawnPosition = spawnPoint.position
            };
            problem.Initialize(config);
            RegisterProblem(problem);
        }
    }

    // 문제 랜덤 스폰(ProblemSpawnGroup) :: void
    private Transform GetRandomSpawnPoint(ProblemSpawnGroup group)
    {
        if (group.spawnPoints.Count == 0) return null;
        //return group.spawnPoints[Random.Range(0, group.spawnPoints.Count)];
        // 그래피티 & 전단지가 플레이어 카메라에 보이지 않을 때만 생성되도록
        List<Transform> shuffled = new List<Transform>(group.spawnPoints);
        ShuffleList(shuffled);

        Camera cam = playerCamera;
        bool requireOutOfView = group.type == ProblemType.Graffiti || group.type == ProblemType.Leaflet;

        foreach (Transform t in shuffled)
        {
            bool inView = IsInCameraView(cam, t.position);
            Debug.Log($"[스폰 체크] {t.name} - 위치: {t.position}, 시야내부: {inView}");

            if (requireOutOfView && inView)
            {
                continue;
            }

            if (!IsProblemAt(t.position))
            {
                return t;
            }
        }

        return null; // 못 찾으면 null
    }

    // 문제 중복 생성 여부 체크
    private bool IsProblemAt(Vector3 position)
    {
        foreach (var p in activeProblems)
        {
            if (Vector3.Distance(p.transform.position, position) < 0.1f)
                return true;
        }
        return false;
    }
    #endregion

    #region 초기 문제 생성 (여러 개 랜덤 시간차)
    // 문제 생성 시 오브젝트 풀링 관리 코루틴
    private IEnumerator SpawnInitialProblemsCoroutine()
    {
        foreach (ProblemSpawnGroup group in spawnGroups)
        {
            if (group.type == ProblemType.TrashCan || group.type == ProblemType.Villain || group.type == ProblemType.Puke) continue;

            int spawnCount = Random.Range(1, 4); // 각 타입당 생성 개수 랜덤 (1~3)
            List<Transform> shuffled = new List<Transform>(group.spawnPoints);
            ShuffleList(shuffled); // 스폰 위치 무작위 섞기

            if (!currentSpawned.ContainsKey(group.type))
                currentSpawned[group.type] = 0;

            int currentCount = currentSpawned[group.type];

            if (spawnLimitLookup.ContainsKey(group.type) && currentCount >= spawnLimitLookup[group.type])
            {
                continue;
            }


            int spawned = 0;

            foreach (Transform spawnPoint in shuffled)
            {
                if (spawned >= spawnCount) break;
                if (IsProblemAt(spawnPoint.position)) continue;

                if ((group.type == ProblemType.Graffiti || group.type == ProblemType.Leaflet) &&
                IsInCameraView(playerCamera, spawnPoint.position))
                    continue;

                string poolTag = group.type.ToString();
                GameObject obj = ProblemPoolManager.Instance.SpawnFromPool(poolTag, spawnPoint.position, spawnPoint.rotation);

                if (obj != null)
                {
                    BaseProblem problem = obj.GetComponent<BaseProblem>();
                    if (problem != null)
                    {
                        ProblemConfig config = new ProblemConfig
                        {
                            type = group.type,
                            spawnPosition = spawnPoint.position
                        };
                        problem.Initialize(config);
                        RegisterProblem(problem);
                    }
                    spawned++;
                }

                yield return new WaitForSeconds(Random.Range(minRespawnTime, maxRespawnTime)); // 시간 부분을 랜덤 설정
            }
        }
    }

    // 동시 생성 시 리스트 섞기 함수
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }
    #endregion

    #region 문제 등록 및 제거
    // 일반 문제 생성 시, 리스트에 추가
    public void RegisterProblem(BaseProblem problem)
    {
        if (!activeProblems.Contains(problem))
        {
            activeProblems.Add(problem);

            if (!currentSpawned.ContainsKey(problem.problemType))
                currentSpawned[problem.problemType] = 0;

            currentSpawned[problem.problemType]++;

            if (GameplayUIController.인스턴스 != null)
            {
                if (problem.problemType == ProblemType.Villain)
                    GameplayUIController.인스턴스.jinsangCount();
                else
                    GameplayUIController.인스턴스.normalCount();
            }
        }
    }



    // 일반 문제 제거 시, 리스트에서 제거
    public void UnregisterProblem(BaseProblem problem)
    {
        if (activeProblems.Contains(problem))
        {
            if (problem == null) return;
            if (!activeProblems.Contains(problem)) return;

            activeProblems.Remove(problem);

            if (currentSpawned.ContainsKey(problem.problemType))
                currentSpawned[problem.problemType] = Mathf.Max(0, currentSpawned[problem.problemType] - 1);

            if (!problem.isSolved && GameplayUIController.인스턴스 != null)
            {
                if (problem.problemType == ProblemType.Villain)
                    GameplayUIController.인스턴스.jinsangDisCount();
                else
                    GameplayUIController.인스턴스.normalDisCount();
            }
            problem.isSolved = true;
        }
    }

    // 진상 문제 생성 시, 리스트에 추가
    public void RegisterJinsangProblem(BaseNPC npc)
    {
        if (!specialProblems.Contains(npc))
        {
            specialProblems.Add(npc);
        }
    }

    // 진상 문제 제거 시, 리스트에서 제거
    public void UnregisterJinsangProblem(BaseNPC npc)
    {
        if (specialProblems.Contains(npc))
        {
            specialProblems.Remove(npc);
        }
    }


    // 문제 별 리스트 등록 관리 영역
    public void NotifyProblemSolved(BaseProblem problem)
    {
        if (problem == null || problem.isSolved == false) return;

        UnregisterProblem(problem);

        if (problem != null && problem.gameObject != null)
        {
            string tag = problem.problemType.ToString();
            ProblemPoolManager.Instance.ReturnToPool(tag, problem.gameObject);
        }

        if (!respawnCoroutines.ContainsKey(problem.problemType))
        {
            Coroutine c = StartCoroutine(RespawnProblemAfterDelay(problem.problemType));
            respawnCoroutines.Add(problem.problemType, c);
        }
        ProblemPoolManager.Instance.ReturnToPool(problem.problemType.ToString(), problem.gameObject);
    }

    // 문제 제거(BaseProblem) :: void
    void RemoveProblem(BaseProblem problem)
    {
        if (activeProblems.Contains(problem))
        {
            activeProblems.Remove(problem);
            //Destroy(problem.gameObject); // 실제 파괴 위치
        }
        string tag = problem.problemType.ToString();
        ProblemPoolManager.Instance.ReturnToPool(tag, problem.gameObject);
    }

    public bool IsProblemTypeActive(ProblemType type)
    {
        return activeProblems.Exists(p => p != null && p.problemType == type && p.gameObject.activeInHierarchy);
    }

    #endregion

    #region 재생성 로직
    // 재생성 코루틴
    IEnumerator RespawnProblemAfterDelay(ProblemType type)
    {
        float delay = Random.Range(minRespawnTime, maxRespawnTime);

        yield return new WaitForSeconds(delay);

        StartCoroutine(SpawnMultipleProblemsDelay(type));

        respawnCoroutines.Remove(type);
    }

    // 재생성 시에도 2개 이상이 생성될 수 있도록 설정된 함수 영역
    private void SpawnMultipleProblems(ProblemType type)
    {
        StartCoroutine(SpawnMultipleProblemsDelay(type));
    }

    // 재생성 시 2개 이상이면 지연 생성 구현 코루틴
    IEnumerator SpawnMultipleProblemsDelay(ProblemType type)
    {
        ProblemSpawnGroup group = spawnGroups.Find(g => g.type == type);
        if (group == null || group.problemPrefab == null || group.spawnPoints.Count == 0)
            yield break;
        if (type == ProblemType.Puke || type == ProblemType.Villain) 
            yield break;

        int spawnCount = Random.Range(1, 4);
        List<Transform> shuffled = new List<Transform>(group.spawnPoints);
        ShuffleList(shuffled);

        for (int i = 0; i < spawnCount && i < shuffled.Count; i++)
        {
            Transform spawnPoint = shuffled[i];

            if (IsProblemAt(spawnPoint.position)) continue;

            // 시야 판정에 의한 스폰 부분 추가
            if ((group.type == ProblemType.Graffiti || group.type == ProblemType.Leaflet) &&
                IsInCameraView(playerCamera, spawnPoint.position))
                continue;

            string poolTag = type.ToString();
            GameObject obj = ProblemPoolManager.Instance.SpawnFromPool(poolTag, spawnPoint.position, spawnPoint.rotation);

            if (obj != null)
            {
                BaseProblem problem = obj.GetComponent<BaseProblem>();
                if (problem != null)
                {
                    ProblemConfig config = new ProblemConfig
                    {
                        type = type,
                        spawnPosition = spawnPoint.position
                    };
                    problem.Initialize(config);
                    RegisterProblem(problem);
                }
            }

            yield return new WaitForSeconds(Random.Range(0.3f, 0.7f)); // 각 생성 간 시간차
        }
    }

    #endregion

    #region 특수 생성 호출
    // 최초 생성 코루틴 동작 함수
    public void StartSpawningInitialProblems()
    {
        StartCoroutine(SpawnInitialProblemsCoroutine());
    }

    public void SpawnProblemAt(ProblemType type, Transform spawnPoint)
    {
        ProblemSpawnGroup group = spawnGroups.Find(g => g.type == type);
        if (group == null || group.problemPrefab == null)
        {
            return;
        }

        if (IsProblemAt(spawnPoint.position)) return;

        GameObject obj = Instantiate(group.problemPrefab, spawnPoint.position, spawnPoint.rotation);
        BaseProblem problem = obj.GetComponent<BaseProblem>();

        if (problem != null)
        {
            ProblemConfig config = new ProblemConfig
            {
                type = type,
                spawnPosition = spawnPoint.position
            };

            problem.Initialize(config);
            activeProblems.Add(problem);
        }
    }
    #endregion

    #region 카메라 시야 판정 로직
    

    // 시야 판정 부분 추가
    private bool IsInCameraView(Camera cam, Vector3 worldPos)
    {
        if (cam == null) return false;

        Vector3 viewPos = cam.WorldToViewportPoint(worldPos);
        if (viewPos.z < 0) return false;

        return viewPos.x >= -viewMargin && viewPos.x <= 1 + viewMargin &&
               viewPos.y >= -viewMargin && viewPos.y <= 1 + viewMargin;
    }



    #endregion

    // 진상 문제 타입 판별
    private bool IsUIType(ProblemType type)
    {
        return type == ProblemType.Villain;
    }
}