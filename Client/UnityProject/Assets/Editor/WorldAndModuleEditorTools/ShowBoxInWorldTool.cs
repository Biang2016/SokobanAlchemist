using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

[EditorTool("取消隐藏世界中的Box")]
class ShowBoxInWorldTool : EditorTool
{
    [SerializeField]
    Texture2D m_ToolIcon;

    GUIContent m_IconContent;

    void OnEnable()
    {
        m_IconContent = new GUIContent()
        {
            image = m_ToolIcon,
            text = "取消隐藏世界中的Box",
            tooltip = "取消隐藏世界中的Box"
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
                    BoxPassiveSkill_Hide hide = null;
                    foreach (BoxPassiveSkill bf in box.RawBoxPassiveSkills)
                    {
                        if (bf is BoxPassiveSkill_Hide bfHide)
                        {
                            hide = bfHide;
                            break;
                        }
                    }

                    if (hide != null)
                    {
                        box.RawBoxPassiveSkills.Remove(hide);
                        EditorUtility.SetDirty(box);
                    }
                    else
                    {
                        continue;
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