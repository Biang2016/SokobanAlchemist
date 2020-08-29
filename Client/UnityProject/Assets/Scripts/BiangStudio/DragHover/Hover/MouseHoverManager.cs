using System;
using System.Collections.Generic;
using BiangStudio.Singleton;
using UnityEngine;

namespace BiangStudio.DragHover
{
    /// <summary>
    /// 鼠标悬停管理器
    /// </summary>
    public class MouseHoverManager : TSingletonBaseManager<MouseHoverManager>
    {
        public MouseHoverManager()
        {
        }

        public delegate bool HoverPressKeyDelegate();

        public delegate Vector2 GetScreenMousePositionDelegate();

        internal HoverPressKeyDelegate KeyDownHandler;
        internal HoverPressKeyDelegate KeyUpHandler;
        internal GetScreenMousePositionDelegate GetMousePositionHandler;

        private List<HoverActionBase> HoverActions = new List<HoverActionBase>();

        public override void Awake()
        {
            M_StateMachine = new StateMachine();
        }

        public void Initialize(HoverPressKeyDelegate keyDownHandler, HoverPressKeyDelegate keyUpHandler, GetScreenMousePositionDelegate getMousePositionHandler)
        {
            KeyDownHandler = keyDownHandler;
            KeyUpHandler = keyUpHandler;
            GetMousePositionHandler = getMousePositionHandler;
        }

        public void AddHoverAction(HoverActionBase hoverAction)
        {
            HoverActions.Add(hoverAction);
        }

        public override void Start()
        {
        }

        public override void Update(float deltaTime)
        {
            M_StateMachine.Update();
        }

        public StateMachine M_StateMachine;

        public class StateMachine
        {
            public StateMachine()
            {
                state = States.None;
                previousState = States.None;
            }

            [Flags]
            public enum States
            {
                None = 0, //禁用
                StartMenu = 1, //开始界面
                ExitMenu = 1 << 1, //Exit菜单
                SettingMenu = 1 << 2, //Setting菜单
                BattleInventory = 1 << 3, //背包物品
                UI = StartMenu | ExitMenu | SettingMenu, //UI
            }

            private States state;
            private States previousState;

            public void SetState(States newState)
            {
                if (state != newState)
                {
                    foreach (HoverActionBase hoverAction in Instance.HoverActions)
                    {
                        if ((newState & hoverAction.State) == 0)
                        {
                            hoverAction.Release();
                        }
                    }

                    previousState = state;
                    state = newState;
                    Debug.Log("MouseHoverManager state: " + state);
                }
            }

            public void ReturnToPreviousState()
            {
                SetState(previousState);
            }

            public States GetState()
            {
                return state;
            }

            public void Update()
            {
                foreach (HoverActionBase hoverAction in Instance.HoverActions)
                {
                    if ((state & hoverAction.State) != 0)
                    {
                        hoverAction.Check();
                    }
                }
            }
        }

        public abstract class HoverActionBase
        {
            protected HoverActionBase(int layerMask, StateMachine.States state, Camera camera)
            {
                State = state;
                LayerMask = layerMask;
                Camera = camera;
            }

            public StateMachine.States State;
            protected int LayerMask;
            protected Camera Camera;

            protected MouseHoverComponent currentTarget; //当前目标

            public abstract void Check();
            public abstract void Release();
        }

        //判定鼠标按下时的Hover，立即生效
        public class PressHoverImmediately<T> : HoverActionBase where T : MonoBehaviour
        {
            public PressHoverImmediately(int layerMask, StateMachine.States state, Camera camera) : base(layerMask, state, camera)
            {
            }

            public override void Check()
            {
                if (Instance.KeyDownHandler())
                {
                    Ray ray = Camera.ScreenPointToRay(Instance.GetMousePositionHandler());
                    RaycastHit raycast;
                    Physics.Raycast(ray, out raycast, 10f, LayerMask);
                    Debug.DrawLine(ray.origin, ray.origin + 10 * ray.direction.normalized, Color.yellow);
                    if (raycast.collider != null)
                    {
                        MouseHoverComponent mouseHoverComponent = raycast.collider.gameObject.GetComponentInParent<MouseHoverComponent>();
                        if (mouseHoverComponent)
                        {
                            if (mouseHoverComponent.GetComponent<T>())
                            {
                                if (currentTarget && currentTarget != mouseHoverComponent)
                                {
                                    Release();
                                }
                            }

                            currentTarget = mouseHoverComponent;
                            currentTarget.IsOnPressHover = true;
                        }
                        else
                        {
                            if (currentTarget)
                            {
                                Release();
                            }
                        }
                    }
                    else
                    {
                        if (currentTarget)
                        {
                            Release();
                        }
                    }
                }
                else
                {
                    if (currentTarget)
                    {
                        Release();
                    }
                }
            }

            public override void Release()
            {
                if (currentTarget)
                {
                    currentTarget.IsOnPressHover = false;
                    currentTarget = null;
                }
            }
        }

        //判定鼠标未按下时的Hover，停留一定时间生效
        public class Hover<T> : HoverActionBase where T : MonoBehaviour
        {
            Vector3 mouseLastPosition;
            private float mouseStopTimeTicker = 0;
            private bool needCheckSpeed = false;

