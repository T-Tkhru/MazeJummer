using System.Collections;
using System.Linq;
using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using UnityEngine;
public class GameManager : NetworkBehaviour
{
    [Networked] private TickTimer Timer { get; set; }
    [Networked] private NetworkBool isGameStarted { get; set; } = false; // ゲームが開始されているかどうか
    [Networked] private NetworkBool isGameFinished { get; set; } = false; // ゲームが終了したかどうか
    [SerializeField] private int timeLimit = 180; // 制限時間（秒）
    private NetworkBool isCountDownTriggered { get; set; } = false;
    [Networked] private NetworkBool isClientReady { get; set; } = false; // クライアントが準備完了かどうか
    [Networked] private TickTimer GameStartTimer { get; set; } // ゲーム開始のカウントダウンタイマー
    // [SerializeField] private bool isSoloMode = false; // ソロモードかどうか
    private bool isSoloMode = false; // 一時的にインスペクターから設定できないようにする
    [SerializeField] private int maxTraps = 3; // 最大トラップ数
    [Networked] private NetworkBool isRunnerWin { get; set; } = false; // ランナーが勝利したかどうか

    public override void Spawned()
    {
        // マスタークライアントが、残り時間のタイマー（ネットワークプロパティ）を60秒に設定する
        if (Runner.IsServer)
        {
            Timer = TickTimer.CreateFromSeconds(Runner, timeLimit);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (isSoloMode) // ソロモードの場合
        {
            SoloModeStart();
        }
        else // マルチプレイヤーモードの場合
        {
            MultiPlayerModeStart();
        }
        if (isGameStarted && !isGameFinished) // ゲームが開始されていて、まだ終了していない場合
        {
            if (Timer.IsRunning && Timer.Expired(Runner)) // タイマーが動いていて、期限切れになった場合
            {
                FinishGame(); // ゲームを終了する
            }
        }

    }

    private void SoloModeStart()
    {
        if (!isCountDownTriggered) // ゲームが開始されていない場合

        {
            isCountDownTriggered = true; // ゲームが開始されたことを示すフラグを立てる
            Debug.Log("ゲーム開始のカウントダウンを開始します。");
            StartGameCountdown(); // ゲーム開始のカウントダウンを開始
        }
        if (GameStartTimer.IsRunning && GameStartTimer.Expired(Runner)) // ゲーム開始のカウントダウンが終了した場合
        {
            Debug.Log("ゲーム開始のカウントダウンが終了しました。");
            GameStartTimer = TickTimer.None; // ゲーム開始のカウントダウンタイマーを無効化
        }
    }

    private void MultiPlayerModeStart()
    {
        if (Runner.ActivePlayers.Count() == 2)
        {
            if (!isCountDownTriggered && isClientReady) // ゲームが開始されていない、かつクライアントが準備完了の場合

            {
                isCountDownTriggered = true; // ゲームが開始されたことを示すフラグを立てる
                Debug.Log("ゲーム開始のカウントダウンを開始します。");
                StartGameCountdown(); // ゲーム開始のカウントダウンを開始
            }
            if (GameStartTimer.IsRunning && GameStartTimer.Expired(Runner)) // ゲーム開始のカウントダウンが終了した場合
            {
                Debug.Log("ゲーム開始のカウントダウンが終了しました。");
                GameStartTimer = TickTimer.None; // ゲーム開始のカウントダウンタイマーを無効化
                StartGame(); // タイマーを開始
            }
        }
    }

    public void StartGame()
    {
        if (Runner.IsServer && !isGameStarted) // サーバーであり、タイマーがまだ開始されていない場合
        {
            isGameStarted = true; // タイマーを開始
            Timer = TickTimer.CreateFromSeconds(Runner, timeLimit);
        }
    }
    public void FinishGame()
    {
        var playerAvatar = GameObject.FindWithTag("Avatar").GetComponent<PlayerAvatar>();
        if (playerAvatar != null)
        {
            playerAvatar.ResetSpeed(); // プレイヤーの速度をリセット
        }
        if (Runner.IsServer && !isGameFinished)
        {
            isGameFinished = true; // ゲームが終了したことを示す
            if (Timer.Expired(Runner))
            {
                isRunnerWin = false;
            }
            else
            {
                isRunnerWin = true;
            }
            // 4秒待つ
            StartCoroutine(WaitBeforeShutdown());
        }
    }
    private IEnumerator WaitBeforeShutdown()
    {
        yield return new WaitForSeconds(4f);
        Debug.Log("ゲームを終了します。");
        Runner.Shutdown();
    }
    private void StartGameCountdown()
    {
        if (Runner.IsServer)
        {
            Debug.Log("Game start countdown initiated.");
            GameStartTimer = TickTimer.CreateFromSeconds(Runner, 5f);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ClientReady()
    {
        Debug.Log($"client is ready!");

        if (Runner.IsServer)
        {
            isClientReady = true; // クライアントが準備完了であることを示す
        }
    }

    public int GetCountdownSeconds()
    {
        if (GameStartTimer.IsRunning)
        {
            float remaining = GameStartTimer.RemainingTime(Runner) ?? 0f;
            return Mathf.CeilToInt(remaining);
        }
        return -1; // カウントダウン中でない
    }

    public bool IsGameStarted()
    {
        return isGameStarted; // ゲームが開始されているかどうかを返す
    }
    public bool IsGameFinished()
    {
        return isGameFinished; // ゲームが終了したかどうかを返す
    }

    public float GetRemainingTime()
    {
        return Timer.RemainingTime(Runner) ?? 0f;
    }
    public int GetTimeLimit()
    {
        return timeLimit; // 制限時間を返す
    }

    public int GetMaxTraps()
    {
        return maxTraps; // 最大トラップ数を返す
    }
    public bool IsRunnerWin()
    {
        return isRunnerWin; // ランナーが勝利したかどうかを返す
    }

}
