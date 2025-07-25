using UnityEngine;
using UnityEditor.Experimental.GraphView;
using System;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
/// <summary>
/// Representation of Skills in the Graphview of SkillForestryWindow
/// </summary>
public class SkillView : Node
{
    public Action<SkillView> OnSkillSelected;
    public Skill skill;
    public Port input;
    public Port output;

    public SkillView(Skill skill) : base("Assets/Editor/SkillView.uxml")
    {
        this.skill = skill;
        this.title = skill.name;
        this.viewDataKey = skill.guid;

        style.left = skill.position.x;
        style.top = skill.position.y;

        CreateInputPorts();
        CreateOutputPorts();
        SetupClasses();

        SerializedObject serializedSkill = new SerializedObject(skill);
        //String = Name of the Label in UXML
        Label descriptionLabel = this.Q<Label>("description");
        //String = Name of the Variable in Skill
        descriptionLabel.bindingPath = "description";
        descriptionLabel.Bind(serializedSkill);

        Label costLabel = this.Q<Label>("cost");
        costLabel.bindingPath = "cost.Array.data[0]";
        costLabel.Bind(serializedSkill);

        Label attributeLabel = this.Q<Label>("attribute");
        attributeLabel.bindingPath = "requiredAttributePoints.Array.data[0]";
        attributeLabel.Bind(serializedSkill);

        Label titleLabel = this.Q<Label>("title-label");
        titleLabel.bindingPath = "skillName";
        titleLabel.Bind(serializedSkill);

        if(skill is SkillLevel)
        {
            Label levelLabel = this.Q<Label>("level");
            levelLabel.bindingPath = "level";
            levelLabel.Bind(serializedSkill);

            Label maxLevelLabel = this.Q<Label>("maxLevel");
            maxLevelLabel.bindingPath = "_maxLevel";
            maxLevelLabel.Bind(serializedSkill);
        }
    }
    /// <summary>
    /// Sets up Classes to use in the USS for styling
    /// </summary>
    private void SetupClasses()
    {
        if(skill is SkillBasic)
        {
            AddToClassList("basic");
        }else if(skill is SkillLevel)
        {
            AddToClassList("level");
        }
    }

    private void CreateOutputPorts()
    {
        output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
        if (output != null)
        {
            output.portName = "";
            output.style.flexDirection = FlexDirection.ColumnReverse;
            outputContainer.Add(output);
        }
    }

    private void CreateInputPorts()
    {
        input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(bool));
        if (input != null)
        {
            input.portName = "";
            input.style.flexDirection = FlexDirection.Column;
            inputContainer.Add(input);
        }
    }

    public override void SetPosition(Rect newPos)
    {
        base.SetPosition(newPos);
        Undo.RecordObject(skill, "Skill Tree (Set Position)");
        skill.position.x = newPos.xMin;
        skill.position.y = newPos.yMin;
        EditorUtility.SetDirty(skill);
    }

    public override void OnSelected()
    {
        base.OnSelected();
        if(OnSkillSelected != null)
        {
            OnSkillSelected.Invoke(this);
        }
    }
}
