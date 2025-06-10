using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector3 Direction;
    public NetworkButtons Buttons;
    public Vector2 Look;
}

public enum NetworkInputButtons
{
    Jump
}