using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ActiveSkill : MonoBehaviour
{
    //public SkillManager Skillmanager;
    public Skill[] playerSkills;
    public bool[] CanSkill;
    public int SkillIndex;

    void Start()
    {
        //Skillmanager = FindObjectOfType<SkillManager>();
        Debug.Log(playerSkills.Length);
        playerSkills = SkillManager.instance.GetRandomSkills(2);
        for(int i = 0; i < playerSkills.Length; i++)
        {
            CanSkill[i] = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(2) && playerSkills[SkillIndex] != null && CanSkill[SkillIndex])
        {
            StartCoroutine(Active(SkillIndex));
        }

        ChangeSkill();
    }

    public IEnumerator Active(int SkillNum)
    {
        playerSkills[SkillNum].Activate(transform.gameObject);
        CanSkill[SkillNum] = false;
        yield return new WaitForSeconds(playerSkills[SkillNum].SkillCool);
        CanSkill[SkillNum] = true;
    }

    public void ChangeSkill()
    {
        float wheelInput = Input.GetAxis("Mouse ScrollWheel");
        if (wheelInput > 0)
        {
            if (SkillIndex >= 1)
            {
                SkillIndex = 0;
            }
            else
            {
                SkillIndex++;
            }
        }
        else if (wheelInput < 0)
        {
            if (SkillIndex <= 0)
            {
                SkillIndex = 1;
            }
            else
            {
                SkillIndex--;
            }
        }
    }
}
