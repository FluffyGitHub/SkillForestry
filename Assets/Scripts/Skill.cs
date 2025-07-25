using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Class holding all Information for an Instance of a Skill, to be used in the SkillTree
/// </summary>
public abstract class Skill : ScriptableObject, IEquatable<Skill>
{
    public String skillName;
    /// <summary>
    /// An Array containing all the costs for different Levels of this skill, if the Skill does not have a Level the array has the size 1.
    /// </summary>
    public int[] cost = {1};
    /// <summary>
    /// How many SkillPoints already need to be invested in the SkillTree, to unlock this skill
    /// </summary>
    [Tooltip("How many Skillpoints already need to be invested in the Skill Tree, to unlock this skill")]public int requiredInvestment = 0;
    /// <summary>
    /// How high the attribute stat needs to be to unlock this skill
    /// </summary>
    [Tooltip("How high the attribute stat needs to be to unlock this skill")]public int[] requiredAttributePoints = { 0 };
    [TextArea] public String description = "Describe me";
    /// <summary>
    /// List containing all skills that are required to be unlocked before unlocking this skill
    /// </summary>
    [HideInInspector] public List<Skill> skillRequirements = new List<Skill>();
    /// <summary>
    /// List containing skills that this skill is a requirement for, usefull when allowing multidirectional paths through the skill Tree via SkillTree: AllSkillRequirements = false.
    /// </summary>
    [HideInInspector] public List<Skill> alternateSkillRequirements = new List<Skill>();
    /// <summary>
    /// used in the SkillForestryUI
    /// </summary>
    [HideInInspector] public String guid;
    /// <summary>
    /// used for position in the SkillForestryUI
    /// </summary>
    [HideInInspector] public Vector2 position;

    public virtual Skill Clone()
    {
        return Instantiate(this);
    }
    #region IEquatable
    public bool Equals(Skill other)
    {
        if (other == null) return false;

        return this.skillName == other.skillName;
    }

    public override bool Equals(object obj) => Equals(obj as Skill);
    public override int GetHashCode() => skillName.GetHashCode();
    #endregion
}
