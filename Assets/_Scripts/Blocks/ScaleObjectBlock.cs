using System.Collections.Generic;
using _Scripts.Core;
using UnityEngine;

namespace _Scripts.Blocks
{
    public class ScaleObjectBlock : BaseBlock
    {
        public override string BlockType => "ScaleObject";

        public enum ScaleMode
        {
            Absolute,
            Multiply
        }

        public ScaleMode mode = ScaleMode.Absolute;
        public GameObject defaultTarget;
        public float defaultX = 2f, defaultY = 2f, defaultZ = 2f;

        public override void Execute(IGraphRuntime rt)
        {
            var target = GetGameObject("Target", defaultTarget, rt);
            if (target == null)
            {
                TriggerFlow("FlowOut", rt);
                return;
            }

            var scale = new Vector3(
                GetFloat("ScaleX", defaultX, rt),
                GetFloat("ScaleY", defaultY, rt),
                GetFloat("ScaleZ", defaultZ, rt));

            if (mode == ScaleMode.Absolute)
                target.transform.localScale = scale;
            else
                target.transform.localScale = Vector3.Scale(target.transform.localScale, scale);

            TriggerFlow("FlowOut", rt);
        }

        public override object GetOutputValue(string p, IGraphRuntime rt) => null;

        public override List<PortDefinition> GetPortDefinitions() => new()
        {
            new PortDefinition("FlowIn", "flow", false),
            new PortDefinition("Target", "gameobject", false),
            new PortDefinition("ScaleX", "float", false),
            new PortDefinition("ScaleY", "float", false),
            new PortDefinition("ScaleZ", "float", false),
            new PortDefinition("FlowOut", "flow", true),
        };
    }
}