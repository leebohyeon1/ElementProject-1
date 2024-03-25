using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "Skills/ActiveSkill/Catapult")]
public class CatapultSkill : Skill
{
    public int PingPongCount;
    public int Damage;
    public int Speed;
    public GameObject StonePrefab;

    public override void Activate(GameObject parent)
    {
        base.Activate(parent);
        StonePrefab.GetComponent<Projectile>().playerMove = parent.GetComponent<PlayerMove>();
        StonePrefab.GetComponent<Projectile>().Speed = Speed;
        StonePrefab.GetComponent<Projectile>().etc = PingPongCount;
        PlayerMove playerMove = parent.GetComponent<PlayerMove>();
        NetworkObject Stone = playerMove.Runner.Spawn(StonePrefab, playerMove.SimpleAttackPosition.position);    
    }

}

