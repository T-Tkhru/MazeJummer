using UnityEngine;
using Fusion;

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
                // ゴールに到達したときの処理をここに追加
                // 例えば、ゲームクリアのUIを表示するなど
            }
            else
            {
                Debug.Log("鍵が足りません。鍵を2つ集めてください。");
            }
        }
    }
}
