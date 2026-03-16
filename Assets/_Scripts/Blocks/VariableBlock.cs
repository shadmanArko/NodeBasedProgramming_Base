using System.Collections.Generic;
using _Scripts.Core;
using UnityEngine;

namespace _Scripts.Blocks
{
    public class VariableBlock : BaseBlock
    {
        public override string BlockType => "Variable";

        public enum Operation
        {
            None,
            Add,
            Subtract,
            Multiply,
            Divide
        }

        public float initialValue = 0f;
        public Operation operation = Operation.None;
        public float operand = 1f;

        [HideInInspector] public float currentValue;

        protected override void OnValidate()
        {
            base.OnValidate();
            currentValue = initialValue;
        }

        private void Awake() => currentValue = initialValue;
        public void Reset() => currentValue = initialValue;

        public override void Execute(IGraphRuntime rt)
        {
            float op = GetFloat("Operand", operand, rt);
            currentValue = operation switch
            {
                Operation.Add => currentValue + op,
                Operation.Subtract => currentValue - op,
                Operation.Multiply => currentValue * op,
                Operation.Divide => Mathf.Abs(op) > 0.0001f ? currentValue / op : currentValue,
                _ => currentValue
            };
            TriggerFlow("FlowOut", rt);
        }

        public override object GetOutputValue(string portName, IGraphRuntime rt) =>
            portName == "Value" ? (object)currentValue : null;

        public override List<PortDefinition> GetPortDefinitions() => new()
        {
            new PortDefinition("FlowIn", "flow", false),
            new PortDefinition("Operand", "float", false),
            new PortDefinition("FlowOut", "flow", true),
            new PortDefinition("Value", "float", true),
        };
    }
}