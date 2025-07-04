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
            GetKey();

            GetComponent<Collider>().enabled = false;
        }
    }

    // 鍵を取得したときの処理
    private void GetKey()
    {
        Debug.Log("鍵を取得しました！");
        Destroy(gameObject);
    }
}
