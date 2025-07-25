using UnityEngine;

public class SkillLevel : Skill
{
    [HideInInspector] public int level = 0;
    public int maxLevel
    {
        get { return _maxLevel; }
        //reassigns the Array for Costs to the appropriate size of the new Max Level and populates it with the previously assigned costs
        set {
            if(value < 0)
            {
                return;
            }
            if( _maxLevel > value)
            {
                int[] temp = new int[value];
                int[] temp2 = new int[value];
                for(int i = 0; i < value; i++)
                {
                    temp[i] = cost[i];
                    temp2[i] = requiredAttributePoints[i];
                }
                cost = temp;
                requiredAttributePoints = temp2;
            }else if (_maxLevel < value)
            {
                int[] temp = new int[value];
                int[] temp2 = new int[value];
                for (int i = 0; i < _maxLevel; i++)
                {
                    temp[i] = cost[i];
                    temp2[i] = requiredAttributePoints[i];
                }
                cost = temp;
                requiredAttributePoints = temp2;
            }
            _maxLevel = value; 
        }
    }
    /// <summary>
    /// Do NOT access via Code, serializes Property for the Inspector. Target "maxLevel" with code.
    /// </summary>
    [SerializeProperty("maxLevel")]
    public int _maxLevel = 1;

    public override Skill Clone()
    {
        SkillLevel skill = Instantiate(this);
        skill.skillRequirements = skillRequirements.ConvertAll(s => s.Clone());
        return skill;
    }
}
