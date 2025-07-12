using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerAvatarView : MonoBehaviour
{
    [SerializeField] private TextMeshPro nameLabel;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    public void SetCameraTarget()
    {
        cinemachineCamera.Priority.Value = 100;
    }
}