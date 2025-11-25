using UnityEngine;
using Fusion;

public class Hammer : NetworkBehaviour
{
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !Object.HasStateAuthority) return;

        if (other.CompareTag("Avatar"))
        {
            hasTriggered = true;
            GetHammer(other);

            GetComponent<Collider>().enabled = false;
        }
    }

    // ハンマーを取得したときの処理
    private void GetHammer(Collider avatar)
    {
        Debug.Log("ハンマーを取得しました！");
        var playerAvatar = avatar.GetComponent<PlayerAvatar>();
        if (playerAvatar != null)
        {
            playerAvatar.AddWallBreaks(1);
        }
        Runner.Despawn(Object);
    }

    public override void FixedUpdateNetwork()
    {
        // 回転させる
        transform.Rotate(Vector3.up, 100 * Runner.DeltaTime, Space.World);
    }
}
