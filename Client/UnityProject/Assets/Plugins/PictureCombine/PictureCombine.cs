using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System;
using System.IO;
using UnityEditor;
using Object = UnityEngine.Object;

/*此脚本用途：把指定文件夹内的所有.gif,.jpg,.png图片全部整合为一个PNG图片，提供图文混合使用表情包
 这里引用的System.Drawing是在C:\Program Files (x86)\Unity\Editor\Data\Mono\lib\mono\2.0目录下，这个是unity安装的目录;
 */
public static class PictureCombine
{
    //考虑到低端机型的原因
    static int maxWidth = 1024;

    //当前的行数
    static int gifCurRow = 0;

    static int pngCurRow = 0;

    //图片当前的最大高度
    static int gifMaxHeight = 0;

    static int pngMaxHeight = 0;

    //换行时上一行图片的高度
    static int gifLastRowHeight = 0;
    static int pngLastRowHeight = 0;

    [MenuItem("Assets/Create/PictureCombine")]
    static void PictureAllCombine()
    {
        Debug.Log("PictureAllCombine start");
        //扫描文件夹，把所有GIF和JPG路径存储，先把每一个GIF的帧图片分表情依次合成，最后再合成JPG,PNG
        //需要把GIF表情帧图片合成，包括不同的长宽的表情，以及不同尺寸的JPG

        Object obj = Selection.activeObject;
        if (obj != null)
        {
            string folderPath = AssetDatabase.GetAssetPath(obj.GetInstanceID());
            if (folderPath.Length > 0)
            {
                if (Directory.Exists(folderPath))
                {
                    List<string> gifList = new List<string>();
                    List<string> pngList = new List<string>();
                    List<string> jpgList = new List<string>();
                    if (Directory.Exists(folderPath))
                    {
                        DirectoryInfo direction = new DirectoryInfo(folderPath);
                        FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
                        //Debug.Log ("文件总数量："+files.Length);

                        for (int i = 0; i < files.Length; i++)
                        {
                            if (files[i].Name.EndsWith(".meta"))
                            {
                                continue;
                            }

                            if (files[i].ToString().Contains(".gif"))
                                gifList.Add(files[i].ToString());

                            if (files[i].ToString().Contains(".jpg"))
                                jpgList.Add(files[i].ToString());

                            if (files[i].ToString().Contains(".png"))
                                pngList.Add(files[i].ToString());
                        }
                    }

                    CombineAll(gifList, jpgList, pngList, folderPath);
                }
            }
        }
    }

    static void CombineAll(List<string> gifs, List<string> jpgs, List<string> pngs, string saveFolder)
    {
        Bitmap gifBmp = GetGifCombine(gifs, saveFolder);
        Bitmap jpgBmp = GetJpgCombine(jpgs, saveFolder);
        Bitmap pngBmp = GetPngCombine(pngs, saveFolder);
        CombineAllBitmap(gifBmp, jpgBmp, pngBmp, saveFolder);
    }

    static Bitmap GetGifCombine(List<string> gifs, string saveFolder)
    {
        Debug.Log("GetGifCombine： " + gifs.Count);
        if (gifs.Count == 0)
            return null;

        gifCurRow = 0;
        gifLastRowHeight = 0;
        gifMaxHeight = 0;

        string path = "";
        Bitmap newBmp = new Bitmap(1, 1);
        int LastWidth = 0;
        int LastHeight = 0;
        int rowIndex = 0;
        Boolean isNextRow = false;

        for (int i = 0; i < gifs.Count; i++)
        {
            path = gifs[i];
            Image gif = Image.FromFile(path);
            FrameDimension fd = new FrameDimension(gif.FrameDimensionsList[0]);
            int count = gif.GetFrameCount(fd);

            for (int j = 0; j < count; j++)
            {
                gif.SelectActiveFrame(fd, j);
                Bitmap gifBmp = new Bitmap(gif);
                rowIndex = 0;
                isNextRow = false;
                rowIndex = (LastWidth + gifBmp.Width) / maxWidth;

                //rowIndex 大于0表示下一个图片宽度超过最大宽度，需要换行
                if (rowIndex > 0)
                {
                    LastWidth = 0;
                    LastHeight = newBmp.Height;
                    isNextRow = true;
                    gifCurRow++;
                }

                if (i == gifs.Count - 1 && j == count - 1)
                {
                    newBmp = CombineGifBitmap(newBmp, gifBmp, LastWidth, LastHeight, isNextRow, true, saveFolder, "GifCombine");
                }
                else
                {
                    newBmp = CombineGifBitmap(newBmp, gifBmp, LastWidth, LastHeight, isNextRow, false, saveFolder, "GifCombine");
                    LastWidth = LastWidth + gifBmp.Width;
                }
            }
        }

        Debug.Log("GifCombine end");
        return newBmp;
    }

