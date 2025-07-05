using UnityEngine;
using Fusion;
using System.Collections;

public class BlindTrap : Trap
{
    [SerializeField]
    private float trapDuration = 10f; // トラップの効果時間
    protected override void TriggerEffect(Collider avatar)
    {
        Debug.Log("トラップに引っかかった！");
        var controller = avatar.GetComponent<NetworkCharacterController>();
        if (controller != null)
        {
            GameObject blindMask = RunnerUIManager.Instance.blindMask;
            blindMask.SetActive(true);
            RunnerUIManager.Instance.ActivateBlind(trapDuration);
            Destroy(gameObject);
        }
    }
}
