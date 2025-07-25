using UnityEngine;

public class Demonstration : MonoBehaviour
{
    [SerializeField] private SkillTree tree;
    [SerializeField] private Skill skillToLearn;
    [SerializeField] private Skill SkillLvlToLearn;

    private int armorBonus = 0;
    private void OnEnable()
    {
        if(tree != null)
        {
            tree.OnSkillUnlocked += Tree_OnSkillUnlocked;
            tree.OnSkillLocked += Tree_OnSkillLocked;
        }
    }

    private void Tree_OnSkillLocked(object sender, SkillTree.OnSkillUnlockedEventArgs e)
    {
        if(e.skill == skillToLearn)
        {
            SkillToLearnLocked();
        }
        if (e.skill == SkillLvlToLearn)
        {
            Debug.Log(SkillLvlToLearn.skillName + " has reached Level " + e.level + "!");
            switch (e.level)
            {
                case 0:
                    armorBonus = 0;
                    break;
                case 1:
                    armorBonus = 20;
                    break;
                case 2:
                    armorBonus = 40;
                    break;
                case 3:
                    armorBonus = 60;
                    break;
                case 4:
                    armorBonus = 80;
                    break;
            }
        }
    }

    private void Tree_OnSkillUnlocked(object sender, SkillTree.OnSkillUnlockedEventArgs e)
    {
        if (e.skill == skillToLearn)
        {
            SkillToLearnUnlocked();
            Debug.Log("Skill " + e.skill.name + " has been unlocked!");
        }
        if (e.skill == SkillLvlToLearn)
        {
            Debug.Log(SkillLvlToLearn.skillName + " has reached Level " + e.level + "!");
            switch (e.level)
            {
                case 1:
                    armorBonus = 20;
                    break;
                case 2:
                    armorBonus = 40;
                    break;
                case 3:
                    armorBonus = 60;
                    break;
                case 4:
                    armorBonus = 80;
                    break;
            }
        }
    }
    private void SkillToLearnUnlocked()
    {
        // Executing Code that needs to be executed after the Skill got unlocked
    }
    private void SkillToLearnLocked()
    {
        // Executing Code that needs to be executed after the Skill was locked/forgotten
    }

    public string ImUsingTheVariableInCodeSoItDoesNotThrowAWarning()
    {
        return armorBonus.ToString();
    }
}
