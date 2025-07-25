using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;

public class InspectorView : VisualElement
{
    public new class UxmlFactory : UxmlFactory<InspectorView, VisualElement.UxmlTraits> { }

    Editor editor;
    public InspectorView()
    {

    }

    internal void UpdateSelection(SkillView skillView)
    {
        Clear();

        UnityEngine.Object.DestroyImmediate(editor);

        editor = Editor.CreateEditor(skillView.skill);
        IMGUIContainer container = new IMGUIContainer(() => {
            if (editor.target)
            {
                editor.OnInspectorGUI();
            }
        });
        Add(container);
    }
}
