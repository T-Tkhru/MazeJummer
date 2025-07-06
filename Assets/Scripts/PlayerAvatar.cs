using System.Collections;
using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Networking;


public class PlayerAvatar : NetworkBehaviour
{
    // プレイヤー名のネットワークプロパティを定義する
    [Networked]
    private NetworkString<_16> NickName { get; set; }
    private ChangeDetector _changeDetector;
    private NetworkCharacterController characterController;
    private PlayerAvatarView view;
    private float defaultSpeed;
    private int speedDownRefCount = 0;
    private float slowSpeed = 1.25f;
    [Networked]
    private int keyCount { get; set; } = 0;
    private GameManager gameManager;
    [SerializeField]
    private GameObject freeLookCamera;
    private bool isReverseInput = false;
    private int reverseInputRefCount = 0;

    public override void Spawned()
    {
        // ネットワークキャラクターコントローラーを取得
        characterController = GetComponent<NetworkCharacterController>();
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

        // NickName がすでにセットされている可能性があるので、即反映
        view = GetComponent<PlayerAvatarView>();

        if (!string.IsNullOrEmpty(NickName.Value))
        {
            view.SetNickName(NickName.Value);
        }

        // 自分自身のアバターにカメラを追従させる
        if (Object.HasInputAuthority)
        {
            Debug.Log("自分のアバターが生成されました。カメラを設定します。");
            // RPCでプレイヤー名を設定する処理をホストに実行してもらう
            Rpc_SetNickName(PlayerData.NickName);
            view.SetCameraTarget();
        }
        else
        {
            Debug.Log("他のプレイヤーのアバターが生成されました。カメラは設定しません。");
        }
        defaultSpeed = characterController.maxSpeed;
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public override void Render()
    {
        foreach (var change in _changeDetector.DetectChanges(this, out var previous, out var current))
        {
            switch (change)
            {
                case nameof(NickName):
                    var reader = GetPropertyReader<NetworkString<_16>>(nameof(NickName));
                    var (_, newName) = reader.Read(previous, current);
                    OnNickNameChanged(newName);
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
    private void OnNickNameChanged(NetworkString<_16> name)
    {
        view.SetNickName(name.Value);
    }


    public override void FixedUpdateNetwork()
    {
        if (!gameManager.IsGameStarted() || gameManager.IsGameFinished())
        {
            // 操作できないように
            characterController.Move(Vector3.zero);
            freeLookCamera.GetComponent<CinemachineInputAxisController>().enabled = false;
            return;
        }
        freeLookCamera.GetComponent<CinemachineInputAxisController>().enabled = true;
        if (GetInput(out NetworkInputData data))
        {
            // 入力方向のベクトルを正規化する
            data.Direction.Normalize();
            // 入力方向を移動方向としてそのまま渡す
            if (isReverseInput)
            {
                // 入力を反転させる
                characterController.Move(-data.Direction);
            }
            else
            {
                // 通常の入力方向で移動
                characterController.Move(data.Direction);
            }
            if (data.Buttons.IsSet(NetworkInputButtons.Jump))
            {
                characterController.Jump();
            }
        }
    }

    public void ActivateSpeedDown(float duration)
    {
        speedDownRefCount++;
        StartCoroutine(HandleSpeedDownEffect(duration));
    }

    private IEnumerator HandleSpeedDownEffect(float duration)
    {
        characterController.maxSpeed = slowSpeed;
        yield return new WaitForSeconds(duration);
        speedDownRefCount--;
        if (speedDownRefCount <= 0)
        {
            speedDownRefCount = 0;
            characterController.maxSpeed = defaultSpeed;
        }
    }
    public void ActivateReverseInput(float duration)
    {
        reverseInputRefCount++;
        StartCoroutine(HandleReverseInputEffect(duration));
    }
    private IEnumerator HandleReverseInputEffect(float duration)
    {
        // 入力を反転させるための処理を実装
        isReverseInput = true;
        yield return new WaitForSeconds(duration);
        reverseInputRefCount--;
        if (reverseInputRefCount <= 0)
        {
            reverseInputRefCount = 0;
            isReverseInput = false; // 入力の反転を解除
        }
    }

    public void IncrementKeyCount()
    {
        keyCount++;
        Debug.Log($"鍵を取得しました！現在の鍵の数: {keyCount}");
        // ここでUIなどに鍵の数を反映する処理を追加できます
    }

    public int GetKeyCount()
    {
        return keyCount;
    }

}
