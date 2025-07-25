using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu()]
public class SkillTree : ScriptableObject
{
    /// <summary>
    /// Event triggered by Unlocking a Skill, contains EventArgs with the unlocked skill.
    /// </summary>
    public event EventHandler<OnSkillUnlockedEventArgs> OnSkillUnlocked;
    /// <summary>
    /// Event triggered by removing an Unlocked Skill, contains EventArgs with the locked skill.
    /// </summary>
    public event EventHandler<OnSkillUnlockedEventArgs> OnSkillLocked;
    public event EventHandler OnSkillPointsChanged;
    public class OnSkillUnlockedEventArgs : EventArgs
    {
        public Skill skill;
        public int level = 0;
    }
    /// <summary>
    /// all Skills present in this SkillTree
    /// </summary>
    [HideInInspector] public List<Skill> skillList;
    private List<Skill> unlockedSkillList;
    private int attributePoints;
    private int skillPoints;
    /// <summary>
    /// How many Skillpoints have been spent in this SkillTree
    /// </summary>
    private int skillPointsSpend;
    /// <summary>
    /// Are all skills under Requirement needed to unlock skills. False if you want multiple ways through the skillTree, Example Path of Exile
    /// </summary>
    [Tooltip ("All Skill-Requirements need to be met for a skill to be unlockable")]
    private bool allSkillRequirements
    {
        get { return _allSkillRequirements; }
        set
        {           
            _allSkillRequirements = value;
            UpdateAlternativeRequirements();
        }
    }
    [SerializeField]
    [SerializeProperty("allSkillRequirements")]
    private bool _allSkillRequirements = true;


    public SkillTree()
    {
        skillList = new List<Skill>();
        unlockedSkillList = new List<Skill>();
        skillPoints = 0;
    }
    /// <summary>
    /// Unlocks the specified Skill in this SkillTree and sends an Event Notification containing the unlocked Skill
    /// </summary>
    /// <param name="skill">Skill to unlock</param>
    private void UnlockSkill(Skill skill)
    {
        if(skill is SkillBasic skillBasic)
        {
            if (!IsSkillUnlocked(skill))
            {
                unlockedSkillList.Add(skill);
                OnSkillUnlocked?.Invoke(this, new OnSkillUnlockedEventArgs
                {
                    skill = skill,
                    level = 1
                });
            }
        }

        if (skill is SkillLevel skillLevel)
        {
            skillLevel.level += 1;
            if (skillLevel.level == 1)
            {
                unlockedSkillList.Add(skillLevel);
            }
            OnSkillUnlocked?.Invoke(this, new OnSkillUnlockedEventArgs
            {
                skill = skillLevel,
                level = skillLevel.level
            });
        }
    }   

    /// <summary>
    /// Locks a Skill, without checking other skills if they still fulfill requirements. Can be directly called when Resetting the SkillTree.
    /// </summary>
    /// <param name="skill"></param>
    private void LockSkillIncomplete(Skill skill)
    {
        if (skill is SkillBasic skillBasic)
        {
            unlockedSkillList.Remove(skill);
            OnSkillLocked?.Invoke(this, new OnSkillUnlockedEventArgs { skill = skill });
        }

        if (skill is SkillLevel skillLevel)
        {
            skillLevel.level -= 1;
            if (skillLevel.level == 0)
            {
                unlockedSkillList.Remove(skillLevel);
            }

            OnSkillLocked?.Invoke(this, new OnSkillUnlockedEventArgs
            {
                skill = skillLevel,
                level = skillLevel.level
            });
        }
    }
    /// <summary>
    /// Locks a Skill and checks afterward if any Skill now no longer qualifies to be unlocked, locking it in the process.
    /// </summary>
    /// <param name="skill"></param>
    private void LockSkill(Skill skill)
    {
            LockSkillIncomplete(skill);
            CheckRequiredInvestment();
            CheckRequiredSkills();
    }
    /// <summary>
    /// Forgets a previously unlocked Skill and refunds the cost of it
    /// </summary>
    /// <param name="skill"></param>
    public void ForgetSkillRefund(Skill skill)
    {
        if(skill is SkillBasic skillBasic)
        {
            if (IsSkillUnlocked(skill))
            {
                AddSkillPoints(skill.cost[0]);
                skillPointsSpend -= skill.cost[0];
                LockSkill(skill);
            }
        }

        if(skill is SkillLevel skillLevel)
        {
            if (IsSkillUnlocked(skillLevel))
            {
                AddSkillPoints(skillLevel.cost[skillLevel.level - 1]);
                skillPointsSpend -= skillLevel.cost[skillLevel.level - 1];
                LockSkill(skillLevel);
            }
        }
    }

