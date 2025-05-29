using Unity.Cinemachine;
using UnityEngine;

public class PlayerAvatarView : MonoBehaviour
{
    public void SetCameraTarget()
    {
        var cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        if (cinemachineCamera != null)
        {
            cinemachineCamera.LookAt = transform;
            cinemachineCamera.Follow = transform;
        }
    }
}