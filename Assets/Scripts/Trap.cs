using UnityEngine;
using Fusion;

public abstract class Trap : NetworkBehaviour
{
    private bool hasTriggered = false;

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !Object.HasStateAuthority) return;

        if (other.CompareTag("Avatar"))
        {
            hasTriggered = true;
            TriggerEffect(other); // 継承先で効果を定義

            // 判定を無効化（もう一度当たっても何もしない）
            GetComponent<Collider>().enabled = false;
        }
    }

    // 継承先で具体的な効果を実装
    protected abstract void TriggerEffect(Collider avatar);

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (TrapperUIManager.Instance != null)
        {
            Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
            TrapperUIManager.Instance.RemoveTrap(gridPos);
        }
    }
}