    static Bitmap CombineGifBitmap(Bitmap bmp1, Bitmap bmp2, int lastWidth, int lastHeight, Boolean isNextRow, Boolean isSave, string saveFolder, string saveName)
    {
        int maxHeight = 0;
        if (isNextRow)
        {
            maxHeight = bmp1.Height + bmp2.Height;
            gifLastRowHeight = bmp1.Height;
        }
        else
        {
            //一旦换行过程中，后续的表情比前面的高，会出现表情存储部分的情况，需要记录上一行的高度
            if (gifCurRow == 0)
            {
                if (bmp2.Height > bmp1.Height)
                    maxHeight = bmp2.Height;
                else
                    maxHeight = bmp1.Height;
            }
            else
            {
                maxHeight = gifLastRowHeight + bmp2.Height;
            }
        }

        //记录图片的最高高度
        if (maxHeight >= gifMaxHeight)
        {
            gifMaxHeight = maxHeight;
        }

        //限定最大宽度，进行换行排列图片，节省空间
        Bitmap newBmp = new Bitmap(maxWidth, gifMaxHeight);
        System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(newBmp);
        g.DrawImage(bmp1, 0, 0);
        g.DrawImage(bmp2, lastWidth, lastHeight);
        g.Save();

        if (isSave)
            newBmp.Save(saveFolder + saveName + ".png", ImageFormat.Png);

        return newBmp;
    }

    static Bitmap GetJpgCombine(List<string> jpgs, string saveFolder)
    {
        Debug.Log("GetJpgCombine： " + jpgs.Count);
        if (jpgs.Count == 0)
            return null;

        pngCurRow = 0;
        pngLastRowHeight = 0;
        pngMaxHeight = 0;

        string path = "";
        Bitmap newBmp = new Bitmap(1, 1);

        int LastWidth = 0;
        int LastHeight = 0;
        int rowIndex = 0;
        Boolean isNextRow = false;

        for (int i = 0; i < jpgs.Count; i++)
        {
            path = jpgs[i];
            Image jpg = Image.FromFile(path);
            Bitmap jpgBmp = new Bitmap(jpg);
            rowIndex = 0;
            isNextRow = false;
            rowIndex = (LastWidth + jpgBmp.Width) / maxWidth;

            if (rowIndex > 0)
            {
                LastWidth = 0;
                LastHeight = newBmp.Height;
                isNextRow = true;
                pngCurRow++;
            }

            if (i == jpgs.Count - 1)
            {
                newBmp = CombineStaticBitmap(newBmp, jpgBmp, LastWidth, LastHeight, isNextRow, true, saveFolder, "JpgCombine");
            }
            else
            {
                newBmp = CombineStaticBitmap(newBmp, jpgBmp, LastWidth, LastHeight, isNextRow, false, saveFolder, "JpgCombine");
                //记录当前添加的最大宽度
                LastWidth = LastWidth + jpgBmp.Width;
            }
        }

        Debug.Log("JpgCombine end");
        return newBmp;
    }

    static Bitmap GetPngCombine(List<string> pngs, string saveFolder)
    {
        Debug.Log("GetPngCombine： " + pngs.Count);
        if (pngs.Count == 0)
            return null;

        pngCurRow = 0;
        pngLastRowHeight = 0;
        pngMaxHeight = 0;

        Bitmap newBmp = new Bitmap(1, 1);
        int LastWidth = 0;
        int LastHeight = 0;
        int rowIndex = 0;
        Boolean isNextRow = false;
        string path = "";

        for (int i = 0; i < pngs.Count; i++)
        {
            path = pngs[i];
            Image png = Image.FromFile(path);
            Bitmap pngBmp = new Bitmap(png);

            isNextRow = false;
            rowIndex = (LastWidth + pngBmp.Width) / maxWidth;

            if (rowIndex > 0)
            {
                LastWidth = 0;
                LastHeight = newBmp.Height;
                isNextRow = true;
                pngCurRow++;
            }

            if (i == pngs.Count - 1)
            {
                newBmp = CombineStaticBitmap(newBmp, pngBmp, LastWidth, LastHeight, isNextRow, true, saveFolder, "PngCombine");
            }
            else
            {
                newBmp = CombineStaticBitmap(newBmp, pngBmp, LastWidth, LastHeight, isNextRow, false, saveFolder, "PngCombine");
                //记录当前添加的最大宽度
                LastWidth = LastWidth + pngBmp.Width;
            }
        }

        Debug.Log("PngCombine end");
        return newBmp;
    }

