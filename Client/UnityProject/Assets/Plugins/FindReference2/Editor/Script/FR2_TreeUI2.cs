using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace vietlabs.fr2
{
    public class FR2_TreeUI2
    {
        internal Drawer drawer;
        internal Rect visibleRect;

        Vector2 position;
        TreeItem rootItem;

        public FR2_TreeUI2(Drawer drawer)
        {
            this.drawer = drawer;
        }

        public void Reset(params string[] root)
        {
            position = Vector2.zero;

            rootItem = new TreeItem()
            {
                tree = this,
                id = "$root",
                height = 0,
                depth = -1,
                _isOpen = true,
                highlight = false,
                childCount = root.Length
            };

            rootItem.RefreshChildren(root);
            rootItem.DeepOpen();
        }

        public void Draw()
        {
            var evtType = Event.current.type;
            var r = GUILayoutUtility.GetRect(1f, Screen.width, 16f, Screen.height);

            if (evtType != EventType.Layout)
            {
                visibleRect = r;
            }

            var contentRect = new Rect(0f, 0f, 1f, rootItem.childrenHeight);
            var noScroll = contentRect.height < visibleRect.height;
            if (noScroll) position = Vector2.zero;

            var minY = (int) position.y;
            var maxY = (int) (position.y + visibleRect.height);

            position = GUI.BeginScrollView(visibleRect, position, contentRect);
            {
                var rect = new Rect(0, 0, r.width - (noScroll ? 4f : 16f), 16f);
                var index = 0;
                rootItem.Draw(ref index, ref rect, minY, maxY);
            }

            GUI.EndScrollView();
        }

        // ------------------------ DELEGATE --------------

        public class Drawer
        {
            public virtual int GetHeight(string id)
            {
                return 16;
            }

            public virtual int GetChildCount(string id)
            {
                return 0;
            }

            public virtual string[] GetChildren(string id)
            {
                return null;
            }

            public virtual void Draw(Rect r, TreeItem item)
            {
                GUI.Label(r, item.id);
            }
        }

        public class GroupDrawer : Drawer
        {
            FR2_TreeUI2 tree;
            Dictionary<string, List<string>> groupDict;
            public Action<Rect, string, int> drawGroup;
            public Action<Rect, string> drawItem;

            public GroupDrawer(Action<Rect, string, int> drawGroup, Action<Rect, string> drawItem)
            {
                this.drawItem = drawItem;
                this.drawGroup = drawGroup;
            }

            // ----------------- TREE WRAPPER ------------------

            public void Reset<T>(List<T> items, Func<T, string> idFunc, Func<T, string> groupFunc, Action<List<string>> customGroupSort = null)
            {
                groupDict = new Dictionary<string, List<string>>();

                for (var i = 0; i < items.Count; i++)
                {
                    List<string> list;

                    var groupName = groupFunc(items[i]);
                    var itemId = idFunc(items[i]);

                    if (!groupDict.TryGetValue(groupName, out list))
                    {
                        list = new List<string>();
                        groupDict.Add(groupName, list);
                    }

                    list.Add(itemId);
                }

                if (tree == null) tree = new FR2_TreeUI2(this);

                var groups = groupDict.Keys.ToList();

                //if (groups.Count == 1) //single group : Flat list
                //{
                //	var v = groupDict[groups[0]];
                //	tree.Reset(v.ToArray());
                //	groupDict.Clear();
                //} else 

                {
                    //multiple groups

                    if (customGroupSort != null)
                    {
                        customGroupSort(groups);
                    }
                    else
                    {
                        groups.Sort();
                    }

                    tree.Reset(groups.ToArray());
                }
            }

            public void Draw()
            {
                if (tree != null) tree.Draw();
            }

            // ----------------- DRAWER WRAPPER ------------------

            public override int GetChildCount(string id)
            {
                List<string> group;
                if (groupDict.TryGetValue(id, out group)) return group.Count;
                return 0;
            }

            public override string[] GetChildren(string id)
            {
                List<string> group;
                if (groupDict.TryGetValue(id, out group)) return group.ToArray();
                return null;
            }

            public override void Draw(Rect r, FR2_TreeUI2.TreeItem item)
            {
                List<string> group;
                if (groupDict.TryGetValue(item.id, out group))
                {
                    drawGroup(r, item.id, item.childCount);
                    return;
                }

                drawItem(r, item.id);
            }
        }

        // ------------------------ TreeItem2 --------------

        public class TreeItem
        {
            //static Color COLOR	= new Color(0f, 0f, 0f, 0.05f);

            internal FR2_TreeUI2 tree;
            internal TreeItem parent;

            public int height;
            public int childrenHeight;

            public string id; // item id
            public int depth; // item depth
            public bool highlight;

            public int childCount;
            public List<TreeItem> children;

            internal bool _isOpen;

            public bool IsOpen
            {
                get { return _isOpen; }
                set
                {
                    if (_isOpen == value || childCount == 0) return;
                    _isOpen = value;

                    if (_isOpen)
                    {
                        if (children == null) RefreshChildren(tree.drawer.GetChildren(id));

                        //Update height for all parents
                        var p = this.parent;
                        while (p != null)
                        {
                            p.childrenHeight += childrenHeight;
                            p = p.parent;
                        }
                    }
                    else
                    {
                        //Update height for all parents
                        var p = this.parent;
                        while (p != null)
                        {
                            p.childrenHeight -= childrenHeight;
                            p = p.parent;
                        }
                    }
                }
            }

            internal void DeepOpen()
            {
                IsOpen = true;
                if (children == null) return;

                for (var i = 0; i < children.Count; i++)
                {
                    children[i].DeepOpen();
                }
            }

            internal void Draw(ref int index, ref Rect rect, int minY, int maxY)
            {
                if (height > 0 && (rect.y >= minY - height || rect.y <= maxY))
                {
                    rect.height = height;

                    if ((index % 2 == 1) && (FR2_Setting.AlternateRowColor))
                    {
                        var o = GUI.color;
                        GUI.color = FR2_Setting.RowColor;
                        GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
                        GUI.color = o;
                    }

                    var x = (depth + 1) * 16f;
                    tree.drawer.Draw(new Rect(x, rect.y, rect.width - x, rect.height), this);

                    if (childCount > 0)
                    {
                        IsOpen = GUI.Toggle(new Rect(rect.x + x - 16f, rect.y, 16f, 16f), IsOpen, string.Empty, EditorStyles.foldout);
                    }

                    index++;
                    rect.y += height;
                }

                if (_isOpen && rect.y <= maxY) //draw children
                {
                    for (var i = 0; i < children.Count; i++)
                    {
                        children[i].Draw(ref index, ref rect, minY, maxY);
                        if (rect.y > maxY) break;
                    }
                }
            }

            internal void RefreshChildren(string[] childrenIDs)
            {
                childCount = childrenIDs.Length;
                childrenHeight = 0;
                children = new List<TreeItem>();

                for (var i = 0; i < childCount; i++)
                {
                    var itemId = childrenIDs[i];

                    var item = new TreeItem()
                    {
                        tree = tree,
                        parent = this,

                        id = itemId,
                        depth = depth + 1,
                        highlight = false,

                        height = tree.drawer.GetHeight(itemId),
                        childCount = tree.drawer.GetChildCount(itemId)
                    };

                    childrenHeight += item.height;
                    children.Add(item);
                }
            }
        }
    }
}