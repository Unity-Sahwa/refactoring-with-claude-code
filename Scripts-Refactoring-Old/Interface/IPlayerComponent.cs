using UnityEngine;

public interface IPlayerComponent : IInjectable
{
    public PlayerComponentEnum ComponentID { get; }
}