    static Bitmap CombineStaticBitmap(Bitmap bmp1, Bitmap bmp2, int lastWidth, int lastHeight, Boolean isNextRow, Boolean isSave, string saveFolder, string saveName)
    {
        int maxHeight = 0;
        if (isNextRow)
        {
            maxHeight = bmp1.Height + bmp2.Height;
            pngLastRowHeight = bmp1.Height;
        }
        else
        {
            if (gifCurRow == 0)
            {
                if (bmp2.Height > bmp1.Height)
                    maxHeight = bmp2.Height;
                else
                    maxHeight = bmp1.Height;
            }
            else
            {
                maxHeight = pngLastRowHeight + bmp2.Height;
            }
        }

        //记录当前行的最高高度，作用下一行绘图开始坐标
        if (maxHeight >= pngMaxHeight)
        {
            pngMaxHeight = maxHeight;
        }

        Bitmap newBmp = new Bitmap(maxWidth, pngMaxHeight);
        System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(newBmp);
        g.DrawImage(bmp1, 0, 0);
        g.DrawImage(bmp2, lastWidth, lastHeight);
        g.Save();

        if (isSave)
            newBmp.Save(saveFolder + saveName + ".png", ImageFormat.Png);

        return newBmp;
    }

    static void CombineAllBitmap(Bitmap bmp1, Bitmap bmp2, Bitmap bmp3, string saveFolder)
    {
        int maxWidth = 0;
        if (bmp1 == null && bmp2 == null & bmp3 == null)
            return;
        if (bmp1 != null && bmp2 == null && bmp3 == null)
        {
            Debug.Log("Gif Combine Finish");
            return;
        }

        if (bmp1 == null && bmp2 != null && bmp3 == null)
        {
            Debug.Log("Jpg Combine Finish");
            return;
        }

        if (bmp1 == null && bmp2 == null && bmp3 != null)
        {
            Debug.Log("Png Combine Finish");
            return;
        }

        if (bmp1 != null && bmp2 != null && bmp3 == null)
        {
            if (bmp1.Width >= bmp2.Width)
                maxWidth = bmp1.Width;
            else
                maxWidth = bmp2.Width;

            int maxHeight = bmp1.Height + bmp2.Height;
            Bitmap newBmp = new Bitmap(maxWidth, maxHeight);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(newBmp);
            g.DrawImage(bmp1, 0, 0);
            g.DrawImage(bmp2, 0, bmp1.Height);
            g.Save();
            newBmp.Save(saveFolder + "TotalCombine.png", ImageFormat.Png);
            Debug.Log("Gif Jpg Combine Finish");
        }

        if (bmp1 != null && bmp2 == null && bmp3 != null)
        {
            if (bmp1.Width >= bmp3.Width)
                maxWidth = bmp1.Width;
            else
                maxWidth = bmp3.Width;

            int maxHeight = bmp1.Height + bmp3.Height;
            Bitmap newBmp = new Bitmap(maxWidth, maxHeight);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(newBmp);
            g.DrawImage(bmp1, 0, 0);
            g.DrawImage(bmp3, 0, bmp1.Height);
            g.Save();
            newBmp.Save(saveFolder + "TotalCombine.png", ImageFormat.Png);
            Debug.Log("Gif Png Combine Finish");
        }

        if (bmp1 == null && bmp2 != null && bmp3 != null)
        {
            if (bmp2.Width >= bmp3.Width)
                maxWidth = bmp2.Width;
            else
                maxWidth = bmp3.Width;

            int maxHeight = bmp2.Height + bmp3.Height;
            Bitmap newBmp = new Bitmap(maxWidth, maxHeight);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(newBmp);
            g.DrawImage(bmp2, 0, 0);
            g.DrawImage(bmp3, 0, bmp2.Height);
            g.Save();
            newBmp.Save(saveFolder + "TotalCombine.png", ImageFormat.Png);
            Debug.Log("Jpg Png Combine Finish");
        }

        if (bmp1 != null && bmp2 != null && bmp3 != null)
        {
            if (bmp1.Width >= bmp2.Width)
                maxWidth = bmp1.Width;
            else
                maxWidth = bmp2.Width;

            if (bmp3.Width >= maxWidth)
                maxWidth = bmp3.Width;

            int maxHeight = bmp1.Height + bmp2.Height + bmp3.Height;

            Bitmap newBmp = new Bitmap(maxWidth, maxHeight);

            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(newBmp);
            g.DrawImage(bmp1, 0, 0);
            g.DrawImage(bmp2, 0, bmp1.Height);
            g.DrawImage(bmp3, 0, bmp1.Height + bmp2.Height);
            g.Save();

            newBmp.Save(saveFolder + "TotalCombine.png", ImageFormat.Png);
            Debug.Log("Gif Jpg Png Combine Finish");
        }
    }
}
#endif