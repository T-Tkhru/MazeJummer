using System.Collections;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Fusion;
using NUnit.Framework;
using TMPro;
using UnityEngine;
public class GameManager : NetworkBehaviour
{
    [SerializeField]
    private TextMeshProUGUI timeLabel;
    [Networked]
    private TickTimer Timer { get; set; }
    [Networked]
    private NetworkBool IsTimerStarted { get; set; } = false; // タイマーが動作中かどうか
    [Networked]
    private NetworkBool IsStopped { get; set; } = false; // タイマーが停止中かどうか
    private float maxTime = 300f; // タイマーの最大時間
    private float remainingTime;
    private float seconds;
    private int minutes;
    private bool isGameStarted = false;
    [Networked]
    private NetworkBool IsClientReady { get; set; } = false; // クライアントが準備完了かどうか

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
        if (Runner.ActivePlayers.Count() == 2)
        {
            if (!isGameStarted && IsClientReady) // ゲームが開始されていない、かつクライアントが準備完了の場合

            {
                isGameStarted = true; // ゲームが開始されたことを示すフラグを立てる
                Debug.Log("ゲーム開始のカウントダウンを開始します。");
                StartCoroutine(StartGameCountdown()); // ゲーム開始のカウントダウンを開始
            }
        }
    }
    public override void Render()
    {
        if (!IsTimerStarted || IsStopped) // タイマーが開始されていない、または停止中の場合
        {
            return;
        }
        else
        {
            // 300 - Timerで経過時間を表示する
            remainingTime = Timer.RemainingTime(Runner) ?? 0f;
            seconds = maxTime - remainingTime; // 300秒から残り時間を引く
            minutes = Mathf.FloorToInt(seconds / 60); // 分を計算

            timeLabel.SetText($"{minutes:D2}:{(int)seconds % 60:D2}");
        }
    }

    public void StartTimer()
    {
        if (Runner.IsServer && !IsTimerStarted) // サーバーであり、タイマーがまだ開始されていない場合
        {
            IsTimerStarted = true; // タイマーを開始
            Timer = TickTimer.CreateFromSeconds(Runner, maxTime); // タイマーを300秒に設定
        }
        else if (Runner.IsServer && IsTimerStarted)
        {
            // 再スタート（Tickは動いてるから、一気に時間飛ぶ）
            IsStopped = false; // タイマーが停止中でないことを示す
        }
    }
    public void StopTimer()
    {
        if (Runner.IsServer && !IsStopped)
        {
            IsStopped = true; // タイマーが停止中であることを示す
            remainingTime = Timer.RemainingTime(Runner) ?? 0f; // 残り時間を取得
            seconds = maxTime - remainingTime; // 300秒から残り時間を引く
            minutes = Mathf.FloorToInt(seconds / 60); // 分を計算
            timeLabel.SetText($"{minutes:D2}:{(int)seconds % 60:D2}"); // タイマーの表示を更新
            Debug.Log($"seconds: {seconds}, minutes: {minutes}");
        }
    }

    private IEnumerator StartGameCountdown()
    {
        if (!Runner.IsServer) yield break;

        Debug.Log("Countdown start!");

        for (int i = 3; i > 0; i--)
        {
            Debug.Log($"Countdown: {i}");
            yield return new WaitForSeconds(1f); // 1秒待つ
        }
        Debug.Log("Game start!");
        StartTimer(); // 実際のタイマー開始
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ClientReady()
    {
        Debug.Log($"client is ready!");

        if (Runner.IsServer)
        {
            IsClientReady = true; // クライアントが準備完了であることを示す
        }
    }

}
