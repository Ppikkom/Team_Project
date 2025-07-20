using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct 장소
{
    public 장소_이름 name;
    public Collider collider;
}

public enum 장소_이름
{
    지상,
    지하1층,
    지하2층,
    지하3층,
    지하4층,
    지하5층,
    기타,
    상호작용 = 9 // 특정 행동을 하는 곳(자판기 등)
}

public class MapData : MonoBehaviour
{
    [System.Serializable]
    public struct roomData
    {
        public string 이름;
        public 장소_이름 분류;
        public Collider collider;
    }

    public roomData[] mapData;

    public Collider GetColliderByName(string keyName)
    {

        foreach (var entry in mapData)
        {
            if (entry.이름 == keyName)
                return entry.collider;
        }

        Debug.LogWarning($"RoomData: 이름 '{keyName}'에 해당하는 entry가 없습니다.");
        return null;
    }

    public string GetNameByCollider(Collider col)
    {
        foreach (var entry in mapData)
        {
            if (entry.collider.GetInstanceID() == col.GetInstanceID())
                return entry.이름;
        }

        Debug.LogWarning($"RoomData: 콜라이더 '{col}'에 해당하는 entry가 없습니다.");
        return null;
    }

    public bool 분류_확인(string name, 장소_이름 p) // 자판기 상호작용
    {
        foreach (var v in mapData)
        {
            if (v.이름 == name && v.분류 == p)
                return true;
        }
        return false;
    }

    public Vector3 랜덤_지역좌표()
    {
        int index;
        while (true)
        {
            index = UnityEngine.Random.Range(0, mapData.Length);
            if (mapData[index].분류 != 장소_이름.상호작용 || mapData[index].분류 != 장소_이름.지상) break;
        }

        Collider gateC = mapData[index].collider;
        Vector3 min = gateC.bounds.center - gateC.bounds.size / 2;
        Vector3 max = gateC.bounds.center + gateC.bounds.size / 2;
        Vector3 pos = new Vector3(min.x + (float)new System.Random().NextDouble() * (max.x - min.x), min.y, min.z + (float)new System.Random().NextDouble() * (max.z - min.z));

        return pos;

    }

    public bool HasCollider(Collider col)
    {
        return GetNameByCollider(col) != null;
    }
}





/*

    [SerializeField] ECDictionary ec;
    public Dictionary<장소_이름, Collider> CollidemapDataap => ec.Dictionary;


    public Collider GetCollider(장소_이름 key)
    {
        return ec.Dictionary.TryGetValue(key, out var col)
            ? col
            : null;
    }
[Serializable]
public class ECDictionary : ISerializationCallbackReceiver
{
    [SerializeField] List<장소_이름> keys = new List<장소_이름>();
    [SerializeField] List<Collider> values = new List<Collider>();

    Dictionary<장소_이름, Collider> dict = new Dictionary<장소_이름, Collider>();
    public Dictionary<장소_이름, Collider> Dictionary => dict;

    public void OnBeforeSerialize()
    {
        if (!Application.isPlaying)
            return;

        keys.Clear();
        values.Clear();
        foreach (var kvp in dict)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        dict = new Dictionary<장소_이름, Collider>();
        int cnt = Mathf.Min(keys.Count, values.Count);
        for (int i = 0; i < cnt; i++)
            dict[keys[i]] = values[i];
    }
}

*/