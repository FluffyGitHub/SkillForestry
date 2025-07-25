using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;

/// <summary>
/// Editor Window used to Display the SkillTree Editor as Part of the SkillForestry Package
/// </summary>
public class SkillForestryWindow : EditorWindow
{
    /// <summary>
    /// used inside this SkillForestryWindow
    /// </summary>
    SkillForestryView skillForestryView;
    /// <summary>
    /// used inside this SkillForestryWindow
    /// </summary>
    InspectorView inspectorView;

    /// <summary>
    /// Opens the EditorWindow also known as "Skill Forestry Window"
    /// </summary>
    [MenuItem("Tools/Skill Forestry")]
    public static void ShowWindow()
    {
        var window = GetWindow<SkillForestryWindow>();
        window.titleContent = new GUIContent("SkillForestry Editor");
        window.minSize = new Vector2(800,600);
    }

    /// <summary>
    /// Opens the "Skill Forestry Window" when double clicking a Skill Tree
    /// </summary>
    /// <param name="instanceId">unused</param>
    /// <param name="line">unused</param>
    /// <returns></returns>
    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceId, int line)
    {
        if(Selection.activeObject is SkillTree)
        {
            ShowWindow();
            return true;
        }
        return false;
    }

    public void CreateGUI()
    {
        VisualTreeAsset original = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/SkillForestryWindow.uxml");
        original.CloneTree(rootVisualElement);

        StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/SkillForestryStyles.uss");
        rootVisualElement.styleSheets.Add(styleSheet);

        skillForestryView = rootVisualElement.Q<SkillForestryView>();
        inspectorView = rootVisualElement.Q<InspectorView>();
        skillForestryView.OnSkillSelected = OnSkillSelectionChanged;
        OnSelectionChange();
    }
    /// <summary>
    /// Updates the Skill Forestry Window when a new SkillTree is selected
    /// </summary>
    private void OnSelectionChange()
    {
        SkillTree skilltree = Selection.activeObject as SkillTree;
        if (skilltree)
        {
            skillForestryView.PopulateView(skilltree);
        }
    }
    /// <summary>
    /// Updates the Inspectorview if another skill is selected
    /// </summary>
    /// <param name="skillView">the skill that is being selected to view</param>
    void OnSkillSelectionChanged(SkillView skillView)
    {
        inspectorView.UpdateSelection(skillView);
    }
}
