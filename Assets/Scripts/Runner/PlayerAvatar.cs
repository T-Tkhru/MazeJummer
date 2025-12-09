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
    [SerializeField] private float breakRange = 1.0f;
    private bool isReverseInput = false;

    private Animator animator;
    [Networked] private float speed { get; set; }
    [Networked] private TickTimer BlindTimer { get; set; } // 速度ダウンのタイマー
    [Networked] private TickTimer SpeedDownTimer { get; set; } // 速度ダウンのタイマー
    [Networked] private TickTimer ReverseInputTimer { get; set; } // 入力反転のタイマー
    [Networked] private NetworkBool prevBreakBlockInput { get; set; } = false; // 前フレームのEボタン入力状態
    [Networked] private int remainingWallBreaks { get; set; } = 0; // 残りの壁破壊回数


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
            // Eボタンが「押された瞬間」だけ検知する（前フレームでは押されていなかったが、今フレームで押されている）
            bool currentBreakBlockInput = data.Buttons.IsSet(NetworkInputButtons.BreakBlock);
            if (currentBreakBlockInput && !prevBreakBlockInput)
            {
                BreakBlock(range: breakRange);
            }
            prevBreakBlockInput = currentBreakBlockInput; // 次フレームのために入力状態を保存
#if UNITY_EDITOR
            if (data.Buttons.IsSet(NetworkInputButtons.Jump))
            {
                characterController.Jump();
            }
#endif
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

    public void BreakBlock(float range = 2.0f)
    {
        if (remainingWallBreaks <= 0)
        {
            Debug.Log("壁を破壊する残り回数がありません。");
            return;
        }
        // 👇 プレイヤーの位置と向きを基準に Ray を発射
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;
        Debug.Log($"BreakBlock: origin={origin}, direction={direction}, range={range}");

        Ray ray = new Ray(origin, direction);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range))
        {
            GameObject hitObj = hit.collider.gameObject;

            if (hitObj.CompareTag("Wall"))
            {
                Wall wall = hitObj.GetComponent<Wall>();
                if (wall != null)
                {
                    bool result = wall.DespawnWall();
                    if (result)
                    {
                        Debug.Log($"Wallブロックを破壊しました: {hitObj.name}");
                        remainingWallBreaks--;
                        RunnerUIManager.Instance?.UpdateHammerDisplay(remainingWallBreaks);
                    }
                    else
                    {
                        Debug.Log($"外周のためWallブロックの破壊に失敗しました: {hitObj.name}");
                    }
                }
                else
                {
                    Debug.LogWarning($"ヒットしたWallにNetworkObjectがありません: {hitObj.name}");
                }
            }
            else
            {
                Debug.Log($"Wall以外のオブジェクトにヒットしました: {hitObj.tag}");
            }
        }
        else
        {
            Debug.Log("目の前に破壊できるWallがありません。");
        }
    }

    public void AddWallBreaks(int count)
    {
        remainingWallBreaks += count;
        Debug.Log($"壁破壊回数を{count}回分追加しました。現在の残り回数: {remainingWallBreaks}");
        RunnerUIManager.Instance?.UpdateHammerDisplay(remainingWallBreaks);
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
