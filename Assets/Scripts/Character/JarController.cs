using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JarController : MonoBehaviour
{
    public int JarId;

    public Dictionary<string, CharacterController> hidingCharacter = new Dictionary<string, CharacterController>();

    public bool IsBreaked;

    public void Setup()
    {
        IsBreaked = false;
        Clear();
    }

    public Dictionary<string, CharacterController> Breaking()
    {
        IsBreaked = true;
        return hidingCharacter;
    }

    public void Clear()
    {
        hidingCharacter.Clear();
    }

    public void EntryHide(string userId, CharacterController characterController)
    {
        hidingCharacter.Add(userId, characterController);
    }

    public void LeaveHide(string userId)
    {
        hidingCharacter.Remove(userId);
    }
}