    public void AddSkillPoints(int skillpoints)
    {
        skillPoints += skillpoints;
        OnSkillPointsChanged?.Invoke(this, EventArgs.Empty);
    }
    /// <summary>
    /// Resets SkillTree to clean State without skills and skillpoints
    /// </summary>
    public void ResetSkillTree()
    {
        Skill[] toRemove = unlockedSkillList.ToArray();
        foreach(Skill skill in toRemove)
        {
            if(skill is SkillBasic skillBasic)
            {
                LockSkillIncomplete(skillBasic);
            }
            if (skill is SkillLevel skillLevel)
            {
                for (int i = skillLevel.level; i > 0 ; i--)
                {
                    LockSkillIncomplete(skillLevel);
                }
            }
        }
        skillPoints = 0;
        skillPointsSpend = 0;
        attributePoints = 0;
        OnSkillPointsChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Tries to unlock the specified skill consuming the required skillpoints in the process.
    /// </summary>
    /// <param name="skill"></param>
    /// <returns>true if able to Learn the skill</returns>
    public bool LearnSkill(Skill skill)
    {
        if(skill is SkillBasic skillBasic)
        {
            if (CanUnlock(skill))
            {
                if (skillPoints >= skill.cost[0])
                {
                    skillPoints -= skill.cost[0];
                    skillPointsSpend += skill.cost[0];
                    OnSkillPointsChanged?.Invoke(this, EventArgs.Empty);
                    UnlockSkill(skill);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }


        if(skill is SkillLevel skillLevel)
        {
            if (CanUnlock(skillLevel))
            {

                if (skillPoints >= skillLevel.cost[skillLevel.level])
                {
                    skillPoints -= skillLevel.cost[skillLevel.level];
                    skillPointsSpend += skillLevel.cost[skillLevel.level];
                    OnSkillPointsChanged?.Invoke(this, EventArgs.Empty);

                    UnlockSkill(skillLevel);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        return false;
    }   

    /// <summary>
    /// Checks if the skill requirements of the specified skill are met.
    /// </summary>
    /// <param name="skill"></param>
    /// <returns></returns>
    public bool CanUnlock(Skill skill)
    {
        if (skill.requiredInvestment > skillPointsSpend) return false;
        if(skill is SkillLevel lvl)
        {
            if (lvl.maxLevel == lvl.level) return false;
            if (skill.requiredAttributePoints[lvl.level] > attributePoints) return false;
        }
        else
        {
            if (skill.requiredAttributePoints[0] > attributePoints) return false;
        }

        if (IsSkillUnlocked(skill))
        {
            if(skill is SkillLevel skillLevel)
            {                
                if(skillLevel.maxLevel == skillLevel.level)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        return RequirementCheck(skill);       
    }
    /// <summary>
    /// Checks if all Requirements of the skill are met
    /// </summary>
    /// <param name="skill"></param>
    /// <returns>true if all requirements of the skill are met</returns>
    private bool RequirementCheck(Skill skill)
    {
        if (skill.skillRequirements.Count == 0)
        {
            return true;
        }
        if (allSkillRequirements)
        {
            List<Skill> requirement = skill.skillRequirements;
            foreach (Skill s in requirement)
            {
                if (!unlockedSkillList.Contains(s))
                {
                    return false;
                }
            }
            return true;
        }
        else
        {
            if (skill.skillRequirements.Intersect(unlockedSkillList).Any()||skill.alternateSkillRequirements.Intersect(unlockedSkillList).Any())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Checks if any of the unlocked skills now fails its required investment, and then locks them if that is the case.
    /// </summary>
    private void CheckRequiredInvestment()
    {
        Skill[] skillsToCheck = unlockedSkillList.ToArray();
        foreach (Skill skill in skillsToCheck)
        {

            if (skill is SkillBasic skillBasic)
            {
                if ((skillBasic.requiredInvestment + skillBasic.cost[0]) > skillPointsSpend || skillBasic.requiredAttributePoints[0] > attributePoints)
                {
                    ForgetSkillRefund(skillBasic);
                }
            }
            if (skill is SkillLevel skillLevel)
            {
                if ((skillLevel.requiredInvestment + skillLevel.cost[skillLevel.level - 1]) > skillPointsSpend || skillLevel.requiredAttributePoints[skillLevel.level -1] > attributePoints)
                {
                    ForgetSkillRefund(skillLevel);
                }
            }
        }
    }

    /// <summary>
    /// Checks if any of the unlocked skills no longer have their required skills unlocked, and locks them if that is the case.
    /// </summary>
    private void CheckRequiredSkills()
    {
        Skill[] skillsToCheck = unlockedSkillList.ToArray();
        foreach (Skill skill in skillsToCheck)
        {
            if (!RequirementCheck(skill))
            {
                ForgetSkillRefund(skill);
            }
        }
    }

    /// <summary>
    /// CanUnlock() with the added benefit of checking if enough skillpoints are available
    /// </summary>
    /// <param name="skill"></param>
    /// <returns></returns>
    public bool CanUnlockWithCost(Skill skill)
    {
        if (skill is SkillBasic skillBasic)
        {
            if (skillPoints >= skillBasic.cost[0])
            {
                return CanUnlock(skill);
            }
            else
            {
                return false;
            }
        }

        if (skill is SkillLevel skillLevel)
        {
            if (skillLevel.level == skillLevel.maxLevel) return false;
            if (skillPoints >= skillLevel.cost[skillLevel.level])
            {
                return CanUnlock(skill);
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    public int GetSkillPoints()
    {
        return skillPoints;
    }
    public int GetSpendSkillPoints()
    {
        return skillPointsSpend;
    }
    public bool IsSkillUnlocked(Skill skill)
    {
        return unlockedSkillList.Contains(skill);
    }

    public List<Skill> GetSkillRequirements(Skill skill)
    {
        return skill.skillRequirements;
    }
    public int GetAttributePoints()
    {
        return attributePoints;
    }
    public void AddAttributePoints(int points)
    {
        attributePoints += points;
        OnSkillPointsChanged?.Invoke(this, EventArgs.Empty);
    }
    public void SetAttributePoints(int points)
    {
        attributePoints = points;
    }
    /// <summary>
    /// Returns current mode of the Skill Tree, are all SkillRequirements Needed?
    /// </summary>
    /// <returns>True if all skill requirements need to be met</returns>
    public bool AllSkillRequirementsNeeded()
    {
        return allSkillRequirements;
    }

    public SkillTree Clone()
    {
        SkillTree tree = Instantiate(this);
        tree.skillList = skillList.ConvertAll(s => s.Clone());
        tree.unlockedSkillList = unlockedSkillList.ConvertAll(s => s.Clone());
        return tree;
    }

    #region Scriptable Object & UI
    #if UNITY_EDITOR
    /// <summary>
    /// Creates a new Skill Scriptable Object and ads it to the current SkillTree as a sub asset
    /// </summary>
    /// <param name="type">a skilltype, must inherit from skill</param>
    /// <returns></returns>
    public Skill CreateSkill(System.Type type)
    {
        Skill skill = ScriptableObject.CreateInstance(type) as Skill;
        skill.name = type.Name;
        if(skill is SkillBasic)
        {
            skill.skillName = "Basic Skill";
        }else if(skill is SkillLevel)
        {
            skill.skillName = "Skill with Level";
        }
        skill.guid = GUID.Generate().ToString();

        Undo.RecordObject(this, "Skill Tree (Create Skill)");
        skillList.Add(skill);

        AssetDatabase.AddObjectToAsset(skill, this);
        Undo.RegisterCreatedObjectUndo(skill, "Skill Tree (Create Skill)");

        AssetDatabase.SaveAssets();
        return skill;
    }
    /// <summary>
    /// Deletes specified skill from the SkillTree AND Asset folder
    /// </summary>
    /// <param name="skill"></param>
    public void DeleteSkill(Skill skill)
    {
        //Does currently not add the Scriptable Object back to the Main Asset when Undoing the delete action
        //Undo.RecordObject(this, "Skill Tree (Delete Skill)");
        skillList.Remove(skill);

        AssetDatabase.RemoveObjectFromAsset(skill);
        //Undo.DestroyObjectImmediate(skill);
        AssetDatabase.SaveAssets();
    }
    /// <summary>
    /// Adds the requirement skill as a Required Skill on the parent
    /// </summary>
    /// <param name="parent">gets new Requirement</param>
    /// <param name="requirement">requirement of parent</param>
    public void AddRequirement(Skill parent, Skill requirement)
    {
        Undo.RecordObject(parent, "Skill Tree (Add Requirement)");
        parent.skillRequirements.Add(requirement);
        EditorUtility.SetDirty(parent);
    }

    /// <summary>
    /// Removes the requirement skill as a Required Skill from the parent
    /// </summary>
    /// <param name="parent">loses Requirement</param>
    /// <param name="requirement">requirement of parent to remove</param>
    public void RemoveRequirement(Skill parent, Skill requirement)
    {
        Undo.RecordObject(parent, "Skill Tree (Remove Requirement)");
        parent.skillRequirements.Remove(requirement);
        EditorUtility.SetDirty(parent);
    }
    /// <summary>
    /// Adds the alternative Requirement to the child Skill
    /// </summary>
    /// <param name="child">gets new alternative Requirement</param>
    /// <param name="altRequirement">alternative Requirement of child</param>
    public void AddAlternativeRequirement(Skill child, Skill altRequirement)
    {
        child.alternateSkillRequirements.Add(altRequirement);
    }
    /// <summary>
    /// Removes the alternative Requirement of the child Skill
    /// </summary>
    /// <param name="child">loses alternative Requirement</param>
    /// <param name="altRequirement">alternative Requirement of child to remove</param>
    public void RemoveAlternativeRequirement(Skill child, Skill altRequirement)
    {
        child.alternateSkillRequirements.Remove(altRequirement);
    }
    /// <summary>
    /// Removes or Adds the alternative Skill Requirement to all Skills in the SkillTree depending on the Setting of - bool: allSkillRequirements
    /// </summary>
    private void UpdateAlternativeRequirements()
    {
        if (allSkillRequirements)
        {
            //remove alternative
            foreach (Skill skill in skillList)
            {
                skill.alternateSkillRequirements.Clear();
            }
        }
        else
        {
            //add alternative
            foreach(Skill skill in skillList)
            {
                Skill[] tmp = skill.skillRequirements.ToArray();
                foreach (Skill requirement in tmp)
                {
                    if(!requirement.alternateSkillRequirements.Contains(skill))
                    requirement.alternateSkillRequirements.Add(skill);
                }
            }
        }
    }
    #endif
#endregion
}
