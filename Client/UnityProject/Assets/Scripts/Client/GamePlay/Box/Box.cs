using System;
using System.Linq;
using BiangStudio.GameDataFormat.Grid;
using BiangStudio.ObjectPool;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;

#endif

public class Box : PoolObject
{
    private BoxUnderWorldModuleDesignerClamper BoxUnderWorldModuleDesignerClamper;
    private GridSnapper GridSnapper;
    public Collider StaticCollider;
    public Collider DynamicCollider;
    internal Rigidbody Rigidbody;

    internal Actor LastTouchActor;
    internal BoxEffectHelper BoxEffectHelper;

    internal bool ArtOnly;

    public override void OnUsed()
    {
        LastTouchActor = null;
        ArtOnly = true;
        base.OnUsed();
    }

    public override void OnRecycled()
    {
        ArtOnly = true;
        WorldModule = null;
        GridPos3D = GridPos3D.Zero;
        lastGP = GridPos3D.Zero;
        State = States.Static;
        BoxEffectHelper?.PoolRecycle();
        BoxEffectHelper = null;
        transform.DOPause();
        if (Rigidbody) Destroy(Rigidbody);
        if (LastTouchActor != null && LastTouchActor.CurrentLiftBox == this)
        {
            LastTouchActor.ThrowState = Actor.ThrowStates.None;
            LastTouchActor.CurrentLiftBox = null;
            LastTouchActor = null;
        }

        base.OnRecycled();
    }

    [HideInInspector]
    public byte BoxTypeIndex;

    [LabelText("箱子特性")]
    [AssetsOnly]
    public BoxFeature BoxFeature;

    [AssetsOnly]
    [ShowIf("Moveable")]
    [BoxGroup("箱子属性")]
    [LabelText("重量")]
    [SerializeField]
    private float Weight;

    public float FinalWeight => Weight * ConfigManager.BoxWeightFactor_Cheat;

    [AssetsOnly]
    [ShowIf("KickOrLiftable")]
    [BoxGroup("箱子属性")]
    [LabelText("撞击特效")]
    [SerializeField]
    private ProjectileType CollideFX;

    [AssetsOnly]
    [ShowIf("KickOrLiftable")]
    [BoxGroup("箱子属性")]
    [LabelText("撞击特效尺寸")]
    [SerializeField]
    private float CollideFXScale = 1f;

    [AssetsOnly]
    [ShowIf("Pushable")]
    [BoxGroup("推箱子属性")]
    [LabelText("抗推力")]
    [PropertyRange(0, 1)]
    public float Static_Inertia = 0.5f;

    [AssetsOnly]
    [ShowIf("Liftable")]
    [BoxGroup("扔箱子属性")]
    [LabelText("扔毁灭伤害半径")]
    [SerializeField]
    private float DestroyDamageRadius_Throw = 1.5f;

    [AssetsOnly]
    [ShowIf("Liftable")]
    [BoxGroup("扔箱子属性")]
    [LabelText("扔毁灭伤害")]
    [SerializeField]
    private float DestroyDamage_Throw = 3f;

    [AssetsOnly]
    [ShowIf("Liftable")]
    [BoxGroup("扔箱子属性")]
    [LabelText("落地摩阻力")]
    public float Throw_Drag = 0.5f;

    [AssetsOnly]
    [ShowIf("Kickable")]
    [BoxGroup("踢箱子属性")]
    [LabelText("摩阻力")]
    public float Dynamic_Drag = 0.5f;

    [AssetsOnly]
    [ShowIf("Kickable")]
    [BoxGroup("踢箱子属性")]
    [LabelText("踢毁灭伤害半径")]
    [SerializeField]
    private float DestroyDamageRadius_Kick = 0.5f;

    [AssetsOnly]
    [ShowIf("Kickable")]
    [BoxGroup("踢箱子属性")]
    [LabelText("踢毁灭伤害")]
    [SerializeField]
    private float DestroyDamage_Kick = 3f;

    private GridPos3D lastGP;

    [HideInEditorMode]
    public GridPos3D GridPos3D;

    [HideInEditorMode]
    public GridPos3D LocalGridPos3D;

    [HideInEditorMode]
    public WorldModule WorldModule;

    public enum States
    {
        Static,
        BeingPushed,
        PushingCanceling,
        BeingKicked,
        BeingLift,
        Lifted,
        Flying,
    }

