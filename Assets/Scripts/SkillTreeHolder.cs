using System;
using System.Collections;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class SkillTreeHolder : MonoBehaviour
{
    public SkillTree skillTree;
    private UIDocument uiDocument;
    private Button gainBttn;
    private Button resetBttn;
    private Button attributeBttn;
    private IntegerField skillPoints;
    private Label currentSkillPoints;
    private Label skillPointsSpend;
    private IntegerField attributePoints;
    private Label currentAttributePoints;


    public event EventHandler DelayedOnSkillPointsChanged;

    private void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) return;
        gainBttn = uiDocument.rootVisualElement.Q<Button>("bttn_SkillPointGain");
        attributeBttn = uiDocument.rootVisualElement.Q<Button>("bttn_AttributeGain");
        resetBttn = uiDocument.rootVisualElement.Q<Button>("bttn_Reset");
        skillPoints = uiDocument.rootVisualElement.Q<IntegerField>("pointsToGain");
        attributePoints = uiDocument.rootVisualElement.Q<IntegerField>("attributeToGain");
        currentSkillPoints = uiDocument.rootVisualElement.Q<Label>("skillPoints");
        currentAttributePoints = uiDocument.rootVisualElement.Q<Label>("attribute");
        skillPointsSpend = uiDocument.rootVisualElement.Q<Label>("skillPointsSpend");

        skillTree.OnSkillPointsChanged += SkillTree_OnSkillPointsChanged; 
        currentSkillPoints.text = skillTree.GetSkillPoints().ToString();
        currentAttributePoints.text = skillTree.GetAttributePoints().ToString();
        skillPointsSpend.text = skillTree.GetSpendSkillPoints().ToString();

        if (gainBttn != null)
        {
            gainBttn.clickable.clicked += () =>
            {
                GainButton();
            };
        }

        if (resetBttn != null)
        {
            resetBttn.clickable.clicked += () =>
            {
                ResetButton();
            };
        }

        if (attributeBttn != null)
        {
            attributeBttn.clickable.clicked += () =>
            {
                AttributeGainButton();
            };
        }

    }

    private void SkillTree_OnSkillPointsChanged(object sender, EventArgs e)
    {
        currentSkillPoints.text = skillTree.GetSkillPoints().ToString();
        currentAttributePoints.text = skillTree.GetAttributePoints().ToString();
        StartCoroutine(DelayedSkillTreeOnSkillPointsChanged());
    }
    /// <summary>
    /// an Event for SkillTree_OnSkillPointsChanged that is delayed by 1 Frame, needed in Automatically generated UI
    /// </summary>
    /// <returns></returns>
    IEnumerator DelayedSkillTreeOnSkillPointsChanged()
    {
        yield return 0;
        skillPointsSpend.text = skillTree.GetSpendSkillPoints().ToString();
        DelayedOnSkillPointsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void GainButton()
    {
        skillTree.AddSkillPoints(skillPoints.value);
    }

    private void ResetButton()
    {
        skillTree.ResetSkillTree();
    }
    private void AttributeGainButton()
    {
        skillTree.AddAttributePoints(attributePoints.value);
    }

}
