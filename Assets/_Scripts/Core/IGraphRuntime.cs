using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Core
{
    public interface IGraphRuntime
    {
        object GetInputValue(string blockId, string portName);
        IReadOnlyList<(BaseBlock block, string inPort)> GetFlowTargets(string blockId, string outPort);
        void RegisterRuntimeObject(GameObject go);
        int ExecutionDepth { get; }
        int MaxExecutionDepth { get; }
    }
}