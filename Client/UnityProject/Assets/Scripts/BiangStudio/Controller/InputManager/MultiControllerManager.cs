using System;
using System.Collections.Generic;
using System.Linq;
using BiangStudio.Singleton;
using UnityEngine.Events;

namespace BiangStudio.Controller
{
    public class MultiControllerManager : MonoSingleton<MultiControllerManager>
    {
        public Dictionary<PlayerNumber, ControllerIndex> PlayerControllerMap = new Dictionary<PlayerNumber, ControllerIndex>();
        public Dictionary<ControllerIndex, Controller> Controllers = new Dictionary<ControllerIndex, Controller>();

        public UnityAction<ControllerIndex> OnAddPlayer;

        void Awake()
        {
            Init();
        }

        public void Init()
        {
            PlayerControllerMap.Clear();
            Controllers.Clear();
            foreach (object o in Enum.GetValues(typeof(ControllerIndex)))
            {
                ControllerIndex ci = (ControllerIndex) o;
                if ((int) ci <= 4)
                {
                    Controllers.Add(ci, new XBoxController());
                }
                else
                {
                    Controllers.Add(ci, new KeyBoardController());
                }

                Controllers[ci].Init(ci);
            }
        }

        private void Update()
        {
            foreach (KeyValuePair<ControllerIndex, Controller> kv in Controllers)
            {
                kv.Value.Update();
            }

            foreach (object o in Enum.GetValues(typeof(ControllerIndex)))
            {
                ControllerIndex ci = (ControllerIndex) o;
                Controller c = Controllers[ci];
                if (c.ButtonDown[c.AddPlayerButton])
                {
                    if (!PlayerControllerMap.Values.Contains(ci))
                    {
                        OnAddPlayer?.Invoke(ci);
                    }
                }
            }
        }
    }
}