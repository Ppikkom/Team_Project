using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[System.Serializable]
public struct VillainSpawnData
{
    public string 이름;
    public float 소환주기;
    public float 개별소환주기;
    public float 오프셋;
}

public class NPCManager : MonoBehaviour
{

    public static NPCManager 인스턴스 { get; set; }

    Dictionary<string, NPCPool> NPC풀 = new Dictionary<string, NPCPool>();
    Dictionary<string, GameObject> NPC프리팹 = new Dictionary<string, GameObject>();
    [SerializeField] private int InitPoolSize = 10;
    [SerializeField] private List<NPCPrefabData> NPC프리팹목록;

    // 소환 관련
    [SerializeField] private VillainSpawnData[] 스폰데이터;


    [System.Serializable]
    public class NPCPrefabData
    {
        public string 이름;
        public GameObject 프리팹;
    }

    private class NPCPool
    {
        public Queue<NPCData> 비활성화 = new Queue<NPCData>();
        public List<NPCData> 활성화 = new List<NPCData>();
    }

    private struct NPCData
    {
        public GameObject gameObject;
        public Transform transform;
    }

    void Awake()
    {
        if (인스턴스 == null)
        {
            인스턴스 = this;
            //DontDestroyOnLoad(gameObject);
            풀초기화();
        }
        else Destroy(gameObject);

        NPC생성();

    }

    void 풀초기화()
    {
        foreach (var 데이터 in NPC프리팹목록)
        {
            if (데이터.프리팹 == null) continue;
            NPC프리팹[데이터.이름] = 데이터.프리팹;
            var pool = new NPCPool();
            NPC풀[데이터.이름] = pool;
            풀확장(데이터.이름, InitPoolSize);
        }
    }

    void 풀확장(string name, int size)
    {
        var 풀 = NPC풀[name];
        int 크기 = 풀.활성화.Count + 풀.비활성화.Count;
        if (크기 >= 50) // 50 -> 최대
        {
            return;
        }
        int 추가 = Mathf.Min(size, 50 - 크기);
        for (int i = 0; i < 추가; i++)
        {
            //Debug.Log(name);
            GameObject npc;
            if (NavMesh.SamplePosition(Vector3.zero, out NavMeshHit hit, 20f, NavMesh.AllAreas))
            {
                npc = Instantiate(NPC프리팹[name], hit.position, Quaternion.identity, transform);

                npc.SetActive(false);
                풀.비활성화.Enqueue(new NPCData
                {
                    gameObject = npc,
                    transform = npc.transform
                });
            }



        }

    }

    public void NPC소환(string name, Vector3 vec)
    {
        if (!NPC풀.TryGetValue(name, out var pool))
        {
            Debug.LogWarning($"해당하는 NPC프리팹이 없습니다. : {name}");
            return;
        }

        if (NPC풀[name].비활성화.Count == 0)
            풀확장(name, 3);

        var 풀 = NPC풀[name];
        var prefab = 풀.비활성화.Dequeue();

        prefab.transform.position = vec;
        //prefab.transform.SetParent(this.transform, worldPositionStays: true);
        prefab.gameObject.SetActive(true);

        var agent = prefab.gameObject.GetComponent<NavMeshAgent>();
        if (agent != null) agent.Warp(vec);

        풀.활성화.Add(prefab);

        return;
    }

    public void NPC반환(GameObject _npc)
    {
        foreach (var 풀 in NPC풀.Values)
        {
            for (int i = 0; i < 풀.활성화.Count; i++)
            {
                if (풀.활성화[i].gameObject == _npc)
                {
                    var npc = 풀.활성화[i];
                    npc.gameObject.SetActive(false);
                    풀.활성화.RemoveAt(i);
                    풀.비활성화.Enqueue(npc);
                    return;
                }
            }
        }
    }

    // 오프셋만큼 기다림 -> ( 소환 -> 반복 )
    void NPC생성()
    {
        Invoke("역1번출구_소환", 스폰데이터[0].오프셋);
        Invoke("역6번출구_소환", 스폰데이터[1].오프셋);
    }

    // 스폰 위치는 임의로 설정
    void 역1번출구_소환() => StartCoroutine(소환(new Vector3(-3, 1, -6), 스폰데이터[0].소환주기, 스폰데이터[0].개별소환주기, 4));
    void 역6번출구_소환() => StartCoroutine(소환(new Vector3(15, -3.5f, -15.85f), 스폰데이터[1].소환주기, 스폰데이터[1].개별소환주기, 4));

    //public void NPC소환(string name, Vector3 vec)

    private IEnumerator 소환(Vector3 pos, float spawnTime, float indivpawnTime, int cnt)
    {
        while (true)
        {
            bool[] vilflag = SetNPCRatio(cnt);
            for (int i = 0; i < cnt; i++)
            {
                if (vilflag[i] == false)
                    NPC소환(NPC프리팹목록[Random.Range(0, 11)].이름, pos); // 일반승객
                else
                {
                    switch (GameManager.인스턴스.현재날짜)
                    {
                        case 1: // 여기 수정함.
                            NPC소환(NPC프리팹목록[Random.Range(33, 37)].이름, pos);
                            break;
                        case 2:
                            //NPC소환(NPC프리팹목록[Random.Range(3, 4)].이름, pos);
                            break;
                    }
                }
                yield return new WaitForSeconds(indivpawnTime);
            }
            yield return new WaitForSeconds(Mathf.Max(0, spawnTime - indivpawnTime * cnt));
        }


    }

    public void 지하철_소환(bool flag = false)
    {
        Vector3 spawnPos;
        bool[] vilflag = SetNPCRatio(8);
        if (flag) // 초기 위치 설정
            spawnPos = new Vector3(6, -19.3f, -77);
        else
            spawnPos = new Vector3(0.75f, -19.3f, -77);

        for (int i = 0; i < 4; i++) // 지하철 칸 갯수
        {
            for (int j = 0; j < 2; j++) // 칸 당 소환인원
            {
                spawnPos.z -= 6f;
                if (flag)
                {
                    if(vilflag[i * 2 + j]) NPC소환(NPC프리팹목록[Random.Range(37, 41)].이름, spawnPos);
                    else NPC소환(NPC프리팹목록[Random.Range(11, 22)].이름, spawnPos);
                }
                else
                {
                    if(vilflag[i * 2 + j]) NPC소환(NPC프리팹목록[Random.Range(41, 45)].이름, spawnPos);
                    else NPC소환(NPC프리팹목록[Random.Range(22, 33)].이름, spawnPos);
                }
                
            }
            spawnPos.z -= 6.5f;
        }
    }

    private bool[] SetNPCRatio(int total)
    {
        List<bool> list = new List<bool>();

        
                int vil = Mathf.RoundToInt(total * GameManager.인스턴스.현재날짜 * 0.1f); // Mathf.RoundToInt(total * (GameManager.인스턴스.현재날짜 - 1) * 0.1f);

                for (int i = 0; i < vil; i++) list.Add(true);
                for (int i = 0; i < total - vil; i++) list.Add(false);

                for (int i = list.Count - 1; i > 0; i--) {
                    int j = Random.Range(0, i + 1);
                    (list[i], list[j]) = (list[j], list[i]);
                }
        //for (int i = 0; i < total; i++) list.Add(false); // 테스트용

            return list.ToArray();
    }

}
