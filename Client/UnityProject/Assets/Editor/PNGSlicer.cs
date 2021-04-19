using UnityEngine;
using UnityEditor;
using System.IO;

public static class ImageSlicer
{
    [MenuItem("Assets/ImageSlicer/Process to Sprites")]
    static void ProcessToSprite()
    {
        foreach (Object obj in Selection.objects)
        {
            Texture2D image = obj as Texture2D;
            string rootPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(image)); //获取路径名称  

            //图片路径名称  
            string pathPNG = rootPath + "/" + image.name + ".PNG";
            string pathJPG = rootPath + "/" + image.name + ".JPG"; //图片路径名称  

            //获取图片入口  
            TextureImporter texImp = AssetImporter.GetAtPath(pathPNG) as TextureImporter;
            if (texImp == null) texImp = AssetImporter.GetAtPath(pathJPG) as TextureImporter;

            AssetDatabase.CreateFolder(rootPath, image.name); //创建文件夹  

            foreach (SpriteMetaData metaData in texImp.spritesheet) //遍历小图集  
            {
                Texture2D myimage = new Texture2D((int) metaData.rect.width, (int) metaData.rect.height);

                //abc_0:(x:2.00, y:400.00, width:103.00, height:112.00)  
                for (int y = (int) metaData.rect.y; y < metaData.rect.y + metaData.rect.height; y++) //Y轴像素  
                {
                    for (int x = (int) metaData.rect.x; x < metaData.rect.x + metaData.rect.width; x++)
                        myimage.SetPixel(x - (int) metaData.rect.x, y - (int) metaData.rect.y, image.GetPixel(x, y));
                }

                //转换纹理到EncodeToPNG兼容格式  
                if (myimage.format != TextureFormat.ARGB32 && myimage.format != TextureFormat.RGB24)
                {
                    Texture2D newTexture = new Texture2D(myimage.width, myimage.height);
                    newTexture.SetPixels(myimage.GetPixels(0), 0);
                    myimage = newTexture;
                }

                var pngData = myimage.EncodeToPNG();

                //AssetDatabase.CreateAsset(myimage, rootPath + "/" + image.name + "/" + metaData.name + ".PNG");  
                File.WriteAllBytes(rootPath + "/" + image.name + "/" + metaData.name + ".PNG", pngData);
                // 刷新资源窗口界面  
                AssetDatabase.Refresh();
            }
        }
    }
}