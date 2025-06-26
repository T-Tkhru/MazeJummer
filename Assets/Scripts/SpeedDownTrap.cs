using UnityEngine;
using Fusion;
using System.Collections;

public class SpeedDownTrap : Trap
{
    [SerializeField]
    private float trapDuration = 10f; // トラップの効果時間
    [SerializeField]
    private float slowSpeed = 1.25f; // トラップに引っかかったときの速度
    protected override void TriggerEffect(Collider avatar)
    {
        Debug.Log("トラップに引っかかった！");
        GetComponent<Collider>().enabled = false;
        var controller = avatar.GetComponent<NetworkCharacterController>();
        if (controller != null)
        {
            float defaultSpeed = controller.maxSpeed;
            controller.maxSpeed = slowSpeed;
            StartCoroutine(RecoverSpeedAfterDelay(controller, defaultSpeed));
        }
    }

    private IEnumerator RecoverSpeedAfterDelay(NetworkCharacterController controller, float defaultSpeed)
    {
        yield return new WaitForSeconds(trapDuration);
        controller.maxSpeed = defaultSpeed;
        Destroy(gameObject);
    }
}
