using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Boot : MonoBehaviour
{
    private void Start()
    {
        // 에셋 로드
        StartCoroutine(LoadAssets());
    }

    private IEnumerator LoadAssets()
    {
        // 에셋 로딩 작업 수행
        // 예: Resources.LoadAsync, AssetBundle 로드 등
        
        // 임시 대기 시간 (실제 로딩 작업으로 대체 필요)
        yield return new WaitForSeconds(1f);

        // 로딩이 완료되면 메인 씬으로 전환
        SceneManager.LoadScene("MainMenu");
    }
}