//包含同步、异步消息接口，对应的消息存在一个消息池的复用
//参数后面可以用一个wrap包起来，做到参数的复用

//对于客户端，由于存在单局内部通信、单局外部通信，单局内与单局外通信，
//所以messenger由approot持有，单局外、单局内都共享，但单局外的messenger，要封装一层，隔离对单局内的影响

//对于ds，比较单纯，生命周期只单局内。


//#define LOG_ALL_MESSAGES
//#define LOG_ADD_LISTENER
//#define LOG_BROADCAST_MESSAGE
//#define LOG_BROADCAST_PROCESS
//#define REQUIRE_LISTENER

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BiangStudio.Messenger
{
    public delegate void Callback();

    public delegate void Callback<T>(T arg1);

    public delegate void Callback<T, U>(T arg1, U arg2);

    public delegate void Callback<T, U, V>(T arg1, U arg2, V arg3);

    public delegate void Callback<T, U, V, UValue>(T arg1, U arg2, V arg3, UValue arg4);

    public delegate void Callback<T, U, V, UValue, VValue>(T arg1, U arg2, V arg3, UValue arg4, VValue arg5);


    public class Messenger
    {
        #region Internal variables

        public Dictionary<uint, Delegate> EventListenTable = new Dictionary<uint, Delegate>(); // 监听列表

        //Message handlers that should never be removed, regardless of calling Cleanup
        public List<uint> permanentMessages = new List<uint>();

        #endregion

        #region Broadcast class define

        // for async broadcast
        public abstract class AsyncMsgInfoBase
        {
            public uint eventID;
            protected string strLog;

            public AsyncMsgInfoBase(uint type)
            {
                eventID = type;
            }

            public virtual void Execute()
            {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
            strLog = "MESSENGER EXCUTE\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\tInvoking \t\"" + eventID + "\"";
#endif
            }

            public abstract void Reset();
        }

        public class AsyncMsgInfo : AsyncMsgInfoBase
        {
            private Callback cb;

            public AsyncMsgInfo(uint type) : base(type)
            {
            }

            public void SetParam(Callback c)
            {
                cb = c;
            }

            public override void Reset()
            {
                SetParam(null);
            }

            public override void Execute()
            {
                base.Execute();

#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
                strLog += "\n" + "none param";
                Delegate d = cb as Delegate;
                if (d != null)
                {
                    Delegate[] cb_list = d.GetInvocationList();
                    foreach (Delegate cb_tmp in cb_list)
                    {
                        strLog += "\n" + "{" + cb_tmp.Target + " -> " + cb_tmp.Method + "}";
                    }
                }
                 Logger.Log(strLog);
#endif
                cb();
            }
        }

        public class AsyncMsgInfo<T> : AsyncMsgInfoBase
        {
            private Callback<T> cb;
            private T param1;

            public AsyncMsgInfo(uint type) : base(type)
            {
            }

            public void SetParam(Callback<T> c, T p)
            {
                cb = c;
                param1 = p;
            }

            public override void Reset()
            {
                SetParam(null, default);
                //throw new NotImplementedException();
            }

            public override void Execute()
            {
                base.Execute();

#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
                strLog += "[Param1]" + param1.ToString();
                Delegate d = cb as Delegate;
                if (d != null)
                {
                    Delegate[] cb_list = d.GetInvocationList();
                    foreach (Delegate cb_tmp in cb_list)
                    {
                        strLog += "\n" + "{" + cb_tmp.Target + " -> " + cb_tmp.Method + "}";
                    }
                }
                 Logger.Log(strLog);
#endif
                cb(param1);
            }
        }

        public class AsyncMsgInfo<T, U> : AsyncMsgInfoBase
        {
            private Callback<T, U> cb;
            private T param1;
            private U param2;

            public AsyncMsgInfo(uint type) : base(type)
            {
            }

            public void SetParam(Callback<T, U> c, T p1, U p2)
            {
                cb = c;
                param1 = p1;
                param2 = p2;
            }

            public override void Reset()
            {
                SetParam(null, default, default);
            }

            public override void Execute()
            {
                base.Execute();

#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
                 Logger.Log("[Param1]" + param1.ToString() + " [param2]" + param2.ToString());

                Delegate d = cb as Delegate;
                if (d != null)
                {
                    Delegate[] cb_list = d.GetInvocationList();
                    foreach (Delegate cb_tmp in cb_list)
                    {
                         Logger.Log("{" + cb_tmp.Target + " -> " + cb_tmp.Method + "}");
                    }
                }
#endif
                cb(param1, param2);
            }
        }

        public class AsyncMsgInfo<T, U, V> : AsyncMsgInfoBase
        {
            private Callback<T, U, V> cb;
            private T param1;
            private U param2;
            private V param3;

            public AsyncMsgInfo(uint type) : base(type)
            {
            }

            public void SetParam(Callback<T, U, V> c, T p1, U p2, V p3)
            {
                cb = c;
                param1 = p1;
                param2 = p2;
                param3 = p3;
            }

            public override void Reset()
            {
                SetParam(null, default, default, default);
            }

            public override void Execute()
            {
                base.Execute();

#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
                Logger.Log("[Param1]" + param1.ToString() + " [param2]" + param2.ToString() + " [param3]" + param3.ToString());

                Delegate d = cb as Delegate;
                if (d != null)
                {
                    Delegate[] cb_list = d.GetInvocationList();
                    foreach (Delegate cb_tmp in cb_list)
                    {
                         Logger.Log("{" + cb_tmp.Target + " -> " + cb_tmp.Method + "}");
                    }
                }
#endif
                cb(param1, param2, param3);
            }
        }

        public class AsyncMsgInfo<T, U, V, UValue> : AsyncMsgInfoBase
        {
            private Callback<T, U, V, UValue> cb;
            private T param1;
            private U param2;
            private V param3;
            private UValue param4;

            public AsyncMsgInfo(uint type) : base(type)
            {
            }

            public void SetParam(Callback<T, U, V, UValue> c, T p1, U p2, V p3, UValue p4)
            {
                cb = c;
                param1 = p1;
                param2 = p2;
                param3 = p3;
                param4 = p4;
            }

            public override void Reset()
            {
                SetParam(null, default, default, default, default);
            }

            public override void Execute()
            {
                base.Execute();

#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
                Logger.Log("[Param1]" + param1.ToString() + " [param2]" + param2.ToString() + " [param3]" + param3.ToString()+ " [param4]" + param4.ToString());

                Delegate d = cb as Delegate;
                if (d != null)
                {
                    Delegate[] cb_list = d.GetInvocationList();
                    foreach (Delegate cb_tmp in cb_list)
                    {
                         Logger.Log("{" + cb_tmp.Target + " -> " + cb_tmp.Method + "}");
                    }
                }
#endif
                cb(param1, param2, param3, param4);
            }
        }

        public class AsyncMsgInfo<T, U, V, UValue, VValue> : AsyncMsgInfoBase
        {
            private Callback<T, U, V, UValue, VValue> cb;
            private T param1;
            private U param2;
            private V param3;
            private UValue param4;
            private VValue param5;

            public AsyncMsgInfo(uint type) : base(type)
            {
            }

            public override void Reset()
            {
                SetParam(null, default, default, default, default, default);
            }

            public void SetParam(Callback<T, U, V, UValue, VValue> c, T p1, U p2, V p3, UValue p4, VValue p5)
            {
                cb = c;
                param1 = p1;
                param2 = p2;
                param3 = p3;
                param4 = p4;
                param5 = p5;
            }

            public override void Execute()
            {
                base.Execute();

#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
                Logger.Log("[Param1]" + param1.ToString() + " [param2]" + param2.ToString() + " [param3]" + param3.ToString()+ " [param4]" + param4.ToString() + " [param5]" + param5.ToString());

                Delegate d = cb as Delegate;
                if (d != null)
                {
                    Delegate[] cb_list = d.GetInvocationList();
                    foreach (Delegate cb_tmp in cb_list)
                    {
                         Logger.Log("{" + cb_tmp.Target + " -> " + cb_tmp.Method + "}");
                    }
                }
#endif
                cb(param1, param2, param3, param4, param5);
            }
        }

        #endregion

        private List<AsyncMsgInfoBase> eventTypeAsyncList = new List<AsyncMsgInfoBase>(); // 事件队列
        private List<AsyncMsgInfoBase> eventForUpdate = new List<AsyncMsgInfoBase>(); // 准备更新的列表

        #region Helper methods

        public List<uint> ExportEventByIDRange(uint from, uint to)
        {
            List<uint> lst = new List<uint>();
            foreach (KeyValuePair<uint, Delegate> item in EventListenTable)
            {
                if (item.Key <= to && item.Key >= from)
                {
                    lst.Add(item.Key);
                    Debug.Log("eventid:" + item.Key);
                }
            }

            return lst;
        }

        //Marks a certain message as permanent.
        public void MarkAsPermanent(uint eventID)
        {
#if LOG_ALL_MESSAGES
	 Logger.Log("Messenger MarkAsPermanent \t\"" + eventID + "\"");
#endif
            permanentMessages.Add(eventID);
        }


        public void Cleanup()
        {
#if LOG_ALL_MESSAGES
	 Logger.Log("MESSENGER Cleanup. Make sure that none of necessary listeners are removed.");
#endif

            List<uint> messagesToRemove = new List<uint>();

            foreach (KeyValuePair<uint, Delegate> pair in EventListenTable)
            {
                bool wasFound = false;

                foreach (uint message in permanentMessages)
                {
                    if (pair.Key == message)
                    {
                        wasFound = true;
                        break;
                    }
                }

                if (!wasFound)
                    messagesToRemove.Add(pair.Key);
            }

            foreach (uint message in messagesToRemove)
            {
                Remove(message);
            }
        }

        private void Remove(uint message)
        {
            // remove from ayncevent
            int nCount = eventTypeAsyncList.Count;

            for (int i = nCount - 1; i >= 0; --i)
            {
                if (eventTypeAsyncList[i].eventID == message)
                {
                    eventTypeAsyncList.RemoveAt(i);
                }
            }

            // remove from m_eventListenTable
            EventListenTable.Remove(message);
        }

        #endregion

        #region Message logging and exception throwing

        public void OnListenerAdding(uint eventID, Delegate listenerBeingAdded)
        {
#if LOG_ALL_MESSAGES || LOG_ADD_LISTENER
	 Logger.Log("MESSENGER OnListenerAdding \"" + eventID + "\"{" + listenerBeingAdded.Target + " -> " + listenerBeingAdded.Method + "}");
#endif

            if (!EventListenTable.ContainsKey(eventID))
            {
                EventListenTable.Add(eventID, null);
            }

            Delegate d = EventListenTable[eventID];
            if (d != null && d.GetType() != listenerBeingAdded.GetType())
            {
                throw new ListenerException(string.Format(
                    "Attempting to add listener with inconsistent signature for event type {0}. Current listeners have type {1} and listener being added has type {2}", eventID, d.GetType().Name,
                    listenerBeingAdded.GetType().Name));
            }
        }

        public bool OnListenerRemoving(uint eventID, Delegate listenerBeingRemoved)
        {
#if LOG_ALL_MESSAGES
	 Logger.Log("MESSENGER OnListenerRemoving \t\"" + eventID + "\"\t{" + listenerBeingRemoved.Target + " -> " + listenerBeingRemoved.Method + "}");
#endif

            if (EventListenTable.ContainsKey(eventID))
            {
                Delegate d = EventListenTable[eventID];

                if (d == null)
                {
                    throw new ListenerException(string.Format("Attempting to remove listener with for event type \"{0}\" but current listener is null.", eventID));
                }

                if (d.GetType() != listenerBeingRemoved.GetType())
                {
                    throw new ListenerException(string.Format(
                        "Attempting to remove listener with inconsistent signature for event type {0}. Current listeners have type {1} and listener being removed has type {2}", eventID,
                        d.GetType().Name, listenerBeingRemoved.GetType().Name));
                }

                return true;
            }

            return false;
        }

        public void OnBroadcasting(uint eventID, bool bAsync)
        {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
         Logger.Log("MESSENGER SEND " + (bAsync ? "ASYNC\t" : "\t") + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\tInvoking \t\"" + eventID + "\"");
#endif

#if REQUIRE_LISTENER
        if (!m_eventListenTable.ContainsKey(eventID))
        {
            throw new BroadcastException(string.Format("Broadcasting message \"{0}\" but no listener found. Try marking the message with Messenger.MarkAsPermanent.", eventID));
        }
#endif
        }

        public BroadcastException CreateBroadcastSignatureException(uint eventID)
        {
            return new BroadcastException(string.Format("Broadcasting message \"{0}\" but listeners have a different signature than the broadcaster.", eventID));
        }

        public class BroadcastException : Exception
        {
            public BroadcastException(string msg)
                : base(msg)
            {
            }
        }

        public class ListenerException : Exception
        {
            public ListenerException(string msg)
                : base(msg)
            {
            }
        }

        #endregion

        #region AddListener

        /// <summary>
        ///     监听事件
        /// </summary>
        public void AddListener(uint eventID, Callback handler)
        {
            OnListenerAdding(eventID, handler);
            EventListenTable[eventID] = (Callback) EventListenTable[eventID] + handler;
        }

        //Single parameter
        /// <summary>
        ///     监听事件
        /// </summary>
        public void AddListener<T>(uint eventID, Callback<T> handler)
        {
            OnListenerAdding(eventID, handler);
            EventListenTable[eventID] = (Callback<T>) EventListenTable[eventID] + handler;
        }

        //Two parameters
        /// <summary>
        ///     监听事件
        /// </summary>
        public void AddListener<T, U>(uint eventID, Callback<T, U> handler)
        {
            OnListenerAdding(eventID, handler);
            EventListenTable[eventID] = (Callback<T, U>) EventListenTable[eventID] + handler;
        }

        //Three parameters
        /// <summary>
        ///     监听事件
        /// </summary>
        public void AddListener<T, U, V>(uint eventID, Callback<T, U, V> handler)
        {
            OnListenerAdding(eventID, handler);
            EventListenTable[eventID] = (Callback<T, U, V>) EventListenTable[eventID] + handler;
        }

        //Four parameters
        /// <summary>
        ///     监听事件
        /// </summary>
        public void AddListener<T, U, V, UValue>(uint eventID, Callback<T, U, V, UValue> handler)
        {
            OnListenerAdding(eventID, handler);
            EventListenTable[eventID] = (Callback<T, U, V, UValue>) EventListenTable[eventID] + handler;
        }

        //Five parameters
        /// <summary>
        ///     监听事件
        /// </summary>
        public void AddListener<T, U, V, UValue, VValue>(uint eventID, Callback<T, U, V, UValue, VValue> handler)
        {
            OnListenerAdding(eventID, handler);
            EventListenTable[eventID] = (Callback<T, U, V, UValue, VValue>) EventListenTable[eventID] + handler;
        }

        #endregion

        #region RemoveListener

        public void RemoveListener(uint eventID, Callback handler)
        {
            if (OnListenerRemoving(eventID, handler))
            {
                EventListenTable[eventID] = (Callback) EventListenTable[eventID] - handler;
                if (EventListenTable[eventID] == null)
                {
                    Remove(eventID);
                }
            }
        }

        public void RemoveListener<T>(uint eventID, Callback<T> handler)
        {
            if (OnListenerRemoving(eventID, handler))
            {
                EventListenTable[eventID] = (Callback<T>) EventListenTable[eventID] - handler;
                if (EventListenTable[eventID] == null)
                {
                    Remove(eventID);
                }
            }
        }

        public void RemoveListener<T, U>(uint eventID, Callback<T, U> handler)
        {
            if (OnListenerRemoving(eventID, handler))
            {
                EventListenTable[eventID] = (Callback<T, U>) EventListenTable[eventID] - handler;
                if (EventListenTable[eventID] == null)
                {
                    Remove(eventID);
                }
            }
        }

        public void RemoveListener<T, U, V>(uint eventID, Callback<T, U, V> handler)
        {
            if (OnListenerRemoving(eventID, handler))
            {
                EventListenTable[eventID] = (Callback<T, U, V>) EventListenTable[eventID] - handler;
                if (EventListenTable[eventID] == null)
                {
                    Remove(eventID);
                }
            }
        }

        public void RemoveListener<T, U, V, UValue>(uint eventID, Callback<T, U, V, UValue> handler)
        {
            if (OnListenerRemoving(eventID, handler))
            {
                EventListenTable[eventID] = (Callback<T, U, V, UValue>) EventListenTable[eventID] - handler;
                if (EventListenTable[eventID] == null)
                {
                    Remove(eventID);
                }
            }
        }

        public void RemoveListener<T, U, V, UValue, VValue>(uint eventID, Callback<T, U, V, UValue, VValue> handler)
        {
            if (OnListenerRemoving(eventID, handler))
            {
                EventListenTable[eventID] = (Callback<T, U, V, UValue, VValue>) EventListenTable[eventID] - handler;
                if (EventListenTable[eventID] == null)
                {
                    Remove(eventID);
                }
            }
        }

        #endregion

        #region Broadcast definition

        public void Broadcast(uint eventID)
        {
            DoBroadcast(eventID, false);
        }

        public void BroadcastAsync(uint eventID)
        {
            DoBroadcast(eventID, true);
        }

        public void Broadcast<T>(uint eventID, T arg1)
        {
            DoBroadcast(eventID, arg1, false);
        }

        public void BroadcastAsync<T>(uint eventID, T arg1)
        {
            DoBroadcast(eventID, arg1, true);
        }

        public void Broadcast<T, U>(uint eventID, T arg1, U arg2)
        {
            DoBroadcast(eventID, arg1, arg2, false);
        }

        public void BroadcastAsync<T, U>(uint eventID, T arg1, U arg2)
        {
            DoBroadcast(eventID, arg1, arg2, true);
        }

        public void Broadcast<T, U, V>(uint eventID, T arg1, U arg2, V arg3)
        {
            DoBroadcast(eventID, arg1, arg2, arg3, false);
        }

        public void BroadcastAsync<T, U, V>(uint eventID, T arg1, U arg2, V arg3)
        {
            DoBroadcast(eventID, arg1, arg2, arg3, true);
        }

        public void Broadcast<T, U, V, UValue>(uint eventID, T arg1, U arg2, V arg3, UValue arg4)
        {
            DoBroadcast(eventID, arg1, arg2, arg3, arg4, false);
        }

        public void BroadcastAsync<T, U, V, UValue>(uint eventID, T arg1, U arg2, V arg3, UValue arg4)
        {
            DoBroadcast(eventID, arg1, arg2, arg3, arg4, true);
        }

        public void Broadcast<T, U, V, UValue, VValue>(uint eventID, T arg1, U arg2, V arg3, UValue arg4, VValue arg5)
        {
            DoBroadcast(eventID, arg1, arg2, arg3, arg4, arg5, false);
        }

        public void BroadcastAsync<T, U, V, UValue, VValue>(uint eventID, T arg1, U arg2, V arg3, UValue arg4, VValue arg5)
        {
            DoBroadcast(eventID, arg1, arg2, arg3, arg4, arg5, true);
        }

        #endregion

        #region Broadcast implementation

        private void ReadyBroadcast(AsyncMsgInfoBase info, bool bAsync)
        {
            if (info != null)
            {
                if (bAsync)
                {
                    eventTypeAsyncList.Add(info);
                }
                else
                {
                    info.Execute();
                    RemoveMsgInfo(info);
                }
            }
        }

        private void DoBroadcast(uint eventID, bool bAsync)
        {
            OnBroadcasting(eventID, bAsync);
            ReadyBroadcast(GetMsgInfo(eventID), bAsync);
        }

        private void DoBroadcast<T>(uint eventID, T arg1, bool bAsync)
        {
            OnBroadcasting(eventID, bAsync);
            ReadyBroadcast(GetMsgInfo(eventID, arg1), bAsync);
        }

        private void DoBroadcast<T, U>(uint eventID, T arg1, U arg2, bool bAsync)
        {
            OnBroadcasting(eventID, bAsync);
            ReadyBroadcast(GetMsgInfo(eventID, arg1, arg2), bAsync);
        }

        private void DoBroadcast<T, U, V>(uint eventID, T arg1, U arg2, V arg3, bool bAsync)
        {
            OnBroadcasting(eventID, bAsync);
            ReadyBroadcast(GetMsgInfo(eventID, arg1, arg2, arg3), bAsync);
        }

        private void DoBroadcast<T, U, V, UValue>(uint eventID, T arg1, U arg2, V arg3, UValue arg4, bool bAsync)
        {
            OnBroadcasting(eventID, bAsync);
            ReadyBroadcast(GetMsgInfo(eventID, arg1, arg2, arg3, arg4), bAsync);
        }

        private void DoBroadcast<T, U, V, UValue, VValue>(uint eventID, T arg1, U arg2, V arg3, UValue arg4, VValue arg5, bool bAsync)
        {
            OnBroadcasting(eventID, bAsync);
            ReadyBroadcast(GetMsgInfo(eventID, arg1, arg2, arg3, arg4, arg5), bAsync);
        }

        #endregion

        #region Broadcast implementation for Async

        public void Update()
        {
            // add all event to update list
            int nCount = eventTypeAsyncList.Count;
            for (int i = 0; i < nCount; ++i)
            {
                eventForUpdate.Add(eventTypeAsyncList[i]);
            }

            eventTypeAsyncList.Clear();

            // call back
            DoProcessCallbacks(eventForUpdate);
            eventForUpdate.Clear();
        }

        public void DoProcessCallbacks(List<AsyncMsgInfoBase> eventTypeList)
        {
            if (eventTypeList.Count > 0)
            {
                int nCount = eventTypeList.Count;
                for (int i = 0; i < nCount; ++i)
                {
                    if (EventListenTable.ContainsKey(eventTypeList[i].eventID))
                    {
                        eventTypeList[i].Execute();
                        RemoveMsgInfo(eventTypeList[i]);
                    }
                    else
                    {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
                     Logger.LogError("MESSENGER ASYNC EXCUTE FAIL:has unlistened\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\tInvoking \t\"" + eventTypeList[i].eventID + "\"");
#endif
                    }
                }
            }
        }

        #endregion

        #region 对象池

        private Dictionary<uint, AsyncMsgInfoBase> eventTypeList_Pool = new Dictionary<uint, AsyncMsgInfoBase>(); // 缓存池，每个事件都缓存

        // 得到一个对象
        private AsyncMsgInfoBase GetMsgInfo(uint eventID)
        {
            Delegate d;
            if (EventListenTable.TryGetValue(eventID, out d))
            {
                Callback callback = d as Callback;
#if UNITY_EDITOR
                if (d != null && callback == null)
                {
                    Debug.LogError(string.Format("{0}这个事件注册参数类型和消息发送的类型不一致，请查一下！！", eventID));
                }
#endif
                if (callback != null)
                {
                    AsyncMsgInfo info = GetFromPool(eventID) as AsyncMsgInfo;
                    if (info == null)
                    {
                        info = new AsyncMsgInfo(eventID);
                    }

                    info.SetParam(callback);

                    return info;
                }
            }

            return null;
        }

        private AsyncMsgInfoBase GetMsgInfo<T>(uint eventID, T arg1)
        {
            Delegate d;
            if (EventListenTable.TryGetValue(eventID, out d))
            {
                Callback<T> callback = d as Callback<T>;
#if UNITY_EDITOR
                if (d != null && callback == null)
                {
                    Debug.LogError(string.Format("{0}这个事件注册参数类型和消息发送的类型不一致，请查一下！！", eventID));
                }
#endif
                if (callback != null)
                {
                    AsyncMsgInfo<T> info = GetFromPool(eventID) as AsyncMsgInfo<T>;
                    if (info == null)
                    {
                        info = new AsyncMsgInfo<T>(eventID);
                    }

                    info.SetParam(callback, arg1);

                    return info;
                }
            }

            return null;
        }

        private AsyncMsgInfoBase GetMsgInfo<T, U>(uint eventID, T arg1, U arg2)
        {
            Delegate d;
            if (EventListenTable.TryGetValue(eventID, out d))
            {
                Callback<T, U> callback = d as Callback<T, U>;
#if UNITY_EDITOR
                if (d != null && callback == null)
                {
                    Debug.LogError(string.Format("{0}这个事件注册参数类型和消息发送的类型不一致，请查一下！！", eventID));
                }
#endif
                if (callback != null)
                {
                    AsyncMsgInfo<T, U> info = GetFromPool(eventID) as AsyncMsgInfo<T, U>;
                    if (info == null)
                    {
                        info = new AsyncMsgInfo<T, U>(eventID);
                    }

                    info.SetParam(callback, arg1, arg2);

                    return info;
                }
            }

            return null;
        }

        private AsyncMsgInfoBase GetMsgInfo<T, U, V>(uint eventID, T arg1, U arg2, V arg3)
        {
            Delegate d;
            if (EventListenTable.TryGetValue(eventID, out d))
            {
                Callback<T, U, V> callback = d as Callback<T, U, V>;
#if UNITY_EDITOR
                if (d != null && callback == null)
                {
                    Debug.LogError(string.Format("{0}这个事件注册参数类型和消息发送的类型不一致，请查一下！！", eventID));
                }
#endif
                if (callback != null)
                {
                    AsyncMsgInfo<T, U, V> info = GetFromPool(eventID) as AsyncMsgInfo<T, U, V>;
                    if (info == null)
                    {
                        info = new AsyncMsgInfo<T, U, V>(eventID);
                    }

                    info.SetParam(callback, arg1, arg2, arg3);

                    return info;
                }
            }

            return null;
        }

        private AsyncMsgInfoBase GetMsgInfo<T, U, V, UValue>(uint eventID, T arg1, U arg2, V arg3, UValue arg4)
        {
            Delegate d;
            if (EventListenTable.TryGetValue(eventID, out d))
            {
                Callback<T, U, V, UValue> callback = d as Callback<T, U, V, UValue>;
#if UNITY_EDITOR
                if (d != null && callback == null)
                {
                    Debug.LogError(string.Format("{0}这个事件注册参数类型和消息发送的类型不一致，请查一下！！", eventID));
                }
#endif
                if (callback != null)
                {
                    AsyncMsgInfo<T, U, V, UValue> info = GetFromPool(eventID) as AsyncMsgInfo<T, U, V, UValue>;
                    if (info == null)
                    {
                        info = new AsyncMsgInfo<T, U, V, UValue>(eventID);
                    }

                    info.SetParam(callback, arg1, arg2, arg3, arg4);

                    return info;
                }
            }

            return null;
        }

        private AsyncMsgInfoBase GetMsgInfo<T, U, V, UValue, VValue>(uint eventID, T arg1, U arg2, V arg3, UValue arg4, VValue arg5)
        {
            Delegate d;
            if (EventListenTable.TryGetValue(eventID, out d))
            {
                Callback<T, U, V, UValue, VValue> callback = d as Callback<T, U, V, UValue, VValue>;
#if UNITY_EDITOR
                if (d != null && callback == null)
                {
                    Debug.LogError(string.Format("{0}这个事件注册参数类型和消息发送的类型不一致，请查一下！！", eventID));
                }
#endif
                if (callback != null)
                {
                    AsyncMsgInfo<T, U, V, UValue, VValue> info = GetFromPool(eventID) as AsyncMsgInfo<T, U, V, UValue, VValue>;
                    if (info == null)
                    {
                        info = new AsyncMsgInfo<T, U, V, UValue, VValue>(eventID);
                    }

                    info.SetParam(callback, arg1, arg2, arg3, arg4, arg5);

                    return info;
                }
            }

            return null;
        }

        private void RemoveMsgInfo(AsyncMsgInfoBase eventInfo)
        {
            eventInfo.Reset();
            AddToPool(eventInfo);
        }

        private void AddToPool(AsyncMsgInfoBase eventInfo)
        {
            if (eventTypeList_Pool.ContainsKey(eventInfo.eventID) == false)
            {
                eventTypeList_Pool.Add(eventInfo.eventID, eventInfo);
            }
        }

        private AsyncMsgInfoBase GetFromPool(uint eventID)
        {
            if (eventTypeList_Pool.ContainsKey(eventID))
            {
                AsyncMsgInfoBase obj = eventTypeList_Pool[eventID];
                eventTypeList_Pool.Remove(eventID);
                return obj;
            }

            return null;
        }

        #endregion
    }
}