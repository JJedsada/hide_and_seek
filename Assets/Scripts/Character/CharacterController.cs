using Cysharp.Threading.Tasks;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class CharacterController : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private PlayerCanvas canvas;
    [SerializeField] private float speedmovement = 10;
    [SerializeField] private float detactRadius = 10;
    [SerializeField] private Animator animator;
    [SerializeField] private LayerMask jarLayer;
    [Space]
    [SerializeField] private GameObject model;

    public Player playerInfo { get; private set; }

    private JarController interacting;

    private bool IsMoveAble;
    public bool IsDead { get; private set; }
    public bool isSeek { get; private set; }
    public bool isHiding { get; private set; }
    public int Score { get; set; } = 0;

    private bool isAction;
    private int breakCount;

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
        if (playerInfo.UserId == PhotonNetwork.LocalPlayer.UserId)
            canvas.SetupDisplayOwner();

        this.playerInfo = playerInfo;
        canvas.SetupDisplayName(playerInfo.NickName);
        SetupDefault();
    }

    private void Update()
    {
        ShowDetectObjective();
        Move(); 
    }

    private void Move()
    {
        if (playerInfo == null || !IsMoveAble || IsDead)
            return;

        if (playerInfo.IsLocal)
        {
            float horizontal = UIManager.Instance.GameplayPanel.joystick.Horizontal;
            float vertical = UIManager.Instance.GameplayPanel.joystick.Vertical;

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
        UIManager.Instance.GameplayPanel.SetupHideAction();
    }

    public void SetupHuntingState(int breakCount = 0)
    {
        if (isSeek)
        {
            isAction = true;
            this.breakCount = breakCount;
            UIManager.Instance.GameplayPanel.SetupPunchAction(breakCount);
            return;
        }
        isAction = false;
    }

    public void ClearPlayerAction()
    {
        isAction = false;
    }

    public void TriggerAction()
    {
        if (!PhotonNetwork.LocalPlayer.IsLocal || !isAction)
            return;

        var interacting = DetectObjective();
        if (!interacting)
            return;
        TriggleAction(interacting);
    }

    private void ShowDetectObjective()
    {
        if (PhotonNetwork.LocalPlayer.UserId != playerInfo.UserId)
        {
            return;
        }

        if (!isAction)
        {
            GameManager.Instance.JarManager.ShowInteraction(null);
            return;
        }

        var interacting = DetectObjective();
        GameManager.Instance.JarManager.ShowInteraction(interacting);
    }

    private Collider DetectObjective()
    {
        var collider = Physics.OverlapSphere(transform.position, detactRadius, jarLayer);

        if (collider.Length <= 0)
        {
            return null;
        }
        return collider[0];
    }

    private void TriggleAction(Collider collider)
    {
        if (isSeek)
        {
            BreakJar(collider);
            return;
        }
        HideAction(collider);
    }

    private void HideAction(Collider collider)
    {
        if (IsDead)
            return;

        interacting = collider.gameObject.GetComponent<JarController>();
        RpcExcute.instance.Rpc_SendHideInJar(!isHiding, interacting.JarId);
    }

    private void BreakJar(Collider collider)
    {
        interacting = collider.gameObject.GetComponent<JarController>();

        if (breakCount <= 0)
            return;

        breakCount--;
        UIManager.Instance.GameplayPanel.SetupPunchCount(breakCount);
        Vector3 diraction = interacting.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(diraction);
        RpcExcute.instance.Rpc_SendBreakJar(interacting.JarId);
    }

    public void SetHideInJar(string userId, bool isHide, int jarId)
    {
        isHiding = isHide;

        SetHideModel(!isHide);
        var jar = GameManager.Instance.JarManager.GetJar(jarId);
        if (isHide)
        {
            jar.EntryHide(userId, this);
            IsMoveAble = false;
            return;
        }

        jar.LeaveHide(userId);
        IsMoveAble = true;
    }

    public void OutHide()
    {
        if (interacting == null)
            return;

        RpcExcute.instance.Rpc_SendHideInJar(false, interacting.JarId);
    }

    public void SetHideModel(bool active)
    {
        model.SetActive(active);
    }

    public void SetActiveNameDisplay(bool active)
    {
        canvas.gameObject.SetActive(active);
    }

    public void SetMoveAble(bool isAble)
    {
        IsMoveAble = isAble;
        if (!isAble)
            animator.SetFloat("Speed", 0);
    }

    public async UniTask Breaking()
    {
        animator.SetTrigger("IsBreakJar");
        IsMoveAble = false;
        await UniTask.Delay(1000);
        IsMoveAble = true;
    }

    public async UniTask Dead()
    {
        IsDead = true;
        await UniTask.Delay(700);

        if (model)
        {
            model.SetActive(true);
        }
      
        UIManager.Instance.GameplayPanel.GetElement(playerInfo.UserId).SetupDead(IsDead);
        animator.SetTrigger("IsDead");
    }

    public void SetupDefault()
    {
        canvas.gameObject.SetActive(true);
        IsMoveAble = true;
        IsDead = false;
        Score = 0;
        isAction = false;
        UIManager.Instance.GameplayPanel.GetElement(playerInfo.UserId).SetupDead(IsDead);
        animator.Play("Idle");
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
       
    }
}