    [HideInPrefabAssets]
    [ReadOnly]
    [LabelText("移动状态")]
    public States State = States.Static;

    protected virtual void Awake()
    {
        GridSnapper = GetComponent<GridSnapper>();
        BoxUnderWorldModuleDesignerClamper = GetComponent<BoxUnderWorldModuleDesignerClamper>();
        BoxUnderWorldModuleDesignerClamper.enabled = !Application.isPlaying;
        GridSnapper.enabled = false;
    }

    protected virtual void Start()
    {
    }

    public void Initialize(GridPos3D localGridPos3D, WorldModule module, float lerpTime, bool artOnly, bool dropping)
    {
        if (dropping)
        {
            if (StaticCollider) StaticCollider.enabled = false;
            if (DynamicCollider) DynamicCollider.enabled = false;
        }

        ArtOnly = artOnly;
        StaticCollider.gameObject.SetActive(!artOnly);
        DynamicCollider.gameObject.SetActive(!artOnly);

        LastTouchActor = null;
        lastGP = GridPos3D;
        WorldModule = module;
        GridPos3D = localGridPos3D + module.ModuleGP * WorldModule.MODULE_SIZE;
        LocalGridPos3D = localGridPos3D;
        transform.parent = module.transform;
        if (lerpTime > 0)
        {
            transform.DOPause();
            transform.DOLocalMove(localGridPos3D.ToVector3(), lerpTime).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                State = States.Static;
                if (dropping)
                {
                    if (StaticCollider) StaticCollider.enabled = true;
                    if (DynamicCollider) DynamicCollider.enabled = true;
                }
            });
            transform.DOLocalRotate(Vector3.zero, lerpTime);
            State = States.BeingPushed;
        }
        else
        {
            transform.localPosition = localGridPos3D.ToVector3();
            transform.localRotation = Quaternion.identity;
            State = States.Static;
            if (dropping)
            {
                if (StaticCollider) StaticCollider.enabled = true;
                if (DynamicCollider) DynamicCollider.enabled = true;
            }
        }

        WorldManager.Instance.CurrentWorld.CheckDropSelf(this);
    }

    public void Push(Vector3 direction)
    {
        if (State == States.Static || State == States.PushingCanceling)
        {
            Vector3 targetPos = GridPos3D.ToVector3() + direction.normalized;
            GridPos3D gp = GridPos3D.GetGridPosByPoint(targetPos, 1);
            if (gp != GridPos3D)
            {
                WorldManager.Instance.CurrentWorld.MoveBox(GridPos3D, gp, States.BeingPushed);
            }
        }
    }

    public void PushCanceled()
    {
        if (State == States.BeingPushed)
        {
            if ((transform.localPosition - LocalGridPos3D.ToVector3()).magnitude > (1 - Static_Inertia))
            {
                WorldManager.Instance.CurrentWorld.MoveBox(GridPos3D, lastGP, States.PushingCanceling);
            }
        }
    }

    public void Kick(Vector3 direction, float force, Actor actor)
    {
        if (State == States.BeingPushed || State == States.Flying || State == States.BeingKicked || State == States.Static || State == States.PushingCanceling)
        {
            if (BoxEffectHelper == null)
            {
                BoxEffectHelper = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.BoxEffectHelper].AllocateGameObject<BoxEffectHelper>(transform);
            }

            LastTouchActor = actor;
            WorldManager.Instance.CurrentWorld.RemoveBox(this);
            State = States.BeingKicked;
            transform.DOPause();
            StaticCollider.enabled = false;
            DynamicCollider.enabled = true;
            Rigidbody = gameObject.GetComponent<Rigidbody>();
            if (!Rigidbody) Rigidbody = gameObject.AddComponent<Rigidbody>();
            Rigidbody.mass = FinalWeight;
            Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            Rigidbody.drag = Dynamic_Drag * ConfigManager.BoxKickDragFactor_Cheat;
            Rigidbody.angularDrag = 0;
            Rigidbody.useGravity = true;
            Rigidbody.velocity = direction.normalized * 1.1f;
            Rigidbody.angularVelocity = Vector3.zero;
            Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            if (direction.x.Equals(0)) Rigidbody.constraints |= RigidbodyConstraints.FreezePositionX;
            if (direction.z.Equals(0)) Rigidbody.constraints |= RigidbodyConstraints.FreezePositionZ;
            Rigidbody.AddForce(direction.normalized * force);
            transform.position = transform.position.ToGridPos3D().ToVector3();
        }
    }

    public bool BeingLift(Actor actor)
    {
        if (State == States.BeingPushed || State == States.Flying || State == States.BeingKicked || State == States.Static || State == States.PushingCanceling)
        {
            LastTouchActor = actor;
            WorldManager.Instance.CurrentWorld.RemoveBox(this);
            State = States.BeingLift;
            transform.DOPause();
            StaticCollider.enabled = true;
            DynamicCollider.enabled = false;
            if (Rigidbody)
            {
                DestroyImmediate(Rigidbody);
            }

            BoxEffectHelper?.PoolRecycle();
            BoxEffectHelper = null;
            return true;
        }

        return false;
    }

    public void Throw(Vector3 direction, float velocity, Actor actor)
    {
        if (State == States.Lifted)
        {
            if (BoxEffectHelper == null)
            {
                BoxEffectHelper = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.BoxEffectHelper].AllocateGameObject<BoxEffectHelper>(transform);
            }

            LastTouchActor = actor;
            State = States.Flying;
            transform.DOPause();
            transform.parent = WorldManager.Instance.CurrentWorld.transform;
            StaticCollider.enabled = false;
            DynamicCollider.enabled = true;
            Rigidbody = gameObject.GetComponent<Rigidbody>();
            if (!Rigidbody) Rigidbody = gameObject.AddComponent<Rigidbody>();
            Rigidbody.mass = FinalWeight;
            Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            Rigidbody.drag = 0;
            Rigidbody.angularDrag = 0;
            Rigidbody.useGravity = true;
            Rigidbody.velocity = direction.normalized * velocity;
            Rigidbody.angularVelocity = Vector3.zero;
            Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        }
    }

    void FixedUpdate()
    {
        if ((State == States.BeingKicked || State == States.Flying) && Rigidbody && Rigidbody.velocity.magnitude < 1f)
        {
            DestroyImmediate(Rigidbody);
            StaticCollider.enabled = true;
            DynamicCollider.enabled = false;
            WorldManager.Instance.CurrentWorld.BoxReturnToWorldFromPhysics(this);
            BoxEffectHelper?.PoolRecycle();
            BoxEffectHelper = null;
        }

        if (Rigidbody && Rigidbody.velocity.magnitude > 1f)
        {
            BoxEffectHelper?.Play();
        }
        else
        {
            BoxEffectHelper?.Stop();
        }
    }

    public bool Pushable => BoxFeature.HasFlag(BoxFeature.Pushable);

    public bool Kickable => BoxFeature.HasFlag(BoxFeature.Kickable);

    public bool Liftable => BoxFeature.HasFlag(BoxFeature.Liftable);

    public bool KickOrLiftable => Kickable || Liftable;

    public bool Moveable => Pushable || Kickable || Liftable;

    public bool Droppable => BoxFeature.HasFlag(BoxFeature.Droppable);

    public bool Breakable => BoxFeature.HasFlag(BoxFeature.ThrowHitBreakable) || BoxFeature.HasFlag(BoxFeature.KickHitBreakable);

    void OnCollisionEnter(Collision collision)
    {
        if (LastTouchActor != null && collision.gameObject == LastTouchActor.gameObject) return;
        if (State == States.Flying)
        {
            // Destroy AOE Damage
            if (!WorldManager.Instance.CurrentWorld.WorldData.WorldFeature.HasFlag(WorldFeature.PlayerImmune))
            {
                Collider[] colliders = Physics.OverlapSphere(transform.position, DestroyDamageRadius_Throw, LayerManager.Instance.LayerMask_HitBox_Player | LayerManager.Instance.LayerMask_HitBox_Enemy);
                HashSet<Actor> damagedActors = new HashSet<Actor>();
                foreach (Collider collider in colliders)
                {
                    Actor actor = collider.GetComponentInParent<Actor>();
                    if (actor && actor != LastTouchActor && actor.IsOpponent(LastTouchActor) && actor.ActorBattleHelper && !damagedActors.Contains(actor))
                    {
                        actor.ActorBattleHelper.Damage(DestroyDamage_Throw);
                        damagedActors.Add(actor);
                    }
                }
            }

            // FX and Recycle
            PlayDestroyFX();
            if (BoxFeature.HasFlag(BoxFeature.ThrowHitBreakable))
            {
                PoolRecycle();
            }
            else
            {
                Box box = collision.gameObject.GetComponentInParent<Box>();
                if (box && !box.BoxFeature.HasFlag(BoxFeature.IsBorder))
                {
                    Rigidbody.drag = Throw_Drag * ConfigManager.BoxThrowDragFactor_Cheat;
                }
            }
        }

        if (State == States.BeingKicked)
        {
            if (collision.gameObject.layer != LayerManager.Instance.Layer_Ground)
            {
                // Destroy AOE Damage
                if (!WorldManager.Instance.CurrentWorld.WorldData.WorldFeature.HasFlag(WorldFeature.PlayerImmune))
                {
                    Collider[] colliders = Physics.OverlapSphere(transform.position, DestroyDamageRadius_Kick, LayerManager.Instance.LayerMask_HitBox_Player | LayerManager.Instance.LayerMask_HitBox_Enemy);
                    HashSet<Actor> damagedActors = new HashSet<Actor>();
                    foreach (Collider collider in colliders)
                    {
                        Actor actor = collider.GetComponentInParent<Actor>();
                        if (actor && actor != LastTouchActor && actor.IsOpponent(LastTouchActor) && actor.ActorBattleHelper && !damagedActors.Contains(actor))
                        {
                            actor.ActorBattleHelper.Damage(DestroyDamage_Kick);
                            damagedActors.Add(actor);
                        }
                    }
                }

                // FX and Recycle
                PlayDestroyFX();
                if (BoxFeature.HasFlag(BoxFeature.KickHitBreakable))
                {
                    PoolRecycle();
                }
            }
        }
    }

    public void PlayDestroyFX()
    {
        ProjectileHit hit = ProjectileManager.Instance.PlayProjectileHit(CollideFX, transform.position);
        if (hit) hit.transform.localScale = Vector3.one * CollideFXScale;
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            Gizmos.color = new Color(0.2f, 0.2f, 0.2f, 0.1f);
            Gizmos.DrawCube(transform.position, Vector3.one);
        }
    }

