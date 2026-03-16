using System.Collections.Generic;
using _Scripts.Core;
using UnityEngine;

namespace _Scripts.Blocks
{
    public class RotateObjectBlock : BaseBlock
    {
        public override string BlockType => "RotateObject";

        public enum RotateMode
        {
            Absolute,
            Relative
        }

        public RotateMode mode = RotateMode.Relative;
        public GameObject defaultTarget;
        public float defaultPitch = 0f, defaultYaw = 45f, defaultRoll = 0f;

        public override void Execute(IGraphRuntime rt)
        {
            var target = GetGameObject("Target", defaultTarget, rt);
            if (target == null)
            {
                TriggerFlow("FlowOut", rt);
                return;
            }

            float pitch = GetFloat("Pitch", defaultPitch, rt);
            float yaw = GetFloat("Yaw", defaultYaw, rt);
            float roll = GetFloat("Roll", defaultRoll, rt);

            if (mode == RotateMode.Absolute)
                target.transform.eulerAngles = new Vector3(pitch, yaw, roll);
            else
                target.transform.Rotate(pitch, yaw, roll, Space.Self);

            TriggerFlow("FlowOut", rt);
        }

        public override object GetOutputValue(string p, IGraphRuntime rt) => null;

        public override List<PortDefinition> GetPortDefinitions() => new()
        {
            new PortDefinition("FlowIn", "flow", false),
            new PortDefinition("Target", "gameobject", false),
            new PortDefinition("Pitch", "float", false),
            new PortDefinition("Yaw", "float", false),
            new PortDefinition("Roll", "float", false),
            new PortDefinition("FlowOut", "flow", true),
        };
    }
}