using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
using HeoWeb.Fusion;
using System;

// #. �÷��̾� ���� ������
public class PlayerStats : NetworkBehaviour
{
    [Networked(OnChanged = nameof(UpdatePlayerName))] public NetworkString<_32> PlayerName { get; set; }// ���ڿ��� ���̸� 32�ڷ� ����
    [Networked(OnChanged = nameof(UpdateHat))] public int hatIndex { get; set; }// ���ڿ��� ���̸� 32�ڷ� ����

    [SerializeField] TMP_Text playerNameLabel;

    public static PlayerStats instance;

    private GameObject currentHat = null;

    [Networked] public int hp { get; set; }


    [Header("�÷��̾� ����")]
    public bool CanControl;
    public bool isDie;
    public bool isPlayer;
    [Space(3f)]

    [Header("�̵�")]
    public float playerSpeed;
    public float fallMultiplier = 2.5f;
    [Space(3f)]

    [Header("����")]
    public bool isJump;
    public bool isWallJump;
    public int JumpCount;
    public float JumpForce;
    public float WallJumpTime = 0.3f;
    public float WallJumpForce = 7;
    [Space(3f)]

    [Header("�뽬")]
    public Vector3 DashDirection;
    public bool CanDash;
    public bool isDash;
    public float DashForce;
    public float dashDistance;
    public float dashDuration;
    public float DashCoolTime;
    [Space(3f)]

    [Header("����")]
    public bool isAttack;
    public bool CanAttack;
    //[Networked]
    //public Vector3 AttackScale { get; set; }
    [Space(3f)]

    [Header("��ų")]
    public Skill[] playerSkills;
    public bool isGuardSkill;
    [Space(3f)]

    [Header("����")]
    public bool isGuard;
    public bool CanGuard;
    public float GuardCool;
    public bool isHitByOtherInGuard;
    public int GuardAmount;
    public float GuardDuration;
    [Space(3f)]

    [Header("����")]
    public bool isSpasticity;


    [Header("���� �ν�")]
    public bool isTouchingWall;
    public bool isTouchingRightWall;
    public bool isTouchingLeftWall;
    public bool isGrounded;
    [Space(3f)]

    [Header("��")]
    public bool isWallSliding;
    public float WallSlideSpeed = 2f;
    public int WallJumpDirection;

    [SerializeField] private Transform playerHead;

    private void Start()
    {
        if (this.HasStateAuthority)
        {
            CanGuard = true;
            CanControl = true;
            CanDash = true;
            CanAttack = true;

            PlayerName = FusionConnection.instance._playerNmae;
            if (instance == null) { instance = this; }

            hp = 2;
        }
    }

    protected static void UpdatePlayerName(Changed<PlayerStats> changed)
    {
        changed.Behaviour.playerNameLabel.text = changed.Behaviour.PlayerName.ToString();
    }

    protected static void UpdateHat(Changed<PlayerStats> changed)
    {
        int _hatIndex = changed.Behaviour.hatIndex;
        GameObject _currentHat = changed.Behaviour.currentHat;

        GameObject hat = Hats.hats[_hatIndex];

        if(hat != null)
        {
            Destroy(_currentHat);
        }

        GameObject newHat = GameObject.Instantiate(hat);
        newHat.transform.parent = changed.Behaviour.playerHead;
        newHat.transform.localPosition = Vector3.zero;
        newHat.transform.localEulerAngles = Vector3.zero;
        newHat.GetComponent<Collider>().enabled = false;

        changed.Behaviour.currentHat = newHat;
    }

}
    