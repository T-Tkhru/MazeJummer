using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;

public class KCCAvater : NetworkBehaviour
{
    private SimpleKCC kcc;

    public override void Spawned()
    {
        kcc = GetComponent<SimpleKCC>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasInputAuthority)
            return;

        // 入力処理（例: WASD）
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        Vector3 move = new Vector3(input.x, 0, input.y);
        kcc.Move(move); // ← 移動はこの関数でやる
    }
}