#if UNITY_EDITOR
    [HideInPlayMode]
    [HideInPrefabAssets]
    [BoxGroup("模组编辑器")]
    [Button("替换Box", ButtonSizes.Large)]
    [GUIColor(0f, 1f, 1f)]
    private void ReplaceBox_Editor()
    {
        WorldModuleDesignHelper module = GetComponentInParent<WorldModuleDesignHelper>();
        if (!module)
        {
            Debug.LogError("此功能只能在模组编辑器中使用");
            return;
        }

        WorldDesignHelper world = GetComponentInParent<WorldDesignHelper>();
        if (world)
        {
            Debug.LogError("此功能只能在模组编辑器中使用");
            return;
        }

        GameObject prefab = (GameObject) Resources.Load("Prefabs/Designs/Box/" + ReplaceBoxTypeName);
        GameObject go = (GameObject) PrefabUtility.InstantiatePrefab(prefab, transform.parent);
        go.transform.position = transform.position;
        go.transform.rotation = Quaternion.identity;
        DestroyImmediate(gameObject);
    }

    [HideInPlayMode]
    [HideInPrefabAssets]
    [ShowInInspector]
    [NonSerialized]
    [BoxGroup("模组编辑器")]
    [LabelText("替换Box类型")]
    [ValueDropdown("GetAllBoxTypeNames")]
    private string ReplaceBoxTypeName;

    private IEnumerable<string> GetAllBoxTypeNames()
    {
        ConfigManager.LoadAllConfigs();
        List<string> res = ConfigManager.BoxTypeDefineDict.TypeIndexDict.Keys.ToList();
        return res;
    }
#endif
}

[Flags]
public enum BoxFeature
{
    None = 0,

    [LabelText("可推动")]
    Pushable = 1 << 0,

    [LabelText("可踢")]
    Kickable = 1 << 1,

    [LabelText("可扔")]
    Liftable = 1 << 2,

    [LabelText("会塌落")]
    Droppable = 1 << 3,

    [LabelText("ChessKing")]
    ChessKing = 1 << 4,

    [LabelText("从属玩家1")]
    BelongToPlayer1 = 1 << 5,

    [LabelText("从属玩家2")]
    BelongToPlayer2 = 1 << 6,

    [LabelText("踢撞会碎")]
    KickHitBreakable = 1 << 7,

    [LabelText("扔撞会碎")]
    ThrowHitBreakable = 1 << 8,

    [LabelText("墙壁")]
    IsBorder = 1 << 9,
}