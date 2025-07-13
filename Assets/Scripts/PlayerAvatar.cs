using System.Collections;
using Fusion;
using Unity.Cinemachine;
using UnityEngine;


public class PlayerAvatar : NetworkBehaviour
{
    private NetworkCharacterController characterController;
    private float defaultSpeed;
    [Networked] private int keyCount { get; set; } = 0;
    private GameManager gameManager;
    [SerializeField] private GameObject freeLookCamera;
    private bool isReverseInput = false;

    private Animator animator;
    [Networked] private float speed { get; set; }
    [Networked] private TickTimer BlindTimer { get; set; } // 速度ダウンのタイマー
    [Networked] private TickTimer SpeedDownTimer { get; set; } // 速度ダウンのタイマー
    [Networked] private TickTimer ReverseInputTimer { get; set; } // 入力反転のタイマー


    public override void Spawned()
    {
        // ネットワークキャラクターコントローラーを取得
        characterController = GetComponent<NetworkCharacterController>();


        // 自分自身のアバターにカメラを追従させる
        if (Object.HasInputAuthority)
        {
            Debug.Log("自分のアバターが生成されました。カメラを設定します。");
            freeLookCamera.GetComponent<CinemachineCamera>().Priority.Value = 100;
        }
        else
        {
            Debug.Log("他のプレイヤーのアバターが生成されました。カメラは設定しません。");
            Debug.Log($"プレイヤーの位置: {transform.position}");
        }
        defaultSpeed = characterController.maxSpeed;
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("Animatorが見つかりません。アニメーションが正しく動作しない可能性があります。");
        }

    }

    public override void Render()
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", speed);
        }
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
        TrapEffectUpdate();
        if (GetInput(out NetworkInputData data))
        {
            // 入力方向のベクトルを正規化する
            data.Direction.Normalize();
            Vector3 move = data.Direction;
            characterController.Move(isReverseInput ? -move : move);
            if (animator != null)
            {
                speed = move.magnitude; // 0〜1
                animator.SetFloat("Speed", speed);
            }
            if (data.Buttons.IsSet(NetworkInputButtons.Jump))
            {
                characterController.Jump();
            }
        }
    }

    private void TrapEffectUpdate()
    {
        if (SpeedDownTimer.Expired(Runner))
        {
            SpeedDownTimer = TickTimer.None;
            characterController.maxSpeed = defaultSpeed; // 元の速度に戻す
        }
        if (ReverseInputTimer.Expired(Runner))
        {
            ReverseInputTimer = TickTimer.None;
            isReverseInput = false; // 入力の反転を解除
        }
        if (BlindTimer.Expired(Runner))
        {
            BlindTimer = TickTimer.None;
        }
    }

    public void ActivateSpeedDown(float duration)
    {
        if (SpeedDownTimer.IsRunning)
        {

            duration += SpeedDownTimer.RemainingTime(Runner) ?? 0f; // 既存のタイマーがある場合、残り時間を加算
            SpeedDownTimer = TickTimer.CreateFromSeconds(Runner, duration);
        }
        else
        {
            SpeedDownTimer = TickTimer.CreateFromSeconds(Runner, duration);
            characterController.maxSpeed = defaultSpeed / 2; // 速度を半分にする
        }
    }
    public void ActivateReverseInput(float duration)
    {
        if (ReverseInputTimer.IsRunning)
        {
            duration += ReverseInputTimer.RemainingTime(Runner) ?? 0f; // 既存のタイマーがある場合、残り時間を加算
            ReverseInputTimer = TickTimer.CreateFromSeconds(Runner, duration);
        }
        else
        {
            ReverseInputTimer = TickTimer.CreateFromSeconds(Runner, duration);
            isReverseInput = true; // 入力の反転を有効化
        }
    }
    public void ActivateBlind(float duration)
    {
        if (BlindTimer.IsRunning)
        {
            duration += BlindTimer.RemainingTime(Runner) ?? 0f; // 既存のタイマーがある場合、残り時間を加算
            BlindTimer = TickTimer.CreateFromSeconds(Runner, duration);
        }
        else
        {
            BlindTimer = TickTimer.CreateFromSeconds(Runner, duration);
        }
    }

    public void IncrementKeyCount()
    {
        keyCount++;
        Debug.Log($"鍵を取得しました！現在の鍵の数: {keyCount}");
        RunnerUIManager.Instance?.UpdateKeyDisplay(keyCount);
    }

    public int GetKeyCount()
    {
        return keyCount;
    }

    public void ResetSpeed()
    {
        speed = 0; // アニメーションの速度をリセット
    }

    public float GetBlindTime()
    {
        return BlindTimer.RemainingTime(Runner) ?? 0f;
    }
    public float GetSpeedDownTime()
    {
        return SpeedDownTimer.RemainingTime(Runner) ?? 0f;
    }

    public float GetReverseInputTime()
    {
        return ReverseInputTimer.RemainingTime(Runner) ?? 0f;
    }

}
