using UnityEngine;
using Fusion;
using System.Collections;

public class Trap1 : NetworkBehaviour
{
    [SerializeField]
    private float trapDuration = 10f; // トラップの効果時間
    [SerializeField]
    private float slowSpeed = 1.25f; // トラップに引っかかったときの速度
    private void OnTriggerEnter(Collider other)
    {
        if (!Object.HasStateAuthority)
        {
            Debug.Log("トラップに引っかかったが、権限がないため処理をスキップします。");
            return; // 権限がない場合は何もしない
        }

        if (other.CompareTag("Avatar"))
        {
            Debug.Log("トラップに引っかかった！");
            GetComponent<Collider>().enabled = false;
            var controller = other.GetComponent<NetworkCharacterController>();
            if (controller != null)
            {
                float defaultSpeed = controller.maxSpeed;
                controller.maxSpeed = slowSpeed;
                StartCoroutine(RecoverSpeedAfterDelay(controller, trapDuration, defaultSpeed));
            }
        }
    }

    private IEnumerator RecoverSpeedAfterDelay(NetworkCharacterController controller, float delay, float defaultSpeed)
    {
        yield return new WaitForSeconds(delay);
        controller.maxSpeed = defaultSpeed;
        Destroy(gameObject);
    }
}
