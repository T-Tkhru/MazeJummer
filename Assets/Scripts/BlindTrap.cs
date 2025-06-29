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
        GameObject canvas = GameObject.Find("Canvas");
        var controller = avatar.GetComponent<NetworkCharacterController>();
        if (controller != null)
        {
            float defaultSpeed = controller.maxSpeed;
            StartCoroutine(RecoverBlindAfterDelay());
        }
    }

    private IEnumerator RecoverBlindAfterDelay()
    {
        yield return new WaitForSeconds(trapDuration);
        Destroy(gameObject);
    }
}