            public Hover(int layerMask, StateMachine.States state, Camera camera, float delaySeconds = 0f, float mouseSpeedThreshold = -1) : base(layerMask, state, camera)
            {
                DelaySeconds = delaySeconds;
                needCheckSpeed = !mouseSpeedThreshold.Equals(-1f);
                MouseSpeedThreshold = mouseSpeedThreshold;
            }

            private float DelaySeconds;
            private float MouseSpeedThreshold;

            public override void Check()
            {
                if (!Instance.KeyDownHandler())
                {
                    Vector3 mouseCurrentPosition = Instance.GetMousePositionHandler.Invoke();
                    if (needCheckSpeed && (mouseCurrentPosition - mouseLastPosition).magnitude / Time.deltaTime > MouseSpeedThreshold)
                    {
                        //鼠标过快移动
                        mouseStopTimeTicker = 0;
                        mouseLastPosition = mouseCurrentPosition;
                        if (currentTarget)
                        {
                            Release();
                        }

                        return;
                    }
                    else
                    {
                        mouseLastPosition = mouseCurrentPosition;
                        mouseStopTimeTicker += Time.deltaTime;
                        if (mouseStopTimeTicker > DelaySeconds)
                        {
                            mouseStopTimeTicker = 0;
                            Ray ray = Camera.ScreenPointToRay(Instance.GetMousePositionHandler());
                            RaycastHit raycast;
                            Physics.Raycast(ray, out raycast, 10f, LayerMask);
                            Debug.DrawLine(ray.origin, ray.origin + 10 * ray.direction.normalized, Color.red, 1f);
                            if (raycast.collider != null)
                            {
                                MouseHoverComponent mouseHoverComponent = raycast.collider.gameObject.GetComponentInParent<MouseHoverComponent>();
                                if (mouseHoverComponent && mouseHoverComponent.GetComponent<T>())
                                {
                                    if (currentTarget && currentTarget != mouseHoverComponent)
                                    {
                                        Release();
                                    }

                                    currentTarget = mouseHoverComponent;
                                    currentTarget.IsOnHover = true;
                                }
                                else
                                {
                                    if (currentTarget)
                                    {
                                        Release();
                                    }
                                }
                            }
                            else
                            {
                                if (currentTarget)
                                {
                                    Release();
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (currentTarget)
                    {
                        Release();
                    }
                }
            }

            public override void Release()
            {
                if (currentTarget)
                {
                    currentTarget.IsOnHover = false;
                    currentTarget = null;
                }
            }
        }

        //判定鼠标的Focus，停留一定时间生效
        public class Focus<T> : HoverActionBase where T : MonoBehaviour
        {
            Vector3 mouseLastPosition;
            private float mouseStopTimeTicker = 0;
            private bool needCheckSpeed = false;

            public Focus(int layerMask, StateMachine.States state, Camera camera, float delaySeconds, float mouseSpeedThreshold) : base(layerMask, state, camera)
            {
                DelaySeconds = delaySeconds;
                needCheckSpeed = !mouseSpeedThreshold.Equals(-1f);
                MouseSpeedThreshold = mouseSpeedThreshold;
            }

            private float DelaySeconds;
            private float MouseSpeedThreshold;

            public override void Check()
            {
                Vector3 mouseCurrentPosition = Instance.GetMousePositionHandler();
                if (needCheckSpeed && (mouseCurrentPosition - mouseLastPosition).magnitude / Time.deltaTime > MouseSpeedThreshold)
                {
                    //鼠标过快移动
                    mouseStopTimeTicker = 0;
                    mouseLastPosition = mouseCurrentPosition;
                    if (currentTarget)
                    {
                        Release();
                    }

                    return;
                }
                else
                {
                    mouseLastPosition = mouseCurrentPosition;
                    mouseStopTimeTicker += Time.deltaTime;
                    if (mouseStopTimeTicker > DelaySeconds)
                    {
                        mouseStopTimeTicker = 0;
                        Ray ray = Camera.ScreenPointToRay(Instance.GetMousePositionHandler());
                        RaycastHit raycast;
                        Physics.Raycast(ray, out raycast, 10f, LayerMask);
                        Debug.DrawLine(ray.origin, ray.origin + 10 * ray.direction.normalized, Color.red, 1f);
                        if (raycast.collider != null)
                        {
                            MouseHoverComponent mouseHoverComponent = raycast.collider.gameObject.GetComponentInParent<MouseHoverComponent>();
                            if (mouseHoverComponent && mouseHoverComponent.GetComponent<T>())
                            {
                                if (currentTarget && currentTarget != mouseHoverComponent)
                                {
                                    Release();
                                }

                                currentTarget = mouseHoverComponent;
                                currentTarget.IsOnFocus = true;
                            }
                            else
                            {
                                if (currentTarget)
                                {
                                    Release();
                                }
                            }
                        }
                        else
                        {
                            if (currentTarget)
                            {
                                Release();
                            }
                        }
                    }
                }
            }

            public override void Release()
            {
                if (currentTarget)
                {
                    currentTarget.IsOnFocus = false;
                    currentTarget = null;
                }
            }

            public void PrintCurrentTarget()
            {
                Debug.Log(currentTarget);
            }
        }
    }
}