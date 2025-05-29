using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerAvatarView : MonoBehaviour
{
    [SerializeField]
    private TextMeshPro nameLabel;
    public void SetCameraTarget()
    {
        var cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        if (cinemachineCamera != null)
        {
            cinemachineCamera.LookAt = transform;
            cinemachineCamera.Follow = transform;
        }
    }

    public void SetNickName(string nickName)
    {
        nameLabel.text = nickName;
    }

    private void LateUpdate()
    {
        nameLabel.transform.rotation = Camera.main.transform.rotation;
    }
}