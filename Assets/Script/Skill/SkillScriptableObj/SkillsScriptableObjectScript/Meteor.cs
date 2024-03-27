using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "Skills/ActiveSkill/Meteor")]
public class Meteor : Skill
{
    public int Damage;
    public GameObject MeteorPrefab;



    public override void Activate(GameObject parent)
    {
        base.Activate(parent);
    }
}
