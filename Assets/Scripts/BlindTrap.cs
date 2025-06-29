using UnityEngine;
using Fusion;
using System.Collections;

public class BlindTrap : Trap
{
    [SerializeField]
    private float trapDuration = 10f; // トラップの効果時間
    [SerializeField]
    private GameObject blindEffectPrefab; // ブラインド効果のプレハブ
    protected override void TriggerEffect(Collider avatar)
    {
        Debug.Log("トラップに引っかかった！");
        Canvas canvas = FindFirstObjectByType<Canvas>();
        var controller = avatar.GetComponent<NetworkCharacterController>();
        if (controller != null)
        {
            GameObject blindEffect = Instantiate(blindEffectPrefab, canvas.transform);
            StartCoroutine(RecoverBlindAfterDelay(blindEffect));
        }
    }

    private IEnumerator RecoverBlindAfterDelay(GameObject blindEffect)
    {
        yield return new WaitForSeconds(trapDuration);
        Destroy(blindEffect); // ブラインド効果を削除
        Destroy(gameObject);
    }
}
