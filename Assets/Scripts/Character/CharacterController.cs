using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private PlayerCanvas convas;
    [SerializeField] private float speedmovement = 10;
    [SerializeField] private float detactRadius = 10;
    [SerializeField] private Animator animator;
    [SerializeField] private LayerMask jarLayer;
    [Space]
    [SerializeField] private GameObject model;

    public Player playerInfo { get; private set; }

    private JarController interacting;

    private bool IsMoveAble;
    public bool IsDead { get; set; }

    private bool isSeek;
    private int breakCount;

    private bool isHiding;

    private bool isAction;

    public void Start()
    {
        GameManager.Instance.CharacterManager.AddCharacter(photonView.ViewID, this);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, detactRadius); 
    }

    public void SetupPlayerData(Player playerInfo)
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

    public void SetupRole(bool isSeek)
    {
        this.isSeek = isSeek;
    }

    public void SetupHidingState()
    {
        if (isSeek)
        {
            isAction = false;
            return;
        }
        isAction = true;
    }

    public void SetupHuntingState(int breakCount = 0)
    {
        if (isSeek)
        {
            isAction = true;
            this.breakCount = breakCount;
            return;
        }
        isAction = false;
    }

    public void TriggerAction()
    {
        if (!PhotonNetwork.LocalPlayer.IsLocal || !isAction)
            return;

        DetectObjective();
    }

    private void DetectObjective()
    {
        var collider = Physics.OverlapSphere(transform.position, detactRadius, jarLayer);

        if (collider.Length <= 0)
            return;

        TriggleAction(collider[0]);
    }

    private void TriggleAction(Collider collider)
    {
        if (isSeek)
        {
            BreakJar();
            return;
        }
        HideAction(collider);
    }

    private void HideAction(Collider collider)
    {
        interacting = collider.gameObject.GetComponent<JarController>();

        isHiding = !isHiding;
        RpcExcute.instance.Rpc_SendHideInJar(!isHiding, interacting.JarId);
    }

    private void BreakJar()
    {
        if (breakCount <= 0)
            return;

        breakCount--;
    }

    public void SetHidigModel(bool isHide, int jarId)
    {
        model.SetActive(!isHide);
        var jar = GameManager.Instance.JarManager.GetJar(jarId);

        if (isHide)
        {
            jar.EntryHide(playerInfo.UserId, this);
            IsMoveAble = false;
            return;
        }

        jar.LeaveHide(playerInfo.UserId);
        IsMoveAble = true;
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
