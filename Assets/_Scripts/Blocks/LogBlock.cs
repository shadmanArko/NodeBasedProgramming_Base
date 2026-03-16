using System.Collections.Generic;
using _Scripts.Core;
using UnityEngine;

namespace _Scripts.Blocks
{
    public class LogBlock : BaseBlock
    {
        public override string BlockType => "Log";
        public string defaultMessage = "Hello from NodeGraph!";
        public bool logFloatValue = false;

        public override void Execute(IGraphRuntime rt)
        {
            var msg = GetString("Message", defaultMessage, rt);
            if (logFloatValue)
                Debug.Log($"[Log] {msg}  |  {GetFloat("FloatValue", 0f, rt)}");
            else
                Debug.Log($"[Log] {msg}");
            TriggerFlow("FlowOut", rt);
        }

        public override object GetOutputValue(string p, IGraphRuntime rt) => null;

        public override List<PortDefinition> GetPortDefinitions() => new()
        {
            new PortDefinition("FlowIn", "flow", false),
            new PortDefinition("Message", "string", false),
            new PortDefinition("FloatValue", "float", false),
            new PortDefinition("FlowOut", "flow", true),
        };
    }
}