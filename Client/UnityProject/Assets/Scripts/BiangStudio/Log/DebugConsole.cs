using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BiangStudio.Log
{
    /// <summary>
    /// A console to display Unity's debug logs in-game.
    /// </summary>
    public class DebugConsole : MonoBehaviour
    {
        private struct Log
        {
            public string message;
            public string stackTrace;
            public LogType type;
        }

        #region Inspector Settings

        /// <summary>
        /// Whether to show the log.
        /// </summary>
        public bool ShowLog;

        /// <summary>
        /// Whether to open the window by shaking the device (mobile-only).
        /// </summary>
        public bool ShakeToOpen = true;

        /// <summary>
        /// The (squared) acceleration above which the window should open.
        /// </summary>
        public float ShakeAcceleration = 3f;

        /// <summary>
        /// Whether to only keep a certain number of logs.
        ///
        /// Setting this can be helpful if memory usage is a concern.
        /// </summary>
        public bool RestrictLogCount = false;

        /// <summary>
        /// Number of logs to keep before removing old ones.
        /// </summary>
        public int MaxLogs = 1000;

        /// <summary>
        /// Whether to collapse identical logs.
        /// </summary>
        public bool Collapse = true;

        #endregion

        public delegate bool OnDebugConsoleKeyDown();

        public OnDebugConsoleKeyDown OnDebugConsoleKeyDownHandler;
        public UnityAction<bool> OnDebugConsoleToggleHandler;

        private readonly List<Log> logs = new List<Log>();
        private Vector2 scrollPosition;
        private bool visible = false;

        private bool Visible
        {
            get { return visible; }
            set
            {
                if (visible != value)
                {
                    OnDebugConsoleToggleHandler?.Invoke(value);
                    visible = value;
                }
            }
        }

        #region Visual elements:

        private static readonly Dictionary<LogType, Color> logTypeColors = new Dictionary<LogType, Color>
        {
            {LogType.Assert, Color.white},
            {LogType.Error, Color.red},
            {LogType.Exception, Color.red},
            {LogType.Log, Color.white},
            {LogType.Warning, Color.yellow},
        };

        private const string windowTitle = "Console";
        private const int margin = 20;
        private static readonly GUIContent clearLabel = new GUIContent("Clear", "Clear the contents of the console.");
        private static readonly GUIContent collapseLabel = new GUIContent("Collapse", "Hide repeated messages.");

        private readonly Rect titleBarRect = new Rect(0, 0, 10000, 20);
        private Rect windowRect = new Rect(margin, margin, Screen.width - (margin * 2), Screen.height - (margin * 2));

        #endregion

        private void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void Update()
        {
            if (ShowLog)
            {
                if (OnDebugConsoleKeyDownHandler != null && OnDebugConsoleKeyDownHandler.Invoke())
                {
                    Visible = !Visible;
                }

                if (ShakeToOpen && Input.acceleration.sqrMagnitude > ShakeAcceleration)
                {
                    Visible = true;
                }
            }
        }

        private void OnGUI()
        {
            if (!Visible)
            {
                return;
            }

            windowRect = GUILayout.Window(123456, windowRect, DrawConsoleWindow, windowTitle);
        }

        /// <summary>
        /// Displays a window that lists the recorded logs.
        /// </summary>
        /// <param name="windowID">Window ID.</param>
        private void DrawConsoleWindow(int windowID)
        {
            DrawLogsList();
            DrawToolbar();

            // Allow the window to be dragged by its title bar.
            GUI.DragWindow(titleBarRect);
        }

        /// <summary>
        /// Displays a scrollable list of logs.
        /// </summary>
        private void DrawLogsList()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            // Iterate through the recorded logs.
            for (int i = 0; i < logs.Count; i++)
            {
                Log log = logs[i];

                // Combine identical messages if collapse option is chosen.
                if (Collapse && i > 0)
                {
                    string previousMessage = logs[i - 1].message;

                    if (log.message == previousMessage)
                    {
                        continue;
                    }
                }

                GUI.contentColor = logTypeColors[log.type];
                GUILayout.Label(log.message + "\n" + log.stackTrace);
            }

            GUILayout.EndScrollView();

            // Ensure GUI colour is reset before drawing other components.
            GUI.contentColor = Color.white;
        }

        /// <summary>
        /// Displays options for filtering and changing the logs list.
        /// </summary>
        private void DrawToolbar()
        {
            //GUILayout.BeginHorizontal();

            //if (GUILayout.Button(clearLabel))
            //{
            //    logs.Clear();
            //}

            //collapse = GUILayout.Toggle(collapse, collapseLabel, GUILayout.ExpandWidth(false));

            //GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Records a log from the log callback.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="stackTrace">Trace of where the message came from.</param>
        /// <param name="type">Type of message (error, exception, warning, assert).</param>
        private void HandleLog(string message, string stackTrace, LogType type)
        {
            logs.Add(new Log
            {
                message = message,
                stackTrace = stackTrace,
                type = type,
            });

            TrimExcessLogs();
        }

        /// <summary>
        /// Removes old logs that exceed the maximum number allowed.
        /// </summary>
        private void TrimExcessLogs()
        {
            if (!RestrictLogCount)
            {
                return;
            }

            int amountToRemove = Mathf.Max(logs.Count - MaxLogs, 0);

            if (amountToRemove == 0)
            {
                return;
            }

            logs.RemoveRange(0, amountToRemove);
        }
    }
}