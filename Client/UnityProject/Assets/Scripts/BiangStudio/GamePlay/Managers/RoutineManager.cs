using System;
using System.Collections.Generic;
using BiangStudio.Singleton;

namespace BiangStudio.GamePlay
{
    public class RoutineManager : TSingletonBaseManager<RoutineManager>
    {
        public delegate void LogError(string log);

        public LogError LogErrorHandler;

        public delegate void TimerDelegate();

        public delegate List<bool> CheckConditionDelegate();

        public delegate void CheckCallBackDelegate();

        public delegate void CountDownDelegate();

        public class TimerRoutine
        {
            public float Interval;
            public float RemainTime;
            public bool Repeat;
            public bool Destroyed;
            public TimerDelegate Handler;
        }

        public class FrameRoutine
        {
            public int Interval;
            public int RemainFrame;
            public bool Repeat;
            public bool Destroyed;
            public TimerDelegate Handler;
        }

        public class ConditionalRoutine
        {
            public float CheckTimeInterval;
            public float CheckRemainTime;
            public CheckConditionDelegate CheckConditionHandler;
            public CheckCallBackDelegate CheckCallBackHandler;
            public List<bool> StatusList = new List<bool>();
        }

        public class CountDownRoutine
        {
            public long CallBackTime;
            public uint TokenID;
            public CountDownDelegate CountDownCallBack;
        }

        public List<TimerRoutine> TimerRoutineList = new List<TimerRoutine>();
        public List<FrameRoutine> FrameRoutineList = new List<FrameRoutine>();
        public List<ConditionalRoutine> ConditionalRoutineList = new List<ConditionalRoutine>();
        public List<CountDownRoutine> CountDownRoutineList = new List<CountDownRoutine>();
        private long currentTimeTick;

        public void Update(float deltaTime, int frameCount)
        {
            currentTimeTick = DateTime.Now.Ticks;

            UpdateTimerRoutine(deltaTime);
            UpdateFrameRoutine();
            UpdateConditionCallBack(deltaTime);
            UpdateCountDownRoutine(deltaTime);
        }

        public void Clear()
        {
            TimerRoutineList?.Clear();
            FrameRoutineList?.Clear();
            ConditionalRoutineList?.Clear();
            CountDownRoutineList?.Clear();
            timeCountDownTokenID = 0;
        }

        #region TimerRoutine

        public void AddTimerRoutine(TimerDelegate handler, float delay, float interval, bool repeat)
        {
            if (handler == null) return;
            if (repeat && interval < 0.0f) return;
            TimerRoutine timerRoutine = new TimerRoutine
            {
                Interval = interval,
                RemainTime = delay,
                Repeat = repeat,
                Destroyed = false,
                Handler = handler,
            };
            TimerRoutineList.Add(timerRoutine);
        }

        public void RemoveTimerRoutine(TimerDelegate handler, bool rightNow = false, bool removeAll = false)
        {
            foreach (TimerRoutine t in TimerRoutineList)
            {
                if (t.Handler == handler)
                {
                    t.Destroyed = true;
                    if (!removeAll) return;
                }
            }
        }

        private void UpdateTimerRoutine(float deltaTime)
        {
            foreach (TimerRoutine t in TimerRoutineList)
            {
                if (t.Destroyed) continue;

                t.RemainTime -= deltaTime;

                if (t.RemainTime > 0.0f) continue;

                try
                {
                    t.Handler?.Invoke();
                }
                catch (Exception e)
                {
                    LogErrorHandler?.Invoke(e.ToString());
                    t.Destroyed = true;
                }

                if (t.Repeat)
                {
                    t.RemainTime += t.Interval;
                }
                else
                {
                    t.Destroyed = true;
                }
            }

            for (int i = 0; i < TimerRoutineList.Count;)
            {
                if (TimerRoutineList[i].Destroyed)
                {
                    TimerRoutineList.RemoveAt(i);
                }
                else
                {
                    ++i;
                }
            }
        }

        #endregion

        #region FrameRoutine

        public void AddFrameRoutine(int delay, int interval, bool repeat, TimerDelegate handler)
        {
            if (interval < 0 || handler == null) return;

            FrameRoutine frameRoutine = new FrameRoutine
            {
                Interval = interval,
                RemainFrame = delay,
                Repeat = repeat,
                Destroyed = false,
                Handler = handler,
            };

            FrameRoutineList.Add(frameRoutine);
        }

        public void RemoveFrameRoutine(TimerDelegate handler, bool rightNow = false)
        {
            foreach (FrameRoutine t in FrameRoutineList)
            {
                if (t.Handler == handler)
                {
                    t.Destroyed = true;
                }
            }
        }

        private void UpdateFrameRoutine()
        {
            foreach (FrameRoutine t in FrameRoutineList)
            {
                if (t.Destroyed) continue;

                t.RemainFrame--;
                if (t.RemainFrame > 0) continue;

                try
                {
                    t.Handler?.Invoke();
                }
                catch (Exception e)
                {
                    LogErrorHandler?.Invoke(e.ToString());
                    t.Destroyed = true;
                }

                if (t.Repeat)
                {
                    t.RemainFrame = t.Interval;
                }
                else
                {
                    t.Destroyed = true;
                }
            }

            for (int i = 0; i < FrameRoutineList.Count; i++)
            {
                if (FrameRoutineList[i].Destroyed)
                {
                    FrameRoutineList.RemoveAt(i);
                }
                else
                {
                    ++i;
                }
            }
        }

        #endregion

        #region ConditionalRoutine

        public void AddConditionCallBack(CheckConditionDelegate onCheckConditionHandler,
            CheckCallBackDelegate onCheckCallBackHandler, float intervalTime = 3.0f)
        {
            if (intervalTime <= 0 || onCheckConditionHandler == null || onCheckCallBackHandler == null) return;

            ConditionalRoutine data = new ConditionalRoutine();
            data.CheckConditionHandler = onCheckConditionHandler;
            data.CheckCallBackHandler = onCheckCallBackHandler;
            data.CheckRemainTime = intervalTime;
            data.CheckTimeInterval = 0;
            data.StatusList = onCheckConditionHandler();
            ConditionalRoutineList.Add(data);
        }

        public void RemoveConditionCallBack(CheckConditionDelegate onCheckConditionHandler, CheckCallBackDelegate onCheckCallBackHandler)
        {
            if (onCheckConditionHandler == null || onCheckCallBackHandler == null) return;

            for (int i = ConditionalRoutineList.Count - 1; i >= 0; i--)
            {
                ConditionalRoutine data = ConditionalRoutineList[i];
                if (data.CheckCallBackHandler == onCheckCallBackHandler && data.CheckConditionHandler == onCheckConditionHandler) ConditionalRoutineList.Remove(data);
            }
        }

        private void UpdateConditionCallBack(float deltaTime)
        {
            foreach (ConditionalRoutine data in ConditionalRoutineList)
            {
                data.CheckTimeInterval -= deltaTime;
                if (data.CheckTimeInterval < 0)
                {
                    data.CheckTimeInterval = data.CheckRemainTime;
                    List<bool> curStatusList = data.CheckConditionHandler(); //状态不一致时才会，调用回调
                    if (curStatusList.Count != data.StatusList.Count)
                    {
                        data.CheckCallBackHandler();
                        data.StatusList = curStatusList;
                        break;
                    }

                    for (int j = 0; j < curStatusList.Count && j < data.StatusList.Count; j++)
                    {
                        if (curStatusList[j] != data.StatusList[j])
                        {
                            data.CheckCallBackHandler();
                            data.StatusList = curStatusList;
                            break;
                        }
                    }
                }
            }
        }

        #endregion

        #region CountDownRoutine

        private uint timeCountDownTokenID;
        private float currentTimeCountDownInterval;
        private readonly float timeCountDownInterval = 1;

        public uint AddTimeCountDownRoutine(long unixTime, CountDownDelegate onTimeCountDownRoutine)
        {
            long nowTime = currentTimeTick;
            long delayTime = unixTime - nowTime;
            if (onTimeCountDownRoutine != null && delayTime > 0)
            {
                timeCountDownTokenID++;
                CountDownRoutine timeCountDown = new CountDownRoutine();
                timeCountDown.CountDownCallBack = onTimeCountDownRoutine;
                timeCountDown.CallBackTime = unixTime;
                timeCountDown.TokenID = timeCountDownTokenID;
                CountDownRoutineList.Add(timeCountDown);
                return timeCountDownTokenID;
            }

            return 0;
        }

        public void RemoveTimeCountDownRoutine(uint tokenID)
        {
            for (int i = CountDownRoutineList.Count - 1; i >= 0; i--)
            {
                CountDownRoutine timeCountDown = CountDownRoutineList[i];
                if (timeCountDown.TokenID == tokenID) CountDownRoutineList.Remove(timeCountDown);
            }
        }

        private void UpdateCountDownRoutine(float deltaTime)
        {
            currentTimeCountDownInterval += deltaTime;
            if (currentTimeCountDownInterval > timeCountDownInterval)
            {
                currentTimeCountDownInterval = 0;
                for (int i = CountDownRoutineList.Count - 1; i >= 0; i--)
                {
                    CountDownRoutine timeCountDown = CountDownRoutineList[i];
                    if (timeCountDown.CallBackTime < currentTimeTick)
                    {
                        timeCountDown.CountDownCallBack();
                        CountDownRoutineList.Remove(timeCountDown);
                    }
                }
            }
        }

        #endregion
    }
}