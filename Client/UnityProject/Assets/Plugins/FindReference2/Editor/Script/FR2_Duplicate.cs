//#define FR2_DEBUG

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using CBParams = System.Collections.Generic.List<System.Collections.Generic.List<string>>;
using Object = UnityEngine.Object;

namespace vietlabs.fr2
{
    internal class FR2_DuplicateAsset : FR2_TreeItemUI
    {
        public FR2_Asset asset;

        public FR2_DuplicateAsset(FR2_Asset asset)
        {
            this.asset = asset;
        }

        protected override void Draw(Rect r)
        {
            var drawR = r;
            drawR.xMin -= 16f;
            asset.Draw(drawR, false, false);

            var bRect = r;
            bRect.xMin += bRect.width - 50f;
            if (GUI.Button(bRect, "Use", EditorStyles.miniButton))
            {
                EditorGUIUtility.systemCopyBuffer = asset.guid;
                Selection.objects = (parent as FR2_DuplicateFolder).children.Select(
                    a => FR2_Unity.LoadAssetAtPath<Object>(((FR2_DuplicateAsset) a).asset.assetPath)
                ).ToArray();
                FR2_Export.MergeDuplicate();
            }

            //if (GUI.Button(bRect, "Remove Others", EditorStyles.miniButton))
            //{
            //    EditorGUIUtility.systemCopyBuffer = asset.guid;
            //    Selection.objects = (parent as FR2_DuplicateFolder).children.Select(
            //        a => FR2_Unity.LoadAssetAtPath<Object>(((FR2_DuplicateAsset)a).asset.assetPath)
            //    ).ToArray();
            //    FR2_Export.MergeDuplicate();
            //}
        }
    }

    internal class FR2_DuplicateFolder : FR2_TreeItemUI
    {
        private static FR2_FileCompare comparer;
        public string assetPath;
        public string count;
        public string filesize;
        public string label;

        public FR2_DuplicateFolder(List<string> list)
        {
            list.Sort((item1, item2) => { return item1.CompareTo(item2); });

            var first = true;
            for (var i = 0; i < list.Count; i++)
            {
                var item = list[i];
                var asset = FR2_Cache.Api.Get(AssetDatabase.AssetPathToGUID(item));

                if (asset == null)
                {
                    Debug.LogWarning("Something wrong, asset not found <" + item + ">");
                    continue;
                }

                if (first)
                {
                    first = false;
                    label = asset.assetName;
                    count = list.Count.ToString();
                    filesize = GetfileSizeString(asset.fileSize);
                    assetPath = item;
                }

                AddChild(new FR2_DuplicateAsset(asset));
            }
        }

        private string GetfileSizeString(long fileSize)
        {
            return fileSize <= 1024
                ? fileSize + " B"
                : fileSize <= 1024 * 1024
                    ? Mathf.RoundToInt(fileSize / 1024f) + " KB"
                    : Mathf.RoundToInt(fileSize / 1024f / 1024f) + " MB";
        }

        protected override void Draw(Rect r)
        {
            var tex = AssetDatabase.GetCachedIcon(assetPath);
            var rect = r;

            if (tex != null)
            {
                rect.width = 16f;
                GUI.DrawTexture(rect, tex);
            }

            rect = r;
            rect.xMin += 16f;
            GUI.Label(rect, label, EditorStyles.boldLabel);

            rect = r;
            rect.xMin += rect.width - 50f;
            GUI.Label(rect, filesize, EditorStyles.miniLabel);

            rect = r;
            rect.xMin += rect.width - 70f;
            GUI.Label(rect, count, EditorStyles.miniLabel);

            rect = r;
            rect.xMin += rect.width - 70f;
        }
    }

    internal class FR2_DuplicateTree : FR2_TreeUI
    {
        private static readonly FR2_FileCompare fc = new FR2_FileCompare();
        private List<FR2_DuplicateFolder> duplicated;

        private FR2_TreeUI tree;

        public void Reset(CBParams assetList)
        {
            fc.Reset(assetList, RefreshView, RefreshView);
        }

