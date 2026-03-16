using System.Collections.Generic;
using _Scripts.Core;
using UnityEngine;

namespace _Scripts.Blocks
{
    public class MoveObjectBlock : BaseBlock
    {
        public override string BlockType => "MoveObject";

        public enum MoveMode
        {
            Absolute,
            Relative
        }

        public MoveMode mode = MoveMode.Absolute;
        public GameObject defaultTarget;
        public float defaultX = 0f, defaultY = 0f, defaultZ = 0f;

        public override void Execute(IGraphRuntime rt)
        {
            var target = GetGameObject("Target", defaultTarget, rt);
            if (target == null)
            {
                TriggerFlow("FlowOut", rt);
                return;
            }

            var delta = new Vector3(
                GetFloat("X", defaultX, rt),
                GetFloat("Y", defaultY, rt),
                GetFloat("Z", defaultZ, rt));

            if (mode == MoveMode.Absolute) target.transform.position = delta;
            else target.transform.position += delta;

            TriggerFlow("FlowOut", rt);
        }

        public override object GetOutputValue(string p, IGraphRuntime rt) => null;

        public override List<PortDefinition> GetPortDefinitions() => new()
        {
            new PortDefinition("FlowIn", "flow", false),
            new PortDefinition("Target", "gameobject", false),
            new PortDefinition("X", "float", false),
            new PortDefinition("Y", "float", false),
            new PortDefinition("Z", "float", false),
            new PortDefinition("FlowOut", "flow", true),
        };
    }
}