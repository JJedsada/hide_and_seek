using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private PlayerCanvas convas;
    [SerializeField] private float speedmovement = 10;
    [SerializeField] private Animator animator;

    private Player playerInfo;

    private bool IsMoveAble;
    public bool IsDead { get; set; }

    public void Start()
    {
        GameManager.Instance.CharacterManager.character.Add(photonView.ViewID, this);
        Debug.Log("Add Gameobject");
    }

    public void Setup(Player playerInfo)
    {
        this.playerInfo = playerInfo;
        convas.SetupDisplayName(playerInfo.NickName);
        SetupDefault();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
        }
        else
        {
            // Network player, receive data
        }
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        if (playerInfo == null || !IsMoveAble || IsDead)
            return;

        if (playerInfo.IsLocal)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 diraction = new Vector3(horizontal, transform.position.y, vertical).normalized;

            if (diraction.magnitude > 0)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(diraction), 0.15f);
            }

            transform.Translate(diraction * speedmovement * Time.deltaTime, Space.World);

            animator.SetFloat("Speed", diraction.magnitude);
        }    
    }

    public void SetMoveAble(bool isAble)
    {
        IsMoveAble = isAble;
        if (!isAble)
            animator.SetFloat("Speed", 0);
    }

    public void SetupDefault()
    {
        IsMoveAble = true;
        IsDead = false;
    }
}
