using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class FieldCamera : MonoBehaviour
{
    public Camera Camera;

    [SerializeField]
    private Camera BattleUICamera;

    [SerializeField]
    private List<Transform> targetList = new List<Transform>();

    [LabelText("水平角")]
    public float HorizontalAngle;

    [LabelText("竖直角")]
    public float VerticalAngle;

    [LabelText("距离")]
    public float Distance;

    [LabelText("InGameUISize")]
    public float InGameUISize;

    [LabelText("相机震屏力度曲线(x伤害y强度")]
    public AnimationCurve CameraShakeStrengthCurve;

    [LabelText("相机震屏力度曲线(x距离y衰减")]
    public AnimationCurve CameraShakeAttenuationByDistanceCurve;

    void Awake()
    {
        Distance_Level = 2;
    }

    void Start()
    {
        ClientGameManager.Instance.BattleMessenger.AddListener<Actor>((uint) Enum_Events.OnPlayerLoaded, AddTargetActor);
        ClientGameManager.Instance.BattleMessenger.AddListener<WorldCameraPOI>((uint) Enum_Events.OnWorldCameraPOILoaded, AddTargetPOI);
    }

    public void InitFocus()
    {
        CameraLerp(false);
    }

    private void AddTargetActor(Actor actor)
    {
        targetList.Add(actor.transform);
    }

    public void AddTargetPOI(WorldCameraPOI poi)
    {
        targetList.Add(poi.transform);
    }

    public float GetScaleForBattleUI()
    {
        return DistanceLevels_ScaleForBattleUI[Distance_Level];
    }

    public float SmoothTime = 0.05f;
    Vector3 curVelocity = Vector3.zero;

    private void Update()
    {
        UpdateFOVLevel();
    }

    private void LateUpdate()
    {
        CameraLerp(true);
    }

    private void CameraLerp(bool lerp)
    {
        Vector3 targetCenter = Vector3.zero;
        foreach (Transform trans in targetList)
        {
            targetCenter += trans.position;
        }

        if (targetList.Count != 0)
        {
            targetCenter /= targetList.Count;
        }

        Vector3 diff = Vector3.zero;
        diff.x = Distance * Mathf.Cos(VerticalAngle * Mathf.Deg2Rad) * Mathf.Sin(HorizontalAngle * Mathf.Deg2Rad);
        diff.y = Distance * Mathf.Sin(VerticalAngle * Mathf.Deg2Rad);
        diff.z = -Distance * Mathf.Cos(VerticalAngle * Mathf.Deg2Rad) * Mathf.Cos(HorizontalAngle * Mathf.Deg2Rad);
        Vector3 destination = targetCenter + diff;
        if (lerp)
        {
            transform.position = Vector3.SmoothDamp(transform.position, destination, ref curVelocity, SmoothTime);
        }
        else
        {
            transform.position = destination;
        }

        transform.forward = targetCenter - destination;
    }

    public void CameraShake(int damage, float distanceFromPlayer)
    {
        Camera.transform.DOShakePosition(0.1f, CameraShakeStrengthCurve.Evaluate(damage) * CameraShakeAttenuationByDistanceCurve.Evaluate(distanceFromPlayer), 10, 90f);
    }

    #region Distance Levels

    private int _distance_Level = 0;

    internal int Distance_Level
    {
        get { return _distance_Level; }
        set
        {
            if (_distance_Level != value)
            {
                _distance_Level = Mathf.Clamp(value, 0, DistanceLevels.Length - 1);
                Distance = DistanceLevels[_distance_Level];
                InGameUISize = InGameUISizeLevels[_distance_Level];
                Camera.orthographicSize = OrthographicSizeLevels[_distance_Level];
                BattleUICamera.orthographicSize = OrthographicSizeLevels[_distance_Level];
            }
        }
    }

    [LabelText("距离等级表")]
    public float[] DistanceLevels = new float[] {10, 15, 20, 25, 30, 35, 50, 75};

    [LabelText("距离等级表-战斗飘字UI")]
    public float[] DistanceLevels_ScaleForBattleUI = new float[] {1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f};

    [LabelText("正交大小等级表")]
    public float[] OrthographicSizeLevels = new float[] {5, 7, 9, 10, 11, 12, 13, 15};

    [LabelText("InGameUI大小等级")]
    public float[] InGameUISizeLevels = new float[] {2f, 1.5f, 1, 0.8f, 0.65f, 0.5f, 0.3f, 0.2f};

    void UpdateFOVLevel()
    {
        if (ControlManager.Instance.Battle_MouseWheel.y < 0)
        {
            Distance_Level++;
        }

        if (ControlManager.Instance.Battle_MouseWheel.y > 0)
        {
            Distance_Level--;
        }
    }

    #endregion
}