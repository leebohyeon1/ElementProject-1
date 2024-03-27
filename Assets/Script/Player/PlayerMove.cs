using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using HeoWeb.Fusion;
using UnityEngine.SceneManagement;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public class PlayerMove : NetworkBehaviour
{
    [SerializeField] private NetworkRunner runner;
    [SerializeField] private PlayerStats stats;
    private PlayerMove move;

    // #. ������Ʈ ���� ����
    private Rigidbody rb;

    // #. �̵� ���� ����
    private float horizontal;
    private float vertical;
    private float existingWallJumpTime;
    public LayerMask groundLayer;

    // #. ���� ���� ����
    public GameObject AttackAreaPrefab;
    public Transform SimpleAttackPosition;
    public GameObject Center;

    // #. ���� ���� ����
    public GameObject Body;
    private Material bodyMat;
    private Color redColor = Color.red;
    private Color grayColor = Color.gray;
    private Color originalColor;

    private void Start()
    {
        runner = FindObjectOfType<NetworkRunner>();
        stats = GetComponent<PlayerStats>();
        move = GetComponent<PlayerMove>();
        rb = GetComponent<Rigidbody>();

        // Body ���� ������Ʈ�� Renderer ������Ʈ ��������
        Renderer renderer = Body.GetComponent<Renderer>();
        if (renderer != null) bodyMat = renderer.material;
        originalColor = bodyMat.color;
    }

    private void Update()
    {
        // �÷��̾� ������ ����
        if (this.HasStateAuthority && !(stats.isDie) && stats.CanControl)
        {
            InputKey();
        }
    }
    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority)
        {
            RotateAttackArea();
            if (!stats.isDash) Move();
            BoolSet();
        }
    }


    private void InputKey()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");


        if (Input.GetButtonDown("Jump") && stats.JumpCount != 0 && stats.isWallSliding) WallJump();
        else if (Input.GetButtonDown("Jump") && stats.JumpCount != 0) Jump();

        if (Input.GetKeyDown(KeyCode.LeftShift) && stats.CanDash && !stats.isDash) Dash();

        if (Input.GetMouseButtonDown(0)) Attack();

        if (Input.GetMouseButtonDown(1) && stats.CanGuard) StartCoroutine(Guard());

        if (Input.GetMouseButtonDown(2)) UseSkill();
    }

    private void Move()
    {
        Vector3 PlayerVelocity = new Vector3(horizontal, 0, 0) * stats.playerSpeed * Runner.DeltaTime;
        float fallspeed = rb.velocity.y;

        PlayerVelocity.y = fallspeed;
        rb.velocity = PlayerVelocity;

        if (rb.velocity.y < 0) // �÷��̾ �Ʒ��� �������� ���̸� �߷� �߰�
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (stats.fallMultiplier - 1) * Runner.DeltaTime;
            stats.isJump = false; //�Ʒ��� �������� ���� ���°� Ǯ��
        }
    }

    #region Jump
    private void Jump()
    {
        // rb.AddForce(Vector3.up * stats.JumpForce, ForceMode.Impulse);

        stats.isJump = true;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * stats.JumpForce, ForceMode.Impulse);
        stats.JumpCount--;
    }
    public void WallJump()
    {
        Debug.Log("�� ����");
        stats.JumpCount--;

        existingWallJumpTime = stats.WallJumpTime;


        //�� ���� ���� ��
        stats.isWallJump = true;
        stats.isJump = true;

        rb.velocity = Vector3.zero;
    }
    public void StopWallJump(int Case)
    {
        stats.isWallJump = false;
        switch (Case)
        {
            case 0:
                rb.velocity = Vector3.zero;
                break;
            case 1:
                rb.velocity /= 2;
                break;
        }
        stats.WallJumpTime = existingWallJumpTime;
    }
    #endregion


    #region Dash
    public void Dash()
    {
        Debug.Log("�뽬");

        stats.isWallSliding = false;
        stats.CanDash = false;
        stats.isDash = true;

        Vector3 dashDirection = stats.DashDirection - transform.position;
        rb.velocity = dashDirection.normalized * (stats.dashDistance / stats.dashDuration);

        Debug.Log(dashDirection.normalized);

        StartCoroutine(ReturnDash());
    }
    public IEnumerator ReturnDash()
    {
        yield return new WaitForSeconds(stats.dashDuration);
        rb.velocity = Vector3.zero;
        stats.isDash = false;
        yield return new WaitForSeconds(stats.DashCoolTime);
        stats.CanDash = true;
    }
    #endregion


    private void RotateAttackArea()
    {
        Vector3 mPosition = Input.mousePosition; //���콺 ��ǥ ����
        Vector3 oPosition = transform.position; //���� ������Ʈ ��ǥ ����
        mPosition.z = oPosition.z - Camera.main.transform.position.z;
        Vector3 target = Camera.main.ScreenToWorldPoint(mPosition);
        stats.DashDirection = target;
        float dy = target.y - oPosition.y;
        float dx = target.x - oPosition.x;
        float rotateDegree = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
        Center.transform.rotation = Quaternion.Euler(0f, 0f, rotateDegree);

        if (!stats.isAttack)
        {
            Center.transform.rotation = Quaternion.Euler(0f, 0f, rotateDegree);
        }
    }

    #region Attack

    private void Attack()
    {
        stats.isAttack = true;
        stats.CanAttack = false;
        NetworkObject attackArea = runner.Spawn(AttackAreaPrefab, SimpleAttackPosition.position, SimpleAttackPosition.rotation);

        Invoke("EndAttack", 0.05f);
    }
    public void EndAttack()
    {
        stats.isAttack = false;
        StartCoroutine(ReturnAttack());
    }
    public IEnumerator ReturnAttack()
    {
        yield return new WaitForSeconds(1);
        stats.CanAttack = true;
    }

    #endregion

    #region Skill
    public void UseSkill()
    {

    }
    #endregion

  
    public void TakeDamage(int Damage)
    {
        if (stats.GuardAmount > 0) 
        {
            stats.GuardAmount -= Damage;
            StopCoroutine(DeleteGuard());
            return;
        }
        stats.hp -= Damage;

        
        if(stats.hp >= 1)
        {
            StartCoroutine(ChangeColorCoroutine()); 
        }
        else
        {
            bodyMat.color = grayColor;  
            stats.isDie = true;
        } 
    }


    #region Guard
    private IEnumerator Guard()
    {
        Debug.Log("����");
        stats.CanGuard = false;
        stats.isGuard = true;
        //bodyMat.color = yelloColor;
        yield return new WaitForSeconds(5f);
        stats.isGuard = false;
        //bodyMat.color = originalColor;
        if (!stats.isHitByOtherInGuard)
        {
            StartCoroutine(ReturnGuard(0));
            Debug.Log("���... �׷��� �ƹ� �ϵ� ������.");
        }
        else
        {
            StartCoroutine(ReturnGuard(1));
        }
    }
    public IEnumerator DeleteGuard()
    {
        stats.isGuardSkill = false;
        yield return new WaitForSeconds(stats.GuardDuration);
        stats.GuardAmount = 0;
    }

    private IEnumerator ReturnGuard(int num)
    {

        switch (num)
        {
            case 0:
                yield return new WaitForSeconds(stats.GuardCool);
                stats.CanGuard = true;
                stats.isHitByOtherInGuard = false;
                break;
            case 1:
                //StopCoroutine(ReturnGuard(0));
                yield return new WaitForSeconds(stats.GuardCool / 2);
                stats.CanGuard = true;
                stats.isHitByOtherInGuard = false;
                break;
        }

    }
    #endregion



    private void BoolSet()
    {
        stats.isTouchingRightWall = Physics.Raycast(transform.position, transform.right, 0.51f, groundLayer);
        stats.isTouchingLeftWall = Physics.Raycast(transform.position, -transform.right, 0.51f, groundLayer);
        stats.isGrounded = Physics.CheckSphere(new Vector3(transform.position.x, transform.position.y - 0.5f, 0), 0.1f, groundLayer);

        if (stats.isTouchingLeftWall || stats.isTouchingRightWall)
        {
            stats.isTouchingWall = true;
        }
        else
        {
            stats.isTouchingWall = false;
        }

        if (stats.isWallSliding)
        {
            rb.velocity = new Vector3(rb.velocity.x, -stats.WallSlideSpeed, rb.velocity.z);
        }

        //���� �پ��ְ� �� �������� Ű �Է� �� �� �����̵� ���°� ��(�� �����̵� ���¿����� �� ���� ����)
        if (/*!stats.isGrounded &&*/ stats.isTouchingWall && horizontal == -stats.WallJumpDirection && !stats.isDash/*&& rb.velocity.y < 0  && !Stat.isAttack */)
        {

            stats.isWallSliding = true;
        }
        else
        {
            stats.isWallSliding = false;
        }

        //�� ���� ������ ��
        if (stats.isWallJump)
        {
            stats.WallJumpTime -= Runner.DeltaTime;
            rb.velocity = new Vector3(stats.WallJumpDirection * stats.WallJumpForce, stats.WallJumpForce, 0);
            if (stats.WallJumpTime <= 0)
            {
                StopWallJump(0);
            }
        }
        if (stats.isTouchingWall && stats.isWallJump && stats.WallJumpTime <= existingWallJumpTime - 0.07f)
        {
            //�� ���� �����϶� �ٸ� ���� ������ �� ���� ����
            StopWallJump(0);
        }
        else if (stats.isWallJump && stats.WallJumpTime <= existingWallJumpTime - 0.1f && horizontal != 0)
        {
            //�� ���� �����϶� �����̸� �� ���� ����
            StopWallJump(1);
        }

        if (stats.isDie || stats.isSpasticity || stats.isGuard)
        {
            horizontal = 0;
            stats.CanControl = false;
        }
        else
        {
            stats.CanControl = true;
        }
        if(stats.isGuardSkill)
        {
            StopCoroutine(DeleteGuard());
            StartCoroutine(DeleteGuard());
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (stats.isTouchingWall || stats.isGrounded)
        {
            stats.WallJumpDirection = stats.isTouchingRightWall ? -1 : 1;
            stats.JumpCount = 2;
        }
    }

    private IEnumerator ChangeColorCoroutine()
    {
        Debug.Log("���� ���߽��ϴ�.");

        bodyMat.color = redColor;
        yield return new WaitForSeconds(0.2f);
        bodyMat.color = originalColor;
    }

}