        private void RefreshView(CBParams assetList)
        {
            itemH = 18f;
            tree = new FR2_TreeUI();
            duplicated = new List<FR2_DuplicateFolder>();

            for (var i = 0; i < assetList.Count; i++)
            {
                duplicated.Add(new FR2_DuplicateFolder(assetList[i]));
            }
        }

        public bool Draw()
        {
            if (fc.nChunks > 0 && fc.nScaned < fc.nChunks)
            {
                var rect = GUILayoutUtility.GetRect(1, Screen.width, 18f, 18f);
                var p = fc.nScaned / (float) fc.nChunks;

                EditorGUI.ProgressBar(rect, p, string.Format("Scanning {0} / {1}", fc.nScaned, fc.nChunks));
                GUILayout.FlexibleSpace();
                return true;
            }

            if (tree != null)
            {
                tree.Draw(duplicated);
            }
            else
            {
                if (Event.current.type == EventType.Repaint && fc.nChunks == 0)
                {
                    Reset(FR2_Cache.Api.ScanSimilar());
                }
            }

            return false;
        }
    }

    internal class FR2_FileCompare
    {
        public List<FR2_Head> deads = new List<FR2_Head>();
        public List<FR2_Head> heads = new List<FR2_Head>();

        public int nChunks;
        public int nScaned;
        public Action<CBParams> OnCompareComplete;

        public Action<CBParams> OnCompareUpdate;

        public void Reset(CBParams list, Action<CBParams> onUpdate, Action<CBParams> onComplete)
        {
            nChunks = 0;
            nScaned = 0;

            if (heads.Count > 0)
            {
                for (var i = 0; i < heads.Count; i++)
                {
                    heads[i].CloseChunk();
                }
            }

            deads.Clear();
            heads.Clear();

            OnCompareUpdate = onUpdate;
            OnCompareComplete = onComplete;

            for (var i = 0; i < list.Count; i++)
            {
                AddHead(list[i]);
            }

            EditorApplication.update -= ReadChunkAsync;
            EditorApplication.update += ReadChunkAsync;
        }

        public FR2_FileCompare AddHead(List<string> files)
        {
            if (files.Count < 2)
            {
                Debug.LogWarning("Something wrong ! head should not contains < 2 elements");
            }

            var chunkList = new List<FR2_Chunk>();
            for (var i = 0; i < files.Count; i++)
            {
                chunkList.Add(new FR2_Chunk
                {
                    file = files[i],
                    stream = new FileStream(files[i], FileMode.Open, FileAccess.Read),
                    buffer = new byte[FR2_Head.chunkSize]
                });
            }

            var file = new FileInfo(files[0]);
            var nChunk = Mathf.CeilToInt(file.Length / (float) FR2_Head.chunkSize);

            heads.Add(new FR2_Head
            {
                fileSize = file.Length,
                currentChunk = 0,
                nChunk = nChunk,
                chunkList = chunkList
            });

            nChunks += nChunk;

            return this;
        }

        private void ReadChunkAsync()
        {
            var alive = ReadChunk();
            var update = false;

            for (var i = heads.Count - 1; i >= 0; i--)
            {
                var h = heads[i];
                if (h.isDead)
                {
                    h.CloseChunk();
                    heads.RemoveAt(i);
                    if (h.chunkList.Count > 1)
                    {
                        update = true;
                        deads.Add(h);
                    }
                }
            }

            if (update) Trigger(OnCompareUpdate);

            if (!alive)
            {
                nScaned = nChunks;
                EditorApplication.update -= ReadChunkAsync;
                Trigger(OnCompareComplete);
            }
        }

        private void Trigger(Action<CBParams> cb)
        {
            if (cb == null) return;
            var list = deads.Select(item => item.GetFiles()).ToList();

//#if FR2_DEBUG
//        Debug.Log("Callback ! " + deads.Count + ":" + heads.Count);
//#endif
            cb(list);
        }

