using Fusion;
using UnityEngine;

public class Wall : NetworkBehaviour
{
    private bool isOuterWall = false; // 外壁かどうかのフラグ
    
    public bool DespawnWall()
    {
        // ネットワーク上で壁を削除する
        if (Object.HasStateAuthority && !isOuterWall)
        {
            Runner.Despawn(Object);
            return true;
        }
        Debug.Log("外壁は削除できません。");
        return false;
    }

    public void SetOuterWall(bool isOuter)
    {
        isOuterWall = isOuter;
    }

    // サーバーで Despawn されたときに全クライアントで呼ばれる
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);
        
        // 壁の座標からグリッド位置を計算
        Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        
        Debug.Log($"Wall が Despawn されました。グリッド位置: {gridPos}");
        
        // TrapperUIManager に通知して、UIタイルを road に変更
        if (TrapperUIManager.Instance != null)
        {
            TrapperUIManager.Instance.OnWallDespawned(gridPos);
        }
    }
}