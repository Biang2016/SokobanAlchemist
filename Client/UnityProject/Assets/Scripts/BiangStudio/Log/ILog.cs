using System;

namespace BiangStudio.Log
{
    public interface ILog
    {
        void Print(string logStr);
        void DoPrint();

        LogVerbosity LogVerbosity { get; set; }

        void PrintWarning(string logStr);
        void PrintError(string logStr);
        void PrintClientStates(string logStr);
        void PrintServerStates(string logStr);
        void PrintReceive(string logStr);
        void PrintSend(string logStr);
    }

    public abstract class LogBase
    {
        public string LogStr;
        public string Time;
    }

    [Flags]
    public enum LogVerbosity
    {
        Normal,
        Warning,
        Error,
        ClientState,
        ServerState,
        Send,
        Receive,
        All = Normal | Warning | Error | ClientState | ServerState | Send | Receive,
        States = ClientState | ServerState,
        StatesAndLog = Normal | Warning | Error | ClientState | ServerState,
    }
}