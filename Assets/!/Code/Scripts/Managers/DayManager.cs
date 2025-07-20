using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class DayManager : MonoBehaviour
{
    public static DayManager 인스턴스 { get; private set; }
    public int 현재날짜 = 1;
    public int 최대날짜 = 5;
    public float 게임시간; // 0~600초 (10분=1일)
    public TextMeshProUGUI 시간텍스트;
    public UnityEvent<int> 날짜변경이벤트 = new UnityEvent<int>();
    public UnityEvent<int> 시간변경이벤트 = new UnityEvent<int>();

    void Awake()
    {
        if (인스턴스 == null) 인스턴스 = this;
        else Destroy(gameObject);
        if (시간텍스트 == null) Debug.LogWarning("시간텍스트가 연결되지 않음");
    }

    public void 시작()
    {
        if (시간텍스트 == null)
        {
            Debug.LogError("시간텍스트가 없음");
            return;
        }

        현재날짜 = GameManager.인스턴스.현재날짜;

        게임시간 = 0f;
        시간업데이트();
        날짜변경이벤트.Invoke(현재날짜);
    }

    void Update()
    {
        if (GameManager.인스턴스.도전모드인가)
        {
            게임시간 += Time.deltaTime;

            시간업데이트();
            return;
        }

        if (현재날짜 <= 최대날짜)
        {
            게임시간 += Time.deltaTime;
            if (게임시간 >= 600f)
            {
                GameManager.인스턴스.결과화면으로();
                enabled = false;
            }
            시간업데이트();
        }
    }

    void 시간업데이트()
    {
        float 시간단위 = 게임시간 / 600f * 600f;
        int 총분 = 9 * 60 + (int)시간단위;
        int 시 = 총분 / 60;
        int 분 = 총분 % 60;
        int 시간숫자 = 시 * 100 + (분 / 30 * 30); // 예: 9:30 -> 930, 15:00 -> 1500

        시간변경이벤트.Invoke(시간숫자);
    }

}