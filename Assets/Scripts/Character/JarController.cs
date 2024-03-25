using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JarController : MonoBehaviour
{
    public int JarId;

    public Dictionary<string, CharacterController> hidingCharacter = new Dictionary<string, CharacterController>();
    [SerializeField] private GameObject spray;
    [SerializeField] private GameObject scan;
    public bool IsBreaked;

    public void Setup()
    {   
        IsBreaked = false;
        Clear();
        ClearSpray();
        ClearScan();
        SetActive(true);
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

    public void Spray()
    {
        spray.gameObject.SetActive(true);
    }

    public void ClearSpray()
    {
        spray.gameObject.SetActive(false);
    }

    public void Scan()
    {
        scan.gameObject.SetActive(hidingCharacter.Keys.Count > 0);
    }

    public void ClearScan()
    {
        scan.gameObject.SetActive(false);
    }

    public void LeaveHide(string userId)
    {
        hidingCharacter.Remove(userId);
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}
