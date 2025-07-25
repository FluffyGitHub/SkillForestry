
public class SkillBasic : Skill
{
    public override Skill Clone()
    {
        SkillBasic skill = Instantiate(this);
        skill.skillRequirements = skillRequirements.ConvertAll(s => s.Clone());
        return skill;
    }
}
