using System.Collections;
using System.Collections.Generic;
using System.IO;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public class BoxScreenShotTool : Editor
{
    [MenuItem("Assets/开发工具/Box拍照工具", priority = -50)]
    public static void CaptureScreenOnOneBox()
    {
        float defaultVerticalAngle = CameraManager.Instance.FieldCamera.TargetConfigData.VerAngle;
        float defaultHorizontalAngle = CameraManager.Instance.FieldCamera.TargetConfigData.HorAngle;
        float defaultFOV = CameraManager.Instance.FieldCamera.TargetConfigData.FOV;
        Color defaultBackgroundColor = CameraManager.Instance.MainCamera.backgroundColor;

        CameraManager.Instance.FieldCamera.TargetConfigData.FOV = 10;

        GameObject[] selectedGOs = Selection.gameObjects;
        foreach (GameObject go in selectedGOs)
        {
            Box box = go.GetComponent<Box>();
            int count = 0;
            ScreenShotAtOneBox(box.gameObject, false, selectedGOs.Length, ref count);
        }

        CameraManager.Instance.FieldCamera.TargetConfigData.VerAngle = defaultVerticalAngle;
        CameraManager.Instance.FieldCamera.TargetConfigData.HorAngle = defaultHorizontalAngle;
        CameraManager.Instance.FieldCamera.TargetConfigData.FOV = defaultFOV;
        CameraManager.Instance.FieldCamera.ForceUpdateCamera();
        CameraManager.Instance.MainCamera.backgroundColor = defaultBackgroundColor;
        EditorUtility.ClearProgressBar();

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Box拍照", "拍照完成", "好");
    }

    [MenuItem("开发工具/Box拍照工具")]
    public static void CaptureScreen()
    {
        ConfigManager.ExportConfigs(false, false);
        float defaultVerticalAngle = CameraManager.Instance.FieldCamera.TargetConfigData.VerAngle;
        float defaultHorizontalAngle = CameraManager.Instance.FieldCamera.TargetConfigData.HorAngle;
        float defaultFOV = CameraManager.Instance.FieldCamera.TargetConfigData.FOV;
        Color defaultBackgroundColor = CameraManager.Instance.MainCamera.backgroundColor;

        CameraManager.Instance.FieldCamera.TargetConfigData.FOV = 10;

        bool cancel = false;
        int totalCount = (ConfigManager.TypeDefineConfigs[TypeDefineType.Box].TypeIndexDict.Count - 2) * 12;
        int count = 0;
        cancel = EditorUtility.DisplayCancelableProgressBar("拍照中", $"准备开始", 0);
        if (!cancel)
        {
            foreach (KeyValuePair<string, ushort> kv in ConfigManager.TypeDefineConfigs[TypeDefineType.Box].TypeIndexDict)
            {
                if (cancel) break;
                if (kv.Key.Equals("BorderBox_Hidden")) continue;
                if (kv.Key.Equals("EnemyFrozenBox")) continue;
                GameObject boxPrefab = (GameObject) Resources.Load("Prefabs/Designs/Box/" + kv.Key);
                cancel = ScreenShotAtOneBox(boxPrefab, cancel, totalCount, ref count);
            }
        }

        CameraManager.Instance.FieldCamera.TargetConfigData.VerAngle = defaultVerticalAngle;
        CameraManager.Instance.FieldCamera.TargetConfigData.HorAngle = defaultHorizontalAngle;
        CameraManager.Instance.FieldCamera.TargetConfigData.FOV = defaultFOV;
        CameraManager.Instance.FieldCamera.ForceUpdateCamera();
        CameraManager.Instance.MainCamera.backgroundColor = defaultBackgroundColor;
        EditorUtility.ClearProgressBar();

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Box拍照", "拍照完成", "好");
    }

    private static bool ScreenShotAtOneBox(GameObject boxPrefab, bool cancel, int totalCount, ref int count)
    {
        GameObject boxGO = Instantiate(boxPrefab, null);
        Box box = boxGO.GetComponent<Box>();
        float maxSizeDimension = float.MinValue;
        Vector3 size = box.EntityIndicatorHelper.EntityOccupationData.BoundsInt.size;
        maxSizeDimension = Mathf.Max(maxSizeDimension, size.x);
        maxSizeDimension = Mathf.Max(maxSizeDimension, size.y);
        maxSizeDimension = Mathf.Max(maxSizeDimension, size.z);

        CameraManager.Instance.FieldCamera.TargetConfigData.Offset = new Vector2(0, 1);
        if (maxSizeDimension <= 1)
        {
            CameraManager.Instance.FieldCamera.TargetConfigData.FOV = 20;
        }
        else if (maxSizeDimension <= 2)
        {
            CameraManager.Instance.FieldCamera.TargetConfigData.FOV = 30;
        }
        else if (maxSizeDimension <= 3)
        {
            CameraManager.Instance.FieldCamera.TargetConfigData.FOV = 40;
        }
        else
        {
            CameraManager.Instance.FieldCamera.TargetConfigData.FOV = 60;
        }

        for (int verAngle = 0; verAngle <= 60; verAngle += 30)
        {
            if (cancel) break;
            for (int horAngle = 0; horAngle <= 45; horAngle += 15)
            {
                if (cancel) break;
                count++;
                string filename = Application.dataPath + "/Textures/BoxScreenShots/" + boxPrefab.name + "_" + horAngle + "_" + verAngle + ".png";
                CameraManager.Instance.FieldCamera.TargetConfigData.HorAngle = horAngle;
                CameraManager.Instance.FieldCamera.TargetConfigData.VerAngle = verAngle;
                CameraManager.Instance.FieldCamera.ForceUpdateCamera();
                CaptureScreenShot.CaptureTransparentScreenShot(Camera.main, 1920, 1080, filename);
                cancel = EditorUtility.DisplayCancelableProgressBar("拍照中", $"{boxPrefab.name} 水平角{horAngle} 竖直角{verAngle}", (float) count / totalCount);
            }
        }

        DestroyImmediate(box.gameObject);
        return cancel;
    }
}

