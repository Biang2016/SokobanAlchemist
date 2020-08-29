using BiangStudio.GameDataFormat;
using UnityEngine;
using UnityEngine.Events;

namespace BiangStudio.LockStep
{
    public class LockStepLogic
    {
        public Fix64 AccumulatedTime { get; private set; } = Fix64.Zero;
        public Fix64 NextGameTime { get; private set; } = Fix64.Zero;
        public Fix64 Interpolation { get; private set; } = Fix64.Zero;
        public long GameFrameCount { get; private set; } = 0;

        public UnityAction OnLogicTick;
        public UnityAction OnLateLogicTick;

        private static int LogicTickRatePerFrame = 60;

        public static Fix64 FixedTimeStep = 1 / (Fix64) LogicTickRatePerFrame;

        public LockStepLogic()
        {
            init();
        }

        public void init()
        {
            AccumulatedTime = Fix64.Zero;
            NextGameTime = Fix64.Zero;
            Interpolation = Fix64.Zero;
            GameFrameCount = 0;
        }

        public void UpdateLogic()
        {
            Fix64 deltaTime = (Fix64) Time.deltaTime;

            /**************以下是帧同步的核心逻辑*********************/
            AccumulatedTime = AccumulatedTime + deltaTime;
            //Debug.Log($"Delta:{deltaTime}");

            //如果真实累计的时间超过游戏帧逻辑原本应有的时间,则循环执行逻辑,确保整个逻辑的运算不会因为帧间隔时间的波动而计算出不同的结果
            while (AccumulatedTime > NextGameTime)
            {
                //运行与游戏相关的具体逻辑
                OnLogicTick?.Invoke();
                OnLateLogicTick?.Invoke();

                //计算下一个逻辑帧应有的时间
                NextGameTime += FixedTimeStep;
                GameFrameCount += 1;

                //Debug.Log($"Tick:{GameFrameCount}");

                //游戏逻辑帧自增
                GameData.CurrentLogicFrame += 1;
            }

            //计算两帧的时间差,用于运行补间动画
            Interpolation = AccumulatedTime + FixedTimeStep - NextGameTime;
        }
    }
}