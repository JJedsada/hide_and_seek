using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerCanvas : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;

    private Camera camera;

    private void Start()
    {
        camera = Camera.main;
    }

    private void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - camera.transform.position);
    }

    public void SetupDisplayName(string name)
    {
        nameText.text = name;
    }

    public void SetupDisplayOwner()
    {
        nameText.color = Color.green;
    }
}
