using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using FlowCanvas;

namespace NodeCanvas.BehaviourTrees
{

    [Name("Sub FlowScript")]
    [Description("Executes a nested FlowScript. Returns Running while the FlowScript is active. You can Finish the FlowScript with the 'Finish' node and return Success or Failure")]
    [Icon("FS")]
    [DropReferenceType(typeof(FlowScript))]
    public class BTNestedFlowScript : BTNodeNested<FlowScript>
    {

        [SerializeField, ExposeField]
        private BBParameter<FlowScript> _flowScript = null;

        public override FlowScript subGraph { get { return _flowScript.value; } set { _flowScript.value = value; } }
        public override BBParameter subGraphParameter => _flowScript;

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {

            if ( subGraph == null ) {
                return Status.Optional;
            }

            if ( status == Status.Resting ) {
                status = Status.Running;
                this.TryStartSubGraph(agent, OnFlowScriptFinished);
            }

            if ( status == Status.Running ) {
                currentInstance.UpdateGraph();
            }

            return status;
        }

        void OnFlowScriptFinished(bool success) {
            if ( status == Status.Running ) {
                status = success ? Status.Success : Status.Failure;
            }
        }

        protected override void OnReset() {
            if ( currentInstance != null ) {
                currentInstance.Stop();
            }
        }
    }
}