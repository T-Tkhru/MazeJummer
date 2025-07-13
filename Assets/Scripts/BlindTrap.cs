using UnityEngine;
using Fusion;

public class BlindTrap : Trap
{
    [SerializeField] private float trapDuration = 10f; // トラップの効果時間
    protected override void TriggerEffect(Collider avatar)
    {
        Debug.Log("トラップに引っかかった！");
        var controller = avatar.GetComponent<NetworkCharacterController>();
        var playerAvatar = avatar.GetComponent<PlayerAvatar>();
        if (controller != null)
        {
            playerAvatar.ActivateBlind(trapDuration);
            Runner.Despawn(Object); // トラップオブジェクトを非表示にする
        }
    }
}
