using UnityEngine;
using UnityEngine.Events;

public class ReputationManager : MonoBehaviour
{
    public static ReputationManager 인스턴스 { get; private set; }
    public int 현재점수 = 100;
    public UnityEvent<int> E평판변경 = new UnityEvent<int>();

    void Awake()
    {
        if (인스턴스 == null) 인스턴스 = this;
        else Destroy(gameObject);
    }

    public void 초기화()
    {
        현재점수 = 100;
        평판변경();
    }

    public void 점수추가(int 값)
    {
        현재점수 += 값;
        평판변경();
    }

    public void 점수감소(int 값)
    {
        현재점수 -= 값;
        평판변경();

        if (현재점수 <= 0)
        {
            if (GameManager.인스턴스.도전모드인가)
            {
                GameManager.인스턴스.도전모드종료();
            }
            else
            {
                GameManager.인스턴스.게임오버();
            }
        }
    }

    public void 문제방치()
    {
        점수감소(1);
    }

    void 평판변경()
    {
        E평판변경.Invoke(현재점수);
    }

    void Update()
    {
        // 테스트용: 키보드의 G 키를 누르면 점수 10 감소
        if (Input.GetKeyDown(KeyCode.G))
        {
            점수감소(10);
            Debug.Log("G 키 누름: 점수 10 감소");
        }
    }
}