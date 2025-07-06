using UnityEngine;
using Fusion;
using System.Collections;

public class BlindTrap : Trap
{
    [SerializeField]
    private float trapDuration = 10f; // トラップの効果時間
    protected override void TriggerEffect(Collider avatar)
    {
        Debug.Log("トラップに引っかかった！");
        var controller = avatar.GetComponent<NetworkCharacterController>();
        if (controller != null)
        {
            RunnerUIManager.Instance.ActivateBlind(trapDuration);
            RPC_TriggerBlindEffect(trapDuration);
            StartCoroutine(DelayedDespawn());
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_TriggerBlindEffect(float duration)
    {
        Debug.Log($"ブラインド効果を適用します。Duration: {duration}");
        if (TrapperUIManager.Instance != null)
        {
            Debug.Log($"TrapperUIManagerが存在します。ブラインド効果を適用します。Duration: {duration}");
            TrapperUIManager.Instance.ActivateBlind(duration);
        }
    }

    private IEnumerator DelayedDespawn()
    {
        yield return new WaitForSeconds(0.5f); // 0.5秒待機してからトラップを消す（RPCを確実に送るため）
        if (Object != null && Object.IsValid)
        {
            Runner.Despawn(Object);
        }
    }
}
