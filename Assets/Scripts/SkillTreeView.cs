using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using System;
using UnityEditor;
using UnityEngine.UIElements;
/// <summary>
/// The Graph like view of the SkillTree used in the automatically generated User Interface
/// </summary>
public class SkillTreeView : GraphView
{
    /// <summary>
    /// needed for it to appear in the Unity UI Builder cause it is still experimental
    /// </summary>
    public new class UxmlFactory : UxmlFactory<SkillTreeView, GraphView.UxmlTraits> { }
    /// <summary>
    /// The skill Tree that this Graph visualizes
    /// </summary>
    SkillTree skillTree;
    private SkillTreeHolder skillTreeHolder;
    private List<SkillViewInteractable> skillViews = new List<SkillViewInteractable>();


    /// <summary>
    /// Creates the look and funcionality of the Skill Tree View
    /// </summary>
    public SkillTreeView()
    {
        Insert(0, new GridBackground());

        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        skillTreeHolder = GameObject.FindObjectOfType<SkillTreeHolder>();
        skillTree = skillTreeHolder.skillTree;
        if(skillTree == null)
        {
            Debug.LogWarning("No SkillTree assigned to display, please assign the desired SkillTree to display.", GameObject.FindObjectOfType<SkillTreeHolder>().gameObject);
            return;
        }
        PopulateView();
        skillTreeHolder.DelayedOnSkillPointsChanged += SkillTreeHolder_DelayedOnSkillPointsChanged;
        UpdateSkillViews();

        StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/SkillForestryStyles.uss");
        styleSheets.Add(styleSheet);
    }

    private void SkillTreeHolder_DelayedOnSkillPointsChanged(object sender, EventArgs e)
    {
        UpdateSkillViews();
    }

    SkillViewInteractable FindSkillViewInteractable(Skill skill)
    {
        return GetNodeByGuid(skill.guid) as SkillViewInteractable;
    }

    internal void PopulateView()
    {
        DeleteElements(graphElements);

        //Creates Skill View
        skillTree.skillList.ForEach(n => CreateSkillView(n));

        //Creates edges
        skillTree.skillList.ForEach(n =>
        {
            var requirements = n.skillRequirements;
            requirements.ForEach(r =>
            {
                SkillViewInteractable parentView = FindSkillViewInteractable(n);
                SkillViewInteractable requirementView = FindSkillViewInteractable(r);

                Edge edge = parentView.input.ConnectTo(requirementView.output);
                AddElement(edge);
            });
        });
    }

    

    void CreateSkillView(Skill skill)
    {       
        SkillViewInteractable skillView = new SkillViewInteractable(skill, skillTree);
        AddElement(skillView);
        skillViews.Add(skillView);
    }
    /// <summary>
    /// Reassigns the Classes for every SkillView in order to assign Styles from USS
    /// </summary>
    void UpdateSkillViews()
    {
        foreach(SkillViewInteractable skillView in skillViews)
        {
            skillView.RemoveFromClassList("unlocked");
            skillView.RemoveFromClassList("available");
            skillView.RemoveFromClassList("locked");
            skillView.RemoveFromClassList("leveling");

            if (skillTree.IsSkillUnlocked(skillView.skill))
            {
                
                if (skillTree.CanUnlockWithCost(skillView.skill))
                {
                    skillView.AddToClassList("leveling");
                }
                else
                {
                    skillView.AddToClassList("unlocked");
                }
                continue;
            }
            if(skillTree.CanUnlockWithCost(skillView.skill))
            {
                skillView.AddToClassList("available");
            }
            else
            {
                skillView.AddToClassList("locked");
            }
            
        }
    }
}
