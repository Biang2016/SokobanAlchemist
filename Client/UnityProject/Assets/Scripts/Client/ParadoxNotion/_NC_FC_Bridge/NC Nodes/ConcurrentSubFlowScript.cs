using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using FlowCanvas;

namespace NodeCanvas.StateMachines
{

    [Name("Parallel Sub FlowScript", -1)]
    [Description("Execute a Sub FlowScript in parallel and for as long as this FSM is running.")]
    [Category("SubGraphs")]
    [Color("ff64cb")]
    public class ConcurrentSubFlowScript : FSMNodeNested<FlowScript>, IUpdatable
    {

        [SerializeField, ExposeField, Name("Parallel FlowScript")]
        protected BBParameter<FlowScript> _subFlowScript = null;

        public override string name => base.name.ToUpper();
        public override int maxInConnections => 0;
        public override int maxOutConnections => 0;
        public override bool allowAsPrime => false;

        public override FlowScript subGraph { get { return _subFlowScript.value; } set { _subFlowScript.value = value; } }
        public override BBParameter subGraphParameter => _subFlowScript;

        ///----------------------------------------------------------------------------------------------

        public override void OnGraphStarted() {
            if ( subGraph == null ) { return; }
            status = Status.Running;
            this.TryStartSubGraph(graphAgent, (result) => { status = result ? Status.Success : Status.Failure; });
        }

        void IUpdatable.Update() {
            this.TryUpdateSubGraph();
        }
    }
}