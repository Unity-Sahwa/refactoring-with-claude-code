using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerInputAction : IInjectable
{
    List<InputActionEnum> InputActionKey { get; }
} 
