using System.Collections;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes
{

    ///Task Action is used to run ActionTasks within the flowscript in a simplified manner without exposing ports
    [Description("Execute an encapsulated action without exposing any value ports")]
    public class TaskAction : FlowNode, ITaskAssignable<ActionTask>
    {

        [SerializeField] private ActionTask _action;

        private FlowOutput onFinish;
        private Coroutine coroutine;

        public override string name => action != null ? action.name : "Action";

        public ActionTask action {
            get { return _action; }
            set
            {
                if ( _action != value ) {
                    _action = value;
                    GatherPorts();
                }
            }
        }

        Task ITaskAssignable.task {
            get { return action; }
            set { action = (ActionTask)value; }
        }

        public override void OnGraphStarted() { coroutine = null; }
        public override void OnGraphStoped() {
            if ( coroutine != null ) {
                StopCoroutine(coroutine);
                coroutine = null;
            }

            if ( action != null ) {
                action.EndAction(null);
            }
        }

        public override void OnGraphPaused() {
            if ( action != null ) {
                action.Pause();
            }
        }

        protected override void RegisterPorts() {
            onFinish = AddFlowOutput(SPACE);
            AddFlowInput(SPACE, (f) =>
            {
                if ( action == null ) {
                    onFinish.Call(f);
                    return;
                }

                if ( coroutine == null ) {
                    coroutine = StartCoroutine(DoUpdate(f));
                }
            });
        }

        IEnumerator DoUpdate(Flow f) {
            SetStatus(Status.Running);
            while ( graph.isPaused || action.Execute(graphAgent, graphBlackboard) == Status.Running ) {
                yield return null;
            }
            coroutine = null;
            onFinish.Call(f);
            SetStatus(Status.Resting);
        }
    }
}