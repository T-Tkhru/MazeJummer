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
}