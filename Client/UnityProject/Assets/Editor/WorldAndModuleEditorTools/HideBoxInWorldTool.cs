using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

[EditorTool("隐藏世界中的Box")]
class HideBoxInWorldTool : EditorTool
{
    [SerializeField]
    Texture2D m_ToolIcon;

    GUIContent m_IconContent;

    void OnEnable()
    {
        m_IconContent = new GUIContent()
        {
            image = m_ToolIcon,
            text = "隐藏世界中的Box",
            tooltip = "隐藏世界中的Box"
        };
    }

    public override GUIContent toolbarIcon
    {
        get { return m_IconContent; }
    }

    public override void OnToolGUI(EditorWindow window)
    {
        foreach (Transform transform in Selection.transforms)
        {
            Box box = transform.GetComponentInParent<Box>();
            if (box)
            {
                WorldDesignHelper world = box.GetComponentInParent<WorldDesignHelper>();
                WorldModuleDesignHelper module = box.GetComponentInParent<WorldModuleDesignHelper>();
                if (world && module)
                {
                    bool hasHide = false;
                    foreach (BoxPassiveSkill bf in box.RawBoxPassiveSkills)
                    {
                        if (bf is BoxPassiveSkill_Hide)
                        {
                            hasHide = true;
                            break;
                        }
                    }

                    if (hasHide)
                    {
                        continue;
                    }
                    else
                    {
                        BoxPassiveSkill_Hide hide = new BoxPassiveSkill_Hide();
                        hide.SpecialCaseType = BoxPassiveSkill.BoxPassiveSkillBaseSpecialCaseType.World;
                        box.RawBoxPassiveSkills.Add(hide);
                        EditorUtility.SetDirty(box);
                    }
                }
                else
                {
                    Debug.LogWarning("此工具仅针对世界编辑器下的模组内的Box");
                }
            }
        }
    }
}