using System.Linq;
using Fusion;
using UnityEngine;
public class GameManager : NetworkBehaviour
{
    [Networked]
    private TickTimer Timer { get; set; }
    [Networked]
    private NetworkBool isGameStarted { get; set; } = false; // ゲームが開始されているかどうか
    [Networked]
    private NetworkBool isGameFinished { get; set; } = false; // ゲームが終了したかどうか
    private float maxTime = 300f; // タイマーの最大時間
    private float remainingTime;
    private float seconds;
    private int minutes;
    private NetworkBool isGameReady { get; set; } = false; // ゲームが開始されているかどうか
    [Networked]
    private NetworkBool isClientReady { get; set; } = false; // クライアントが準備完了かどうか
    [Networked]
    private TickTimer GameStartTimer { get; set; } // ゲーム開始のカウントダウンタイマー
    [SerializeField]
    private bool isSoloMode = false; // ソロモードかどうか
    [SerializeField]
    private int maxTraps = 3; // 最大トラップ数

    public override void Spawned()
    {
        // マスタークライアントが、残り時間のタイマー（ネットワークプロパティ）を60秒に設定する
        if (Runner.IsServer)
        {
            // タイマーを300秒（5分）に設定
            Timer = TickTimer.CreateFromSeconds(Runner, maxTime);
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

    }

    private void SoloModeStart()
    {
        if (!isGameReady) // ゲームが開始されていない場合

        {
            isGameReady = true; // ゲームが開始されたことを示すフラグを立てる
            Debug.Log("ゲーム開始のカウントダウンを開始します。");
            StartGameCountdown(); // ゲーム開始のカウントダウンを開始
        }
        if (GameStartTimer.IsRunning && GameStartTimer.Expired(Runner)) // ゲーム開始のカウントダウンが終了した場合
        {
            Debug.Log("ゲーム開始のカウントダウンが終了しました。");
            GameStartTimer = TickTimer.None; // ゲーム開始のカウントダウンタイマーを無効化
            StartTimer(); // タイマーを開始
        }
    }

    private void MultiPlayerModeStart()
    {
        if (Runner.ActivePlayers.Count() == 2)
        {
            if (!isGameReady && isClientReady) // ゲームが開始されていない、かつクライアントが準備完了の場合

            {
                isGameReady = true; // ゲームが開始されたことを示すフラグを立てる
                Debug.Log("ゲーム開始のカウントダウンを開始します。");
                StartGameCountdown(); // ゲーム開始のカウントダウンを開始
            }
            if (GameStartTimer.IsRunning && GameStartTimer.Expired(Runner)) // ゲーム開始のカウントダウンが終了した場合
            {
                Debug.Log("ゲーム開始のカウントダウンが終了しました。");
                GameStartTimer = TickTimer.None; // ゲーム開始のカウントダウンタイマーを無効化
                StartTimer(); // タイマーを開始
            }
        }
    }

    public void StartTimer()
    {
        if (Runner.IsServer && !isGameStarted) // サーバーであり、タイマーがまだ開始されていない場合
        {
            isGameStarted = true; // タイマーを開始
            Timer = TickTimer.CreateFromSeconds(Runner, maxTime); // タイマーを300秒に設定
        }
        else if (Runner.IsServer && isGameStarted)
        {
            // 再スタート（Tickは動いてるから、一気に時間飛ぶ）
            isGameFinished = false; // タイマーが停止中でないことを示す
        }
    }
    public void StopTimer()
    {
        if (Runner.IsServer && !isGameFinished)
        {
            isGameFinished = true; // タイマーが停止中であることを示す
            remainingTime = Timer.RemainingTime(Runner) ?? 0f; // 残り時間を取得
            seconds = maxTime - remainingTime; // 300秒から残り時間を引く
            minutes = Mathf.FloorToInt(seconds / 60); // 分を計算
            Debug.Log($"seconds: {seconds}, minutes: {minutes}");
        }
    }
    private void StartGameCountdown()
    {
        if (Runner.IsServer)
        {
            Debug.Log("Game start countdown initiated.");
            GameStartTimer = TickTimer.CreateFromSeconds(Runner, 3f); // 3秒のカウントダウンタイマーを設定
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

    public int GetMaxTraps()
    {
        return maxTraps; // 最大トラップ数を返す
    }
}
