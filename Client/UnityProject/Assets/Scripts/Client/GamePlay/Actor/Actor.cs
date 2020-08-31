using System.Collections.Generic;
using System.Linq;
using BiangStudio.GameDataFormat.Grid;
using BiangStudio.ObjectPool;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class Actor : PoolObject
{
    public Rigidbody RigidBody;
    public ActorPushHelper ActorPushHelper;
    public ActorFaceHelper ActorFaceHelper;
    public ActorSkinHelper ActorSkinHelper;

    [DisplayAsString]
    [LabelText("移动倾向")]
    public Vector3 CurMoveAttempt;

    [DisplayAsString]
    [LabelText("世界坐标")]
    public GridPos3D CurGP;

    [LabelText("阵营")]
    public Camp Camp;

    [LabelText("踢箱子力量")]
    public float KickForce = 5;

    [LabelText("起步速度")]
    public float Accelerate = 10f;

    [LabelText("移动速度")]
    public float MoveSpeed = 10f;

    [LabelText("最大速度阻力")]
    public float Drag = 10f;

    private List<SmoothMove> SmoothMoves = new List<SmoothMove>();

    public bool IsPlayer => Camp == Camp.Player;

    void Awake()
    {
        SmoothMoves = GetComponentsInChildren<SmoothMove>().ToList();
        foreach (SmoothMove sm in SmoothMoves)
        {
            sm.enabled = false;
        }

        ClientGameManager.Instance.BattleMessenger.AddListener<Actor>((uint) Enum_Events.OnPlayerLoaded, OnLoaded);
    }

    public void OnLoaded(Actor actor)
    {
        if (actor == this)
        {
            foreach (SmoothMove sm in SmoothMoves)
            {
                sm.enabled = true;
            }
        }
    }

    public void SetShown(bool shown)
    {
    }

    void FixedUpdate()
    {
        CurGP = GridPos3D.GetGridPosByTrans(transform, 1);
    }

    public void Kick()
    {
        foreach (Box box in ActorFaceHelper.FacingBoxList)
        {
            box.Kick(transform.forward, KickForce);
        }

        ActorFaceHelper.FacingBoxList.Clear();
    }
}