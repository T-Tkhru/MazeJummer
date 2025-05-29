using Fusion;
using UnityEngine;
using UnityEngine.Networking;


public class PlayerAvater : NetworkBehaviour
{
    private NetworkCharacterController characterController;
    [SerializeField]
    private PlayerAvatarView view;

    public override void Spawned()
    {
        // ネットワークキャラクターコントローラーを取得
        characterController = GetComponent<NetworkCharacterController>();
        // 自分自身のアバターにカメラを追従させる
        if (Object.HasInputAuthority)
        {
            Debug.Log("自分のアバターが生成されました。カメラを設定します。");
            view.SetCameraTarget();
        }



    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            // 入力方向のベクトルを正規化する
            data.Direction.Normalize();
            // 入力方向を移動方向としてそのまま渡す
            characterController.Move(data.Direction);
            if (data.Buttons.IsSet(NetworkInputButtons.Jump))
            {
                characterController.Jump();
            }
        }
    }

}
