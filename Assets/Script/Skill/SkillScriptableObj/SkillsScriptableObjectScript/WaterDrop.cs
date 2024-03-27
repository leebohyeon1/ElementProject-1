using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Skill", menuName = "Skills/ActiveSkill/WaterDrop")]
public class WaterDrop : Skill
{
    public float Duration;
    public int GuardAmount;
    public override void Activate(GameObject parent)
    {
        base.Activate(parent);
        PlayerStats stats = parent.GetComponent<PlayerStats>();
        stats.GuardAmount = GuardAmount;
        stats.GuardDuration = Duration;
        stats.isGuardSkill = true;
    }

}
