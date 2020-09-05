using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.BehaviourTrees
{

    [Category("Composites")]
    [Description("Used for Utility AI, the Priority Selector executes the child with the highest priority weight. If it fails, the Priority Selector will continue with the next highest priority child until one Succeeds, or until all Fail (similar to how a normal Selector does).")]
    [Icon("Priority")]
    [Color("b3ff7f")]
    public class PrioritySelector : BTComposite
    {

        [AutoSortWithChildrenConnections]
        public List<BBParameter<float>> priorities;

        private Connection[] orderedConnections;
        private int current = 0;

        public override void OnChildConnected(int index) {
            if ( priorities == null ) { priorities = new List<BBParameter<float>>(); }
            if ( priorities.Count < outConnections.Count ) {
                priorities.Insert(index, new BBParameter<float> { value = 1, bb = graphBlackboard });
            }
        }

        public override void OnChildDisconnected(int index) {
            priorities.RemoveAt(index);
        }

        protected override Status OnExecute(Component agent, IBlackboard blackboard) {

            if ( status == Status.Resting ) {
                orderedConnections = outConnections.OrderBy(c => priorities[outConnections.IndexOf(c)].value).ToArray();
            }

            for ( var i = orderedConnections.Length; i-- > 0; ) {
                status = orderedConnections[i].Execute(agent, blackboard);
                if ( status == Status.Success ) {
                    return Status.Success;
                }

                if ( status == Status.Running ) {
                    current = i;
                    return Status.Running;
                }
            }

            return Status.Failure;
        }

        protected override void OnReset() {
            current = 0;
        }

        ////////////////////////////////////////
        ///////////GUI AND EDITOR STUFF/////////
        ////////////////////////////////////////
#if UNITY_EDITOR

        public override string GetConnectionInfo(int i) {
            return priorities[i].ToString();
        }

        public override void OnConnectionInspectorGUI(int i) {
            priorities[i] = (BBParameter<float>)NodeCanvas.Editor.BBParameterEditor.ParameterField("Priority Weight", priorities[i]);
        }

        protected override void OnNodeInspectorGUI() {

            if ( outConnections.Count == 0 ) {
                GUILayout.Label("Make some connections first");
                return;
            }

            for ( var i = 0; i < priorities.Count; i++ ) {
                priorities[i] = (BBParameter<float>)NodeCanvas.Editor.BBParameterEditor.ParameterField("Priority Weight", priorities[i]);
            }
        }

#endif
    }
}