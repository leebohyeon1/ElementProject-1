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

    // #. 컴포넌트 관련 변수
    private Rigidbody rb;

    // #. 이동 관련 변수
    private float horizontal;
    private float vertical;
    private float existingWallJumpTime;
    public LayerMask groundLayer;

    // #. 공격 관련 변수
    public GameObject AttackAreaPrefab;
    public Transform SimpleAttackPosition;
    public GameObject Center;

    // #. 색상 관련 변수
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

        // Body 게임 오브젝트의 Renderer 컴포넌트 가져오기
        Renderer renderer = Body.GetComponent<Renderer>();
        if (renderer != null) bodyMat = renderer.material;
        originalColor = bodyMat.color;
    }

    private void Update()
    {
        // 플레이어 움직임 관련
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

        if (rb.velocity.y < 0) // 플레이어가 아래로 떨어지는 중이면 중력 추가
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (stats.fallMultiplier - 1) * Runner.DeltaTime;
            stats.isJump = false; //아래로 떨어지면 점프 상태가 풀림
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
        Debug.Log("벽 점프");
        stats.JumpCount--;

        existingWallJumpTime = stats.WallJumpTime;


        //벽 점프 상태 온
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
        Debug.Log("대쉬");

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
        Vector3 mPosition = Input.mousePosition; //마우스 좌표 저장
        Vector3 oPosition = transform.position; //게임 오브젝트 좌표 저장
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
    public void GetSkill()
    {
        for (int i = 0; i < stats.HaveSkill_ID.Length; i++)
        {
            //if (stats.HaveSkill_ID[i] == null)
            //{
            //    stats.HaveSkill_ID[i] = null;
            //}


        }

    }
    #endregion

  
    public void TakeDamage()
    {
        stats.hp--;

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
        Debug.Log("가드");
        stats.CanGuard = false;
        stats.isGuard = true;
        //bodyMat.color = yelloColor;
        yield return new WaitForSeconds(5f);
        stats.isGuard = false;
        //bodyMat.color = originalColor;
        if (!stats.isHitByOtherInGuard)
        {
            StartCoroutine(ReturnGuard(0));
            Debug.Log("방어... 그러나 아무 일도 없었다.");
        }
        else
        {
            StartCoroutine(ReturnGuard(1));
        }
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

        //벽에 붙어있고 벽 방향으로 키 입력 시 벽 슬라이딩 상태가 됨(벽 슬라이딩 상태에서만 벽 점프 가능)
        if (/*!stats.isGrounded &&*/ stats.isTouchingWall && horizontal == -stats.WallJumpDirection && !stats.isDash/*&& rb.velocity.y < 0  && !Stat.isAttack */)
        {

            stats.isWallSliding = true;
        }
        else
        {
            stats.isWallSliding = false;
        }

        //벽 점프 상태일 때
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
            //벽 점프 상태일때 다른 벽에 붙으면 벽 점프 멈춤
            StopWallJump(0);
        }
        else if (stats.isWallJump && stats.WallJumpTime <= existingWallJumpTime - 0.1f && horizontal != 0)
        {
            //벽 점프 상태일때 움직이면 벽 점프 멈춤
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
        Debug.Log("공격 당했습니다.");

        bodyMat.color = redColor;
        yield return new WaitForSeconds(0.2f);
        bodyMat.color = originalColor;
    }

}