public static class CaptureScreenShot
{
    public static void CaptureTransparentScreenShot(Camera cam, int width, int height, string screenGrabFile_Path)
    {
        // This is slower, but seems more reliable.
        RenderTexture bak_cam_targetTexture = cam.targetTexture;
        CameraClearFlags bak_cam_clearFlags = cam.clearFlags;
        RenderTexture bak_RenderTexture_active = RenderTexture.active;

        Texture2D tex_white = new Texture2D(width, height, TextureFormat.ARGB32, false);
        Texture2D tex_black = new Texture2D(width, height, TextureFormat.ARGB32, false);
        Texture2D tex_transparent = new Texture2D(width, height, TextureFormat.ARGB32, false);
        // Must use 24-bit depth buffer to be able to fill background.
        RenderTexture render_texture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
        Rect grab_area = new Rect(0, 0, width, height);

        RenderTexture.active = render_texture;
        cam.targetTexture = render_texture;
        cam.clearFlags = CameraClearFlags.SolidColor;

        cam.backgroundColor = Color.black;
        cam.Render();
        tex_black.ReadPixels(grab_area, 0, 0);
        tex_black.Apply();

        cam.backgroundColor = Color.white;
        cam.Render();
        tex_white.ReadPixels(grab_area, 0, 0);
        tex_white.Apply();

        // Create Alpha from the difference between black and white camera renders
        for (int y = 0; y < tex_transparent.height; ++y)
        {
            for (int x = 0; x < tex_transparent.width; ++x)
            {
                float alpha = tex_white.GetPixel(x, y).r - tex_black.GetPixel(x, y).r;
                alpha = 1.0f - alpha;
                Color color;
                if (alpha.Equals(0))
                {
                    color = Color.clear;
                }
                else
                {
                    color = tex_black.GetPixel(x, y) / alpha;
                }

                color.a = alpha;
                tex_transparent.SetPixel(x, y, color);
            }
        }

        // Encode the resulting output texture to a byte array then write to the file
        Rect croppedRect = CropRect(tex_transparent);
        Texture2D cropTexture = new Texture2D((int) croppedRect.width, (int) croppedRect.height, TextureFormat.ARGB32, false);
        Color[] cropPixels = tex_transparent.GetPixels((int) croppedRect.x, (int) croppedRect.y, (int) croppedRect.width, (int) croppedRect.height);
        cropTexture.SetPixels(cropPixels);
        byte[] pngShot = ImageConversion.EncodeToPNG(cropTexture);
        File.WriteAllBytes(screenGrabFile_Path, pngShot);

        cam.clearFlags = bak_cam_clearFlags;
        cam.targetTexture = bak_cam_targetTexture;
        RenderTexture.active = bak_RenderTexture_active;
        RenderTexture.ReleaseTemporary(render_texture);

        Object.DestroyImmediate(tex_black);
        Object.DestroyImmediate(tex_white);
        Object.DestroyImmediate(tex_transparent);
        Object.DestroyImmediate(cropTexture);
    }

    public static void SimpleCaptureTransparentScreenShot(Camera cam, int width, int height, string screenGrabFile_Path)
    {
        // Depending on your render pipeline, this may not work.
        RenderTexture bak_cam_targetTexture = cam.targetTexture;
        CameraClearFlags bak_cam_clearFlags = cam.clearFlags;
        RenderTexture bak_RenderTexture_active = RenderTexture.active;

        Texture2D tex_transparent = new Texture2D(width, height, TextureFormat.ARGB32, false);
        // Must use 24-bit depth buffer to be able to fill background.
        RenderTexture render_texture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
        Rect grab_area = new Rect(0, 0, width, height);

        RenderTexture.active = render_texture;
        cam.targetTexture = render_texture;
        cam.clearFlags = CameraClearFlags.SolidColor;

        // Simple: use a clear background
        cam.backgroundColor = Color.clear;
        cam.Render();
        tex_transparent.ReadPixels(grab_area, 0, 0);
        tex_transparent.Apply();

        // Encode the resulting output texture to a byte array then write to the file
        Rect croppedRect = CropRect(tex_transparent);
        Texture2D tex_transparent_cropped = tex_transparent.CropTexture(croppedRect);
        byte[] pngShot = ImageConversion.EncodeToPNG(tex_transparent_cropped);
        File.WriteAllBytes(screenGrabFile_Path, pngShot);

        cam.clearFlags = bak_cam_clearFlags;
        cam.targetTexture = bak_cam_targetTexture;
        RenderTexture.active = bak_RenderTexture_active;
        RenderTexture.ReleaseTemporary(render_texture);

        Object.DestroyImmediate(tex_transparent);
        Object.DestroyImmediate(tex_transparent_cropped);
    }

    private static Rect CropRect(Texture2D texture)
    {
        Color[] pixels = texture.GetPixels();
        int newLeft = -1;
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                int index = CoordinateToGridIndex(x, y, texture.width, texture.height);
                Color color = pixels[index];
                if (color.a > 0.01f)
                {
                    newLeft = x;
                    break;
                }
            }

            if (newLeft != -1)
            {
                break;
            }
        }

        int newRight = -1;
        for (int x = texture.width - 1; x >= 0; x--)
        {
            for (int y = 0; y < texture.height; y++)
            {
                int index = CoordinateToGridIndex(x, y, texture.width, texture.height);
                Color color = pixels[index];
                if (color.a > 0.01f)
                {
                    newRight = x;
                    break;
                }
            }

            if (newRight != -1)
            {
                break;
            }
        }

        int newTop = -1;
        for (int y = texture.height - 1; y >= 0; y--)
        {
            for (int x = 0; x < texture.width; x++)
            {
                int index = CoordinateToGridIndex(x, y, texture.width, texture.height);
                Color color = pixels[index];
                if (color.a > 0.01f)
                {
                    newTop = y;
                    break;
                }
            }

            if (newTop != -1)
            {
                break;
            }
        }

        int newBottom = -1;
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                int index = CoordinateToGridIndex(x, y, texture.width, texture.height);
                Color color = pixels[index];
                if (color.a > 0.01f)
                {
                    newBottom = y;
                    break;
                }
            }

            if (newBottom != -1)
            {
                break;
            }
        }

        return new Rect(newLeft, newBottom, newRight - newLeft, newTop - newBottom);
    }

    private static int CoordinateToGridIndex(int x, int y, int xMax, int yMax)
    {
        if (x >= xMax || y >= yMax || x < 0 || y < 0)
        {
            return -1;
        }

        return ((y * xMax) + x);
    }
}