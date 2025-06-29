using UnityEngine;

public class SpeedDownTrap : Trap
{
    [SerializeField]
    private float trapDuration = 10f; // トラップの効果時間
    protected override void TriggerEffect(Collider avatar)
    {
        Debug.Log("トラップに引っかかった！");
        var playerAvatar = avatar.GetComponent<PlayerAvatar>();
        if (playerAvatar != null)
        {
            playerAvatar.ActivateSpeedDown(trapDuration);
            Debug.Log("プレイヤーの速度をダウンさせました。");
            Destroy(gameObject);
        }
    }
}
