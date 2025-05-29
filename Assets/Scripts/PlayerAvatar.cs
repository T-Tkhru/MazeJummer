using Fusion;
using UnityEngine;
using UnityEngine.Networking;


public class PlayerAvatar : NetworkBehaviour
{
    // プレイヤー名のネットワークプロパティを定義する
    [Networked]
    private NetworkString<_16> NickName { get; set; }
    private ChangeDetector _changeDetector;
    private NetworkCharacterController characterController;
    [SerializeField]
    private PlayerAvatarView view;

    public override void Spawned()
    {
        // ネットワークキャラクターコントローラーを取得
        characterController = GetComponent<NetworkCharacterController>();
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

        // NickName がすでにセットされている可能性があるので、即反映
        if (!string.IsNullOrEmpty(NickName.Value))
        {
            view.SetNickName(NickName.Value);
        }

        // 自分自身のアバターにカメラを追従させる
        if (Object.HasInputAuthority)
        {
            Debug.Log("自分のアバターが生成されました。カメラを設定します。");
            view.SetCameraTarget();
            // RPCでプレイヤー名を設定する処理をホストに実行してもらう
            Rpc_SetNickName(PlayerData.NickName);
        }
    }

    public override void Render()
    {
        foreach (var change in _changeDetector.DetectChanges(this, out var previous, out var current))
        {
            switch (change)
            {
                case nameof(NickName):
                    var reader = GetPropertyReader<NetworkString<_16>>(nameof(NickName));
                    var (oldName, newName) = reader.Read(previous, current);
                    OnNickNameChanged(oldName, newName);
                    break;
            }
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void Rpc_SetNickName(string nickName)
    {
        NickName = nickName;
    }

    // ネットワークプロパティ（NickName）が更新された時に呼ばれるコールバック
    private void OnNickNameChanged(NetworkString<_16> oldValue, NetworkString<_16> newValue)
    {
        view.SetNickName(newValue.Value);
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
