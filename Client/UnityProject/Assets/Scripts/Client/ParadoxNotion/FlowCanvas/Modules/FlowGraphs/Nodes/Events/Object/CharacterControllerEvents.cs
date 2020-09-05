using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes
{

    [Name("Character Controller")]
    [Category("Events/Object")]
    [Description("Called when the Character Controller hits a collider while performing a Move")]
    public class CharacterControllerEvents : RouterEventNode<CharacterController>
    {

        private FlowOutput onHit;
        private CharacterController receiver;
        private ControllerColliderHit hitInfo;

        protected override void RegisterPorts() {
            onHit = AddFlowOutput("Collider Hit");
            AddValueOutput<CharacterController>("Receiver", () => { return receiver; });
            AddValueOutput<GameObject>("Other", () => { return hitInfo.gameObject; });
            AddValueOutput<Vector3>("Collision Point", () => { return hitInfo.point; });
            AddValueOutput<ControllerColliderHit>("Collision Info", () => { return hitInfo; });
        }

        protected override void Subscribe(ParadoxNotion.Services.EventRouter router) {
            router.onControllerColliderHit += OnControllerColliderHit;
        }

        protected override void UnSubscribe(ParadoxNotion.Services.EventRouter router) {
            router.onControllerColliderHit -= OnControllerColliderHit;
        }

        void OnControllerColliderHit(ParadoxNotion.EventData<ControllerColliderHit> msg) {
            this.receiver = ResolveReceiver(msg.receiver);
            this.hitInfo = msg.value;
            onHit.Call(new Flow());
        }
    }
}