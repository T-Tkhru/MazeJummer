using UnityEngine;
using Fusion;
using System.Collections;

public class BlindTrap : Trap
{
    [SerializeField]
    private float trapDuration = 10f; // トラップの効果時間
    private GameObject blindMask;
    [SerializeField]
    private GameObject blindMaskPrefab; // BlindMaskのプレハブ

    protected override void TriggerEffect(Collider avatar)
    {
        Debug.Log("トラップに引っかかった！");
        Canvas canvas = FindFirstObjectByType<Canvas>();
        var controller = avatar.GetComponent<NetworkCharacterController>();
        if (controller != null)
        {
            GameObject blindMask = RunnerUIManager.Instance.BlindMask;
            blindMask.SetActive(true);
            StartCoroutine(RecoverBlindAfterDelay(blindMask));
        }
    }

    private IEnumerator RecoverBlindAfterDelay(GameObject blindMask)
    {
        yield return new WaitForSeconds(trapDuration);
        blindMask.SetActive(false);
        Destroy(gameObject);
    }
}
