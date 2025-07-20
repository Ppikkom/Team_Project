using System.Collections.Generic;
using UnityEngine;

// 문제 프리팹을 오브젝트 풀링으로 관리하는 PoolManager 추가
public class ProblemPoolManager : MonoBehaviour
{
    // 문제 오브젝트를 풀링으로 관리하는 매니저 (싱글톤)
    public static ProblemPoolManager Instance { get; private set; }

    // 인스펙터 상에서 값을 입력 가능
    [System.Serializable]
    public class Pool
    {
        public string tag; // 오브젝트 풀 식별 태그
        public GameObject prefab; // 등록할 프리팹
        public int size; // 프리팹 생성 개수
    }

    public List<Pool> poolList; // 인스펙터에서 Pool을 리스트로 지정 가능
    private Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<string, Pool> poolConfigLookup;
    private ProblemManager problemManager; // ProblemManager 참조

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializePools();
    }

    // 풀 멤버 초기화 영역
    private void InitializePools()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        poolConfigLookup = new Dictionary<string, Pool>();
        problemManager = FindAnyObjectByType<ProblemManager>();

        if (problemManager == null)
        {
            return;
        }

        foreach (var pool in poolList)
        {

            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, transform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolDictionary.Add(pool.tag, objectPool);
            poolConfigLookup.Add(pool.tag, pool);
        }
    }

    // 실행 시, 오브젝트 풀에서부터 프리팹 꺼내기 영역
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            return null;
        }

        // 현재 스폰된 개수 확인
        ProblemType problemType = (ProblemType)System.Enum.Parse(typeof(ProblemType), tag, true);
        int currentSpawned = problemManager.GetCurrentSpawnedCount(problemType);
        int maxSpawnLimit = problemManager.GetSpawnLimit(problemType); // 최대 스폰 개수 조정

        if (currentSpawned >= maxSpawnLimit)
        {
            return null;
        }

        Queue<GameObject> pool = poolDictionary[tag];
        GameObject obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
            if (obj == null)
            {
                obj = CreateNewObject(tag);
            }
        }
        else
        {
            obj = CreateNewObject(tag);
        }

        if (obj != null)
        {
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
        }

        return obj;
    }

    // 태그 별 오브젝트 풀 생성 영역
    private GameObject CreateNewObject(string tag)
    {
        if (!poolConfigLookup.TryGetValue(tag, out var config))
            return null;

        GameObject obj = Instantiate(config.prefab, transform);
        return obj;
    }

    // 제거되면, 다시 오브젝트 풀로 반환받는 영역
    public void ReturnToPool(string tag, GameObject obj)
    {

        // 파괴된 오브젝트를 무시해서 에러가 발생하지 않도록
        if (obj == null)
        {
            return;
        }

        if (!poolDictionary.ContainsKey(tag))
        {
            Destroy(obj);
            return;
        }

        // BaseProblem 상태 초기화
        var problem = obj.GetComponent<BaseProblem>();
        if (problem != null)
        {
            ProblemManager.인스턴스.UnregisterProblem(problem);
            problem.Initialize(new ProblemConfig()); // 기본 초기화
        }

        obj.SetActive(false);
        poolDictionary[tag].Enqueue(obj);
    }

    // 오브젝트 풀이 생성될 때마다 카운트 영역
    public int GetPoolCount(string tag)
    {
        return poolDictionary.ContainsKey(tag) ? poolDictionary[tag].Count : 0;
    }
}
