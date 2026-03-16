using System.Collections.Generic;
using _Scripts.Core;
using UnityEngine;

namespace _Scripts.Blocks
{
    public class CompareBlock : BaseBlock
    {
        public override string BlockType => "Compare";

        public enum CompareOperator
        {
            Equal,
            NotEqual,
            GreaterThan,
            GreaterOrEqual,
            LessThan,
            LessOrEqual
        }

        public float defaultA = 0f, defaultB = 0f;
        public CompareOperator compareOp = CompareOperator.GreaterThan;

        public override void Execute(IGraphRuntime rt) => TriggerFlow("FlowOut", rt);

        public override object GetOutputValue(string portName, IGraphRuntime rt)
        {
            if (portName != "Result") return null;
            float a = GetFloat("A", defaultA, rt);
            float b = GetFloat("B", defaultB, rt);
            return compareOp switch
            {
                CompareOperator.Equal => Mathf.Approximately(a, b),
                CompareOperator.NotEqual => !Mathf.Approximately(a, b),
                CompareOperator.GreaterThan => a > b,
                CompareOperator.GreaterOrEqual => a >= b,
                CompareOperator.LessThan => a < b,
                CompareOperator.LessOrEqual => a <= b,
                _ => false
            };
        }

        public override List<PortDefinition> GetPortDefinitions() => new()
        {
            new PortDefinition("FlowIn", "flow", false),
            new PortDefinition("A", "float", false),
            new PortDefinition("B", "float", false),
            new PortDefinition("FlowOut", "flow", true),
            new PortDefinition("Result", "bool", true),
        };
    }
}