using BiangLibrary.Singleton;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraManager : MonoSingleton<CameraManager>
{
    public Camera MainCamera => FieldCamera.Camera;
    public Camera BattleUICamera;
    public FieldCamera FieldCamera;

    public PostProcessVolume PostProcessVolume;
}