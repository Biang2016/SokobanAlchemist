using System;
using System.Collections.Generic;
using UnityEngine;

namespace vietlabs.fr2
{
    public class FR2_RefTreeItem<T> where T : class
    {
        public T item;
        public int level;
        public T parent;
    }

    public class FR2_RefTree<T> where T : class
    {
        internal Func<T, Rect, bool, float> BriefDrawer;
        internal FR2_TreeUI Drawer;

        internal List<T> DrawList;
        internal Func<T, List<T>> GetChildren;
        internal Func<T, Rect, bool, float> ItemDrawer;
        internal float itemHeight;
        internal Dictionary<T, FR2_RefTreeItem<T>> Map;

        internal int nLevels;
        internal List<T> Root;

        // drawer cache
        internal T selected;
        internal List<T> selectedParents;

        internal Func<T, T, int> Sorter;

        public FR2_RefTree()
        {
        }

        public FR2_RefTree(Func<T, List<T>> getChildren, Func<T, Rect, bool, float> itemDrawer)
        {
            GetChildren = getChildren;
            ItemDrawer = itemDrawer;

            Map = new Dictionary<T, FR2_RefTreeItem<T>>();
            DrawList = new List<T>();
            Drawer = new FR2_TreeUI {itemH = 18f};

            selectedParents = new List<T>();
        }

        public void Reset(List<T> rootList)
        {
            Map.Clear();

            selected = null;
            selectedParents.Clear();
            Drawer.selected = 0;

            itemHeight = 16f;
            Root = rootList;
            RefreshChildren();
            RefreshDrawList(true);
        }

        internal void RefreshDrawList(bool drawRoot, int maxLevel = -1)
        {
            DrawList.Clear();

            if (drawRoot) DrawList.AddRange(Root);
            foreach (var item in Map)
            {
                if (maxLevel == -1 || item.Value.level <= maxLevel)
                {
                    DrawList.Add(item.Key);
                }
            }

            DrawList.Sort((item1, item2) =>
            {
                FR2_RefTreeItem<T> t;
                var lv1 = 0;
                var lv2 = 0;

                if (Map.TryGetValue(item1, out t))
                {
                    lv1 = t.level;
                }

                ;
                if (Map.TryGetValue(item2, out t))
                {
                    lv2 = t.level;
                }

                ;

                if (lv1 == lv2 && Sorter != null) return Sorter(item1, item2);
                return lv1.CompareTo(lv2);
            });
        }

        private FR2_RefTreeItem<T> Add(T item, T parent, int level)
        {
            if (Map.ContainsKey(item) || Root.Contains(item)) return null;
            var result = new FR2_RefTreeItem<T>
            {
                level = level,
                item = item,
                parent = parent
            };

            Map.Add(item, result);
            return result;
        }

        private void RefreshChildren()
        {
            var last = Root;
            var level = 1;

            while (true)
            {
                List<FR2_RefTreeItem<T>> lvList;
                last = Scan(last, level, out lvList);
                if (last.Count == 0) break;

                level++;
            }

            ;
        }

        private List<T> Scan(List<T> scanList, int level, out List<FR2_RefTreeItem<T>> lvList)
        {
            lvList = new List<FR2_RefTreeItem<T>>();
            var result = new List<T>();

            for (var i = 0; i < scanList.Count; i++)
            {
                var list = GetChildren(scanList[i]);
                if (list == null) continue;

                for (var j = 0; j < list.Count; j++)
                {
                    if (result.Contains(list[j])) continue;

                    var item = Add(list[j], scanList[i], level);
                    if (item != null)
                    {
                        result.Add(item.item);
                        lvList.Add(item);
                    }
                }
            }

            return result;
        }

        public void Draw()
        {
            if (DrawList.Count == 0) return;

            if (DrawList[Drawer.selected] != selected)
            {
                //selection changed : refresh !
                var item = DrawList[Drawer.selected];
                if (item == null)
                {
                    Debug.LogWarning("Something wrong, item should not be null <" + Drawer.selected + ">" + item);
                    return;
                }

                FR2_RefTreeItem<T> pInfo;

                selected = item;
                selectedParents.Clear();

                while (Map.TryGetValue(item, out pInfo))
                {
                    item = pInfo.parent;

                    if (item != null)
                    {
                        selectedParents.Add(item);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            Drawer.Draw(DrawList.Count, (idx, rect, hasMouse) =>
            {
                var item = DrawList[idx];
                var offset = 4f;
                FR2_RefTreeItem<T> info;

                if (Map.TryGetValue(item, out info))
                {
                    offset += Map[item].level * 16f;
                }

                LeftRect(offset, ref rect);
                var highlight = info != null && selectedParents.Contains(info.item);
                ItemDrawer(DrawList[idx], rect, highlight);
            }, false);

            var bDrawer = BriefDrawer != null ? BriefDrawer : ItemDrawer;
            var count = selectedParents.Count - 1;

            for (var i = count; i >= 0; i--)
            {
                var asset = selectedParents[i];

                var r = GUILayoutUtility.GetRect(1f, Screen.width, 16f, 16f);
                //var hasMouse = Event.current.type == EventType.mouseUp && r.Contains(Event.current.mousePosition);
                LeftRect((count - i) * 16f, ref r);
                bDrawer(asset, r, false);
            }

            var r2 = GUILayoutUtility.GetRect(1f, Screen.width, 16f, 16f);
            //var hasMouse2 = Event.current.type == EventType.mouseUp && r2.Contains(Event.current.mousePosition);
            LeftRect(selectedParents.Count * 16f, ref r2);
            bDrawer(selected, r2, true);
        }

        private Rect LeftRect(float w, ref Rect rect)
        {
            rect.x += w;
            rect.width -= w;
            return new Rect(rect.x - w, rect.y, w, rect.height);
        }
    }
}