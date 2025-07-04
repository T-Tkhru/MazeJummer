using UnityEngine;
using Fusion;

public class Key : NetworkBehaviour
{
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !Object.HasStateAuthority) return;

        if (other.CompareTag("Avatar"))
        {
            hasTriggered = true;
            GetKey(other);

            GetComponent<Collider>().enabled = false;
        }
    }

    // 鍵を取得したときの処理
    private void GetKey(Collider avatar)
    {
        Debug.Log("鍵を取得しました！");
        var playerAvatar = avatar.GetComponent<PlayerAvatar>();
        if (playerAvatar != null)
        {
            playerAvatar.IncrementKeyCount();
        }
        Destroy(gameObject);
    }
}
