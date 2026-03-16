using System.Collections.Generic;
using _Scripts.Core;

namespace _Scripts.Blocks
{
    public class BranchBlock : BaseBlock
    {
        public override string BlockType => "Branch";
        public bool defaultCondition = true;

        public override void Execute(IGraphRuntime rt)
        {
            bool condition = GetBool("Condition", defaultCondition, rt);
            TriggerFlow(condition ? "True" : "False", rt);
        }

        public override object GetOutputValue(string portName, IGraphRuntime runtime) => null;

        public override List<PortDefinition> GetPortDefinitions() => new()
        {
            new PortDefinition("FlowIn", "flow", false),
            new PortDefinition("Condition", "bool", false),
            new PortDefinition("True", "flow", true),
            new PortDefinition("False", "flow", true),
        };
    }
}