using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using System.Linq;
using System;

/// <summary>
/// The Graph like view of the SkillTree used in the Editor Window
/// </summary>
public class SkillForestryView : GraphView
{
    public Action<SkillView> OnSkillSelected;
    /// <summary>
    /// needed for it to appear in the Unity UI Builder cause it is still experimental
    /// </summary>
    public new class UxmlFactory : UxmlFactory<SkillForestryView, GraphView.UxmlTraits> { }
    /// <summary>
    /// The skill Tree that this Graph currently visualizes
    /// </summary>
    SkillTree skillTree;
    /// <summary>
    /// Creates the look and funcionality of the Skill Forest View
    /// </summary>
    public SkillForestryView()
    {
        Insert(0, new GridBackground());

        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/SkillForestryStyles.uss");
        styleSheets.Add(styleSheet);

        Undo.undoRedoPerformed += OnUndoRedo;
    }

    private void OnUndoRedo()
    {
        PopulateView(skillTree);
        AssetDatabase.SaveAssets();
    }

    SkillView FindSkillView(Skill skill)
    {
        return GetNodeByGuid(skill.guid) as SkillView;
    }

    internal void PopulateView(SkillTree skillTree)
    {
        this.skillTree = skillTree;

        graphViewChanged -= OnGraphViewChanged;
        DeleteElements(graphElements);
        graphViewChanged += OnGraphViewChanged;

        //Creates Skill View
        skillTree.skillList.ForEach(n => CreateSkillView(n));

        //Creates edges
        skillTree.skillList.ForEach(n =>
        {
            var requirements = n.skillRequirements;
            requirements.ForEach(r =>
            {
                SkillView parentView = FindSkillView(n);
                SkillView requirementView = FindSkillView(r);

                Edge edge = parentView.input.ConnectTo(requirementView.output);
                AddElement(edge);
            });
        });

        String pathtoAsset = AssetDatabase.GetAssetPath(skillTree);
        Object[] data = AssetDatabase.LoadAllAssetsAtPath(pathtoAsset);

        if (data == null) return;
        foreach (Object o in data)
        {
            String skillName;
            if(o is Skill skill)
            {
                skillName = skill.skillName;
                SerializedObject serialized = new SerializedObject(skill);
                SerializedProperty objName = serialized.FindProperty("m_Name");
                if(objName.stringValue == skillName)
                {
                    continue;
                }
                objName.stringValue = skillName;
                serialized.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(serialized.targetObject), ImportAssetOptions.ForceUpdate);
            }
        }
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(endPort =>
        endPort.direction != startPort.direction &&
        endPort.node != startPort.node).ToList();
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {
        if(graphViewChange.elementsToRemove != null)
        {
            graphViewChange.elementsToRemove.ForEach(elem =>
            {
                SkillView skillView = elem as SkillView;
                if (skillView != null)
                {
                    skillTree.DeleteSkill(skillView.skill);
                }

                Edge edge = elem as Edge;
                if (edge != null)
                {
                    SkillView parentView = edge.input.node as SkillView;
                    SkillView requirementView = edge.output.node as SkillView;
                    skillTree.RemoveRequirement(parentView.skill, requirementView.skill);
                    if (!skillTree.AllSkillRequirementsNeeded())
                    {
                        skillTree.RemoveAlternativeRequirement(requirementView.skill, parentView.skill);
                    }
                }
            });
        }

        if(graphViewChange.edgesToCreate != null)
        {
            graphViewChange.edgesToCreate.ForEach(edge =>
            {
                SkillView parentView = edge.input.node as SkillView;
                SkillView requirementView = edge.output.node as SkillView;
                skillTree.AddRequirement(parentView.skill, requirementView.skill);
                if (!skillTree.AllSkillRequirementsNeeded())
                {
                    skillTree.AddAlternativeRequirement(requirementView.skill, parentView.skill);
                }
            });
        }
        return graphViewChange;
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        {
            var types = TypeCache.GetTypesDerivedFrom<Skill>();
            foreach(var type in types)
            {
                evt.menu.AppendAction($"{type.Name}", (a) => CreateSkill(type));
            }
        }
    }

    void CreateSkill(System.Type type)
    {
        Skill skill = skillTree.CreateSkill(type);
        CreateSkillView(skill);
    }

    void CreateSkillView(Skill skill)
    {
        SkillView skillView = new SkillView(skill);
        skillView.OnSkillSelected = OnSkillSelected;
        AddElement(skillView);

    }
}
