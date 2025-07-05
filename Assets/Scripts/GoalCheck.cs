using UnityEngine;
using Fusion;
using System.Collections;

public class GoalCheck : NetworkBehaviour
{
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !Object.HasStateAuthority) return;

        if (other.CompareTag("Avatar"))
        {
            hasTriggered = true;
            int keyCount = other.GetComponent<PlayerAvatar>().GetKeyCount();
            if (keyCount >= 2)
            {
                Debug.Log("ゴールしました！");
                GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
                if (gameManager != null)
                {
                    gameManager.StopTimer();
                }
                else
                {
                    Debug.LogWarning("GameManagerが見つかりません。");
                }
            }
            else
            {
                Debug.Log("鍵が足りません。鍵を2つ集めてください。");
                StartCoroutine(WaitForGoalReset());
            }
        }
    }
    private IEnumerator WaitForGoalReset()
    {
        yield return new WaitForSeconds(3f); // 3秒待機してから再度トリガーを有効にする
        hasTriggered = false;
    }
}
