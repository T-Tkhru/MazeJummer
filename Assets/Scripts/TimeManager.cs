using System.Security.Cryptography.X509Certificates;
using Fusion;
using NUnit.Framework;
using TMPro;
using UnityEngine;
public class TimeManager : NetworkBehaviour
{
    [SerializeField]
    private TextMeshProUGUI timeLabel;
    [Networked]
    private TickTimer Timer { get; set; }
    [Networked]
    private NetworkBool IsStarted { get; set; } = false; // タイマーが動作中かどうか
    [Networked]
    private NetworkBool IsStopped { get; set; } = false; // タイマーが停止中かどうか
    private float maxTime = 300f; // タイマーの最大時間
    private float remainingTime;
    private float seconds;
    private int minutes;

    public override void Spawned()
    {
        // マスタークライアントが、残り時間のタイマー（ネットワークプロパティ）を60秒に設定する
        if (Runner.IsServer)
        {
            // タイマーを300秒（5分）に設定
            Timer = TickTimer.CreateFromSeconds(Runner, 300f);
        }
    }
    public override void Render()
    {
        if (!IsStarted)
        {
            return; // タイマーが動作中でない、またはサーバーでない場合は何もしない
        }
        else if (IsStopped)
        {
            seconds = maxTime - remainingTime; // 300秒から残り時間を引く
            minutes = Mathf.FloorToInt(seconds / 60); // 分を計算
            Debug.Log($"seconds: {seconds}, minutes: {minutes}");
            timeLabel.SetText($"{minutes:D2}:{(int)seconds % 60:D2}"); // タイマーの表示を更新
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
        if (Runner.IsServer && !IsStarted) // サーバーであり、タイマーがまだ開始されていない場合
        {
            IsStarted = true; // タイマーを開始
            Timer = TickTimer.CreateFromSeconds(Runner, maxTime); // タイマーを300秒に設定
        }
        else if (Runner.IsServer && IsStarted)
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
        }
    }
}
