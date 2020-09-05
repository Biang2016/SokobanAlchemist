using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using FlowCanvas;

namespace NodeCanvas.StateMachines
{

    [Name("Sub FlowScript")]
    [Description("Adds a FlowCanvas FlowScript as a nested State of the FSM. The FlowScript State is never finished by itself, unless a 'Finish' node is used in the FlowScript. Success/Failure events can optionlay be used alongside with a CheckEvent on state transitions to catch whether the FlowScript Finished in success or failure.")]
    [DropReferenceType(typeof(FlowScript))]
    public class FlowScriptState : FSMStateNested<FlowScript>
    {

        [SerializeField, ExposeField]
        private BBParameter<FlowScript> _flowScript = null;

        [DimIfDefault]
        public string successEvent;
        [DimIfDefault]
        public string failureEvent;

        public override FlowScript subGraph { get { return _flowScript.value; } set { _flowScript.value = value; } }
        public override BBParameter subGraphParameter => _flowScript;

        protected override void OnEnter() {

            if ( subGraph == null ) {
                Finish(false);
                return;
            }

            this.TryStartSubGraph(graphAgent, OnFlowScriptFinished);
        }

        protected override void OnUpdate() {
            currentInstance.UpdateGraph();
        }

        void OnFlowScriptFinished(bool success) {
            if ( this.status == Status.Running ) {
                if ( !string.IsNullOrEmpty(successEvent) && success ) {
                    SendEvent(successEvent);
                }

                if ( !string.IsNullOrEmpty(failureEvent) && !success ) {
                    SendEvent(failureEvent);
                }

                Finish(success);
            }
        }

        protected override void OnExit() {
            if ( currentInstance != null ) {
                if ( this.status == Status.Running ) {
                    this.TryReadMappedVariables();
                }
                currentInstance.Stop();
            }
        }
    }
}