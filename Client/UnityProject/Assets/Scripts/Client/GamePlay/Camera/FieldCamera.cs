using System;
using System.Collections.Generic;
using BiangStudio.GameDataFormat.Grid;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

[ExecuteInEditMode]
public class FieldCamera : MonoBehaviour
{
    #region 相机本体

    public Camera Camera;

    [SerializeField]
    private Camera BattleUICamera;

    [SerializeField]
    private Transform CamRoot;

    private Vector3 CamRootPos
    {
        get { return CamRoot.position; }
        set { CamRoot.position = value; }
    }

    private Quaternion CamRootRot
    {
        get { return CamRoot.rotation; }
        set { CamRoot.rotation = value; }
    }

    [SerializeField]
    [ReadOnly]
    [BoxGroup("实时属性")]
    [LabelText("当前相机目标点(未考虑Offset)")]
    private Vector3 TargetPos;

    [SerializeField]
    [ReadOnly]
    [BoxGroup("实时属性")]
    [LabelText("视野宽度")]
    public float FrustumWidth;

    [SerializeField]
    [ReadOnly]
    [BoxGroup("实时属性")]
    [LabelText("视野高度")]
    public float FrustumHeight;

    internal Vector3 CurCameraForward = Vector3.forward;
    internal Vector3 CurCameraRight = Vector3.right;
    internal Vector3 CurCameraUp = Vector3.up;

    #endregion

    [SerializeField]
    private List<Transform> targetList = new List<Transform>();

    [ReadOnly]
    [LabelText("回溯参数")]
    public CameraConfigData LastConfigData = new CameraConfigData();

    [ReadOnly]
    [LabelText("当前参数")]
    public CameraConfigData CurrentConfigData = new CameraConfigData();

    [LabelText("目标参数")]
    public CameraConfigData TargetConfigData = new CameraConfigData();

    private void SetTargetConfigData(CameraConfigData newData)
    {
        newData.ApplyTo(TargetConfigData, true); // TargetConfigData立即同步，反映在面板上，体现目前相机被影响了
        CurrentConfigData.ApplyTo(LastConfigData, true);
        TargetConfigData.ApplyTo(CurrentConfigData, false); // 部分平滑参数需要一段时间逐渐同步给CurrentConfigData
    }

    [Serializable]
    public class CameraConfigData
    {
        // 基本属性
        [SerializeField]
        [BoxGroup("基本属性")]
        [LabelText("水平偏转角")]
        public float HorAngle = 0.0f;

        [SerializeField]
        [BoxGroup("基本属性")]
        [LabelText("竖直偏转角")]
        public float VerAngle = 60f;

        [SerializeField]
        [BoxGroup("基本属性")]
        [LabelText("屏幕倾斜角")]
        [Range(-180, 180)]
        public float ScreenAngle = 0f;

        [SerializeField]
        [BoxGroup("基本属性")]
        [LabelText("FOV")]
        [Range(5, 70)]
        public float FOV = 35f;

        [SerializeField]
        [BoxGroup("基本属性")]
        [LabelText("相机偏移(平行屏幕平面)")]
        public Vector2 Offset = new Vector2(0.0f, 0f);

        [SerializeField]
        [BoxGroup("基本属性")]
        [LabelText("目标距离")]
        [Range(0, 50)]
        public float Distance = 13.5f;

        [SerializeField]
        [BoxGroup("基本属性")]
        [LabelText("移动平滑时间(秒)")]
        [Range(0.00001f, 10)]
        public float DampPosTime = 0.5f;

        [SerializeField]
        [BoxGroup("基本属性")]
        [LabelText("旋转平滑时间(秒)")]
        [Range(0.00001f, 10)]
        public float DampRotTime = 0.5f;

        [SerializeField]
        [BoxGroup("基本属性")]
        [LabelText("FOV变化平滑时间(秒)")]
        [Range(0.00001f, 10)]
        public float DampFOVTime = 0.5f;

        /// <summary>
        /// 将本配置的信息复制到目标配置
        /// </summary>
        /// <param name="targetConfig"></param>
        /// <param name="forceMove">是否强制同步相机位置、角度等信息</param>
        public void ApplyTo(CameraConfigData targetConfig, bool forceMove)
        {
            if (forceMove)
            {
                targetConfig.HorAngle = HorAngle;
                targetConfig.VerAngle = VerAngle;
                targetConfig.ScreenAngle = ScreenAngle;
                targetConfig.FOV = FOV;
                targetConfig.Distance = Distance;
                targetConfig.Offset = Offset;
            }

            targetConfig.DampPosTime = DampPosTime;
            targetConfig.DampRotTime = DampRotTime;
            targetConfig.DampFOVTime = DampFOVTime;
        }
    }

    #region SmoothDamp变量的变化速度、平滑时间、相机和相机墙的约束关系等，勿乱用

    private float tempChangeSpeed_Pos_Distance;
    private Vector3 tempChangeSpeed_Pos_Offset;
    private Vector3 tempChangeSpeed_Pos_TargetPos;
    private float tempChangeSpeed_RotH;
    private float tempChangeSpeed_RotV;
    private float tempChangeSpeed_RotS;
    private float tempChangeSpeed_FOV;

    private void ResetLerpSpeed()
    {
        tempChangeSpeed_Pos_Distance = 0;
        tempChangeSpeed_Pos_Offset = Vector3.zero;
        tempChangeSpeed_Pos_TargetPos = Vector3.zero;
        tempChangeSpeed_RotH = 0;
        tempChangeSpeed_RotV = 0;
        tempChangeSpeed_FOV = 0;
    }

    #endregion

    [LabelText("InGameUISize")]
    public float InGameUISize;

    [LabelText("相机震屏力度曲线(x伤害y强度")]
    public AnimationCurve CameraShakeStrengthCurve;

    [LabelText("相机震屏力度曲线(x距离y衰减")]
    public AnimationCurve CameraShakeAttenuationByDistanceCurve;

    void Awake()
    {
        Distance_Level = 2;
        SetTargetConfigData(TargetConfigData);
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

    private void Update()
    {
        UpdateFOVLevel();
    }

    private void LateUpdate()
    {
        CameraLerp(true);
    }

    [Button("刷新相机位置")]
    public void ForceUpdateCamera()
    {
        CameraLerp(false);
    }

    private void CameraLerp(bool lerp)
    {
        float _fov = lerp ? Mathf.SmoothDamp(CurrentConfigData.FOV, TargetConfigData.FOV, ref tempChangeSpeed_FOV, CurrentConfigData.DampFOVTime, 9999) : TargetConfigData.FOV;
        float _horAngle = lerp ? Mathf.SmoothDamp(CurrentConfigData.HorAngle, TargetConfigData.HorAngle, ref tempChangeSpeed_RotH, CurrentConfigData.DampRotTime, 9999) : TargetConfigData.HorAngle;
        float _verAngle = lerp ? Mathf.SmoothDamp(CurrentConfigData.VerAngle, TargetConfigData.VerAngle, ref tempChangeSpeed_RotV, CurrentConfigData.DampRotTime, 9999) : TargetConfigData.VerAngle;
        float _screenAngle = lerp ? Mathf.SmoothDamp(CurrentConfigData.ScreenAngle, TargetConfigData.ScreenAngle, ref tempChangeSpeed_RotS, CurrentConfigData.DampRotTime, 9999) : TargetConfigData.ScreenAngle;
        Vector3 _offset = lerp ? Vector3.SmoothDamp(CurrentConfigData.Offset, TargetConfigData.Offset, ref tempChangeSpeed_Pos_Offset, CurrentConfigData.DampPosTime, 9999) : (Vector3) TargetConfigData.Offset;
        float _distance = lerp ? Mathf.SmoothDamp(CurrentConfigData.Distance, TargetConfigData.Distance, ref tempChangeSpeed_Pos_Distance, CurrentConfigData.DampPosTime, 9999) : TargetConfigData.Distance;

        // TargetPos计算（不带偏移）
        Vector3 _targetPos = Vector3.zero;
        foreach (Transform trans in targetList)
        {
            _targetPos += trans.position;
        }

        if (targetList.Count != 0)
        {
            _targetPos /= targetList.Count;
        }

        if (lerp) _targetPos = Vector3.SmoothDamp(TargetPos, _targetPos, ref tempChangeSpeed_Pos_TargetPos, CurrentConfigData.DampPosTime, 9999);
        // 以TargetPos为基础，考虑偏移、旋转和相机距离，来计算相机位置和旋转
        CalculatePosAndRot(_targetPos, _horAngle, _verAngle, _screenAngle, _offset, _distance, out Vector3 _cameraPos, out Quaternion _cameraRot);

        // 预计算结束，应用变化
        Camera.fieldOfView = _fov;
        TargetPos = _targetPos;

        CamRootPos = _cameraPos;
        CamRootRot = _cameraRot;

        CurCameraForward = CamRootRot * Vector3.forward;
        CurCameraRight = CamRootRot * Vector3.right;
        CurCameraUp = CamRootRot * Vector3.up;

        // 最终预计算结果反映在面板上
        CurrentConfigData.FOV = _fov;
        CurrentConfigData.HorAngle = _horAngle;
        CurrentConfigData.VerAngle = _verAngle;
        CurrentConfigData.ScreenAngle = _screenAngle;
        CurrentConfigData.Offset = _offset;
        CurrentConfigData.Distance = _distance;
    }

    public void CameraShake(int damage, float distanceFromPlayer)
    {
        Camera.transform.DOShakePosition(0.1f, CameraShakeStrengthCurve.Evaluate(damage) * CameraShakeAttenuationByDistanceCurve.Evaluate(distanceFromPlayer), 10, 90f);
    }

    public void CameraLeftRotate()
    {
        RotateDirection = GridPosR.RotateOrientationClockwise90(RotateDirection);
        TargetConfigData.HorAngle += 90f;
    }

    public void CameraRightRotate()
    {
        RotateDirection = GridPosR.RotateOrientationAntiClockwise90(RotateDirection);
        TargetConfigData.HorAngle -= 90f;
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
                TargetConfigData.Distance = DistanceLevels[_distance_Level];
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
        if (Application.isPlaying)
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
    }

    #endregion

    #region RotateDirections

    private GridPosR.Orientation _rotateDirection = GridPosR.Orientation.Up;

    internal GridPosR.Orientation RotateDirection
    {
        get { return _rotateDirection; }
        set
        {
            if (_rotateDirection != value)
            {
                _rotateDirection = value;
                //Debug.Log(_rotateDirection);
            }
        }
    }

    #endregion

    #region CalculateUtils

    // 相机位置旋转计算核心逻辑
    private void CalculatePosAndRot(Vector3 targetPos, float horAngle, float verAngle, float screenAngle, Vector3 offset, float distance, out Vector3 _cameraPos, out Quaternion _cameraRot)
    {
        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;

        Vector3 cameraForward = Quaternion.AngleAxis(verAngle, right) * forward;
        cameraForward = Quaternion.AngleAxis(horAngle, Vector3.up) * cameraForward;

        Vector3 screenUp = new Vector3(Mathf.Sin(Mathf.Deg2Rad * screenAngle), Mathf.Cos(Mathf.Deg2Rad * screenAngle));
        _cameraRot = Quaternion.LookRotation(cameraForward, screenUp);

        Vector3 cameraRight = _cameraRot * Vector3.right;
        Vector3 cameraUp = _cameraRot * Vector3.up;

        Vector3 offsetTargetPos = targetPos + offset.x * cameraRight + offset.y * cameraUp;

        _cameraPos = offsetTargetPos - distance * cameraForward.normalized;
    }

    private Vector3 GetTargetPosFromCamPos(Vector3 cameraPos, Quaternion cameraRot)
    {
        return cameraPos + cameraRot * Vector3.forward * CurrentConfigData.Distance;
    }

    #endregion

    #region Frustum

    private void RefreshFrustum()
    {
        FrustumWidth = GetFrustumWidth(Camera.fieldOfView, Camera.aspect, CurrentConfigData.HorAngle, CurrentConfigData.Distance);
        FrustumHeight = GetFrustumHeight(Camera.fieldOfView, CurrentConfigData.VerAngle, CurrentConfigData.Distance);
    }

    /// <summary>
    /// 获取z=0 plane跟视锥相交的宽
    /// </summary>
    private static float GetFrustumWidth(float fov, float aspect, float horAngle, float distance)
    {
        float fovRad = fov * Mathf.Deg2Rad;
        float hfov = 2 * Mathf.Atan(Mathf.Tan(fovRad / 2) * aspect) * Mathf.Rad2Deg;
        return GetViewSize(hfov, horAngle, distance);
    }

    /// <summary>
    /// 获取z=0 plane跟视锥相交的高
    /// </summary>
    private static float GetFrustumHeight(float fov, float verAngle, float distance)
    {
        return GetViewSize(fov, verAngle, distance);
    }

    private static float GetViewSize(float viewAngle, float deltaAngle, float distance)
    {
        float retVal = 0.0f;

        // 计算视点垂直点上方的长度
        if (deltaAngle > viewAngle * 0.5f)
        {
            float verDis = distance * Mathf.Cos(deltaAngle * Mathf.Deg2Rad);

            float degTop = deltaAngle - viewAngle * 0.5f;
            float deg = degTop + viewAngle;
            float verZPlaneDis = verDis * Mathf.Tan(deg * Mathf.Deg2Rad);

            float topDis = verDis * Mathf.Tan(degTop * Mathf.Deg2Rad);

            retVal = verZPlaneDis - topDis;
        }
        else
        {
            float verDis = distance * Mathf.Cos(deltaAngle * Mathf.Deg2Rad);

            // 计算垂直点 到 视锥下射线到zplane的距离
            float degTop = viewAngle * 0.5f - deltaAngle;
            float deg = deltaAngle + viewAngle * 0.5f;
            float verZPlaneDis = verDis * Mathf.Tan(deg * Mathf.Deg2Rad);

            float topDis = verDis * Mathf.Tan(degTop * Mathf.Deg2Rad);

            retVal = verZPlaneDis + topDis;
        }

        return retVal;
    }

    public bool IsInVisibleFrustum(Vector3 point)
    {
        Vector3 viewportPoint = Camera.WorldToViewportPoint(point);
        return viewportPoint.x >= 0.0f && viewportPoint.x <= 1.0f
                                       && viewportPoint.y >= 0.0f && viewportPoint.y <= 1.0f
                                       && viewportPoint.z >= Camera.nearClipPlane && viewportPoint.z <= Camera.farClipPlane;
    }

    #endregion
}