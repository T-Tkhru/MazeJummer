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

    public override void FixedUpdateNetwork()
    {
        // 回転させる
        transform.Rotate(Vector3.up, 100 * Runner.DeltaTime, Space.World);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (TrapperUIManager.Instance != null)
        {
            Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
            TrapperUIManager.Instance.RemoveKey(gridPos);
        }
    }
}