        private bool ReadChunk()
        {
            var alive = false;
            for (var i = 0; i < heads.Count; i++)
            {
                var h = heads[i];
                if (h.isDead)
                {
                    Debug.LogWarning("Should never be here : " + h.chunkList[0].file);
                    continue;
                }

                nScaned++;
                alive = true;
                h.ReadChunk();
                h.CompareChunk(heads);
                break;
            }

            //if (!alive) return false;

            //alive = false;
            //for (var i = 0; i < heads.Count; i++)
            //{
            //    var h = heads[i];
            //    if (h.isDead) continue;

            //    h.CompareChunk(heads);
            //    alive |= !h.isDead;
            //}

            return alive;
        }
    }

    internal class FR2_Head
    {
        public const int chunkSize = 10240;

        public List<FR2_Chunk> chunkList;
        public int currentChunk;

        public long fileSize;

        public int nChunk;
        public int size; //last stream read size

        public bool isDead
        {
            get { return (currentChunk == nChunk) || (chunkList.Count == 1); }
        }

        public List<string> GetFiles()
        {
            return chunkList.Select(item => item.file).ToList();
        }

        public void AddToDict(byte b, FR2_Chunk chunk, Dictionary<byte, List<FR2_Chunk>> dict)
        {
            List<FR2_Chunk> list;
            if (!dict.TryGetValue(b, out list))
            {
                list = new List<FR2_Chunk>();
                dict.Add(b, list);
            }

            list.Add(chunk);
        }

        public void CloseChunk()
        {
            for (var i = 0; i < chunkList.Count; i++)
            {
                chunkList[i].stream.Close();
                chunkList[i].stream = null;
            }
        }

        public void ReadChunk()
        {
#if FR2_DEBUG
        if (currentChunk == 0) Debug.LogWarning("Read <" + chunkList[0].file + "> " + currentChunk + ":" + nChunk);
#endif
            if (currentChunk == nChunk)
            {
                Debug.LogWarning("Something wrong, should dead <" + isDead + ">");
                return;
            }

            var from = currentChunk * chunkSize;
            size = (int) Mathf.Min(fileSize - from, chunkSize);

            for (var i = 0; i < chunkList.Count; i++)
            {
                var chunk = chunkList[i];
                chunk.size = size;
                chunk.stream.Read(chunk.buffer, 0, size);
            }

            currentChunk++;
        }

        public void CompareChunk(List<FR2_Head> heads)
        {
            var idx = chunkList.Count;
            var buffer = chunkList[idx - 1].buffer;

            while (--idx >= 0)
            {
                var chunk = chunkList[idx];
                var diff = FirstDifferentIndex(buffer, chunk.buffer, size);
                if (diff == -1) continue;
#if FR2_DEBUG
            Debug.Log(string.Format(
                " --> Different found at : idx={0} diff={1} size={2} chunk={3}",
            idx, diff, size, currentChunk));
#endif

                var v = buffer[diff];
                var d = new Dictionary<byte, List<FR2_Chunk>>(); //new heads

                chunkList.RemoveAt(idx);
                AddToDict(chunk.buffer[diff], chunk, d);

                for (var j = idx - 1; j >= 0; j--)
                {
                    var tChunk = chunkList[j];
                    var tValue = tChunk.buffer[diff];
                    if (tValue == v) continue;

                    idx--;
                    chunkList.RemoveAt(j);
                    AddToDict(tChunk.buffer[diff], tChunk, d);
                }

                foreach (var item in d)
                {
                    var list = item.Value;
                    if (list.Count == 1)
                    {
#if FR2_DEBUG
                    Debug.Log(" --> Dead head found for : " + list[0].file);
#endif
                    }
                    else if (list.Count > 1) // 1 : dead head
                    {
#if FR2_DEBUG
                    Debug.Log(" --> NEW HEAD : " + list[0].file);
#endif
                        heads.Add(new FR2_Head
                        {
                            nChunk = nChunk,
                            fileSize = fileSize,
                            currentChunk = currentChunk - 1,
                            chunkList = list
                        });
                    }
                }
            }
        }

        internal static int FirstDifferentIndex(byte[] arr1, byte[] arr2, int maxIndex)
        {
            for (var i = 0; i < maxIndex; i++)
            {
                if (arr1[i] != arr2[i]) return i;
            }

            return -1;
        }
    }

    internal class FR2_Chunk
    {
        public byte[] buffer;
        public string file;
        public long size;
        public FileStream stream;
    }
}