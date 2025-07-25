using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

/// <summary>
/// Representation of Skills in the Graphview of SkillTreeWindow, aka the automatically generated User Interface
/// </summary>
public class SkillViewInteractable : Node
{
    public Skill skill;
    public SkillTree skillTree;
    public Port input;
    public Port output;
    private Label costLabel;
    private Label attributeLabel;

    public SkillViewInteractable(Skill skill, SkillTree skillTree) : base("Assets/Editor/SkillViewInteractable.uxml")
    {
        this.skillTree = skillTree;
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
        descriptionLabel.text = skill.description;

        costLabel = this.Q<Label>("cost");
        if(skill is SkillBasic)
        {
            costLabel.bindingPath = "cost.Array.data[0]";
            costLabel.Bind(serializedSkill);
        }
        if(skill is SkillLevel level)
        {
            if(level.level == level.maxLevel)
            {
                costLabel.text = "max";
            }
            else
            {
                costLabel.text = skill.cost[level.level].ToString();
            }          
        }
        

        Label investmentLabel = this.Q<Label>("investment");
        investmentLabel.text = skill.requiredInvestment.ToString();
        attributeLabel = this.Q<Label>("requiredAttribute");
        if (skill is SkillBasic)
        {
            attributeLabel.bindingPath = "requiredAttributePoints.Array.data[0]";
            attributeLabel.Bind(serializedSkill);
        }
        if (skill is SkillLevel lvl)
        {
            if (lvl.level == lvl.maxLevel)
            {
                attributeLabel.text = "max";
            }
            else
            {
                attributeLabel.text = skill.requiredAttributePoints[lvl.level].ToString();
            }
        }

        Label titleLabel = this.Q<Label>("title-label");
        titleLabel.text = skill.skillName;

        if (skill is SkillLevel)
        {
            Label levelLabel = this.Q<Label>("level");
            levelLabel.bindingPath = "level";
            levelLabel.Bind(serializedSkill);

            Label maxLevelLabel = this.Q<Label>("maxLevel");
            maxLevelLabel.bindingPath = "_maxLevel";
            maxLevelLabel.Bind(serializedSkill);
        }

        Button learnButton = this.Q<Button>("learnButton");
        if(learnButton != null)
        {
            learnButton.clickable.clicked += () =>
            {
                LearnButton();
            };
        }

        Button forgetButton = this.Q<Button>("forgetButton");
        if (forgetButton != null)
        {
            forgetButton.clickable.clicked += () =>
            {
                ForgetButton();
            };
        }
    }
    /// <summary>
    /// Sets up Classes to use in the USS for styling
    /// </summary>
    private void SetupClasses()
    {
        if (skill is SkillBasic)
        {
            AddToClassList("basic");
        }
        else if (skill is SkillLevel)
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
        skill.position.x = newPos.xMin;
        skill.position.y = newPos.yMin;
    }
    /// <summary>
    /// Function called when the Learn Button is pressed
    /// </summary>
    private void LearnButton()
    {
        if(skill is SkillBasic)
        {
            skillTree.LearnSkill(skill as SkillBasic);
        }else if(skill is SkillLevel lvl)
        {
            if(skillTree.LearnSkill(lvl))
            {
                if (lvl.level == lvl.maxLevel)
                {
                    costLabel.text = "max";
                    attributeLabel.text = "max";
                }
                else
                {
                    costLabel.text = skill.cost[lvl.level].ToString();
                    attributeLabel.text = skill.requiredAttributePoints[lvl.level].ToString();
                }
            }
        }

    }
    /// <summary>
    /// Function called when the Forget Button is pressed
    /// </summary>
    private void ForgetButton()
    {      
        if (skill is SkillBasic)
        {
            skillTree.ForgetSkillRefund(skill as SkillBasic);
        }
        else if (skill is SkillLevel lvl)
        {
            skillTree.ForgetSkillRefund(lvl);
            costLabel.text = skill.cost[lvl.level].ToString();
            attributeLabel.text = skill.requiredAttributePoints[lvl.level].ToString();
        }
    }
}

