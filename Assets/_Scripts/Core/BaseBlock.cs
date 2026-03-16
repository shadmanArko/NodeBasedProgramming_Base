using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Core
{
    public abstract class BaseBlock : MonoBehaviour
    {
        public string blockId;
        public abstract string BlockType { get; }
        public abstract void Execute(IGraphRuntime runtime);
        public abstract object GetOutputValue(string portName, IGraphRuntime runtime);
        public abstract List<PortDefinition> GetPortDefinitions();
        protected virtual void OnValidate() => EnsureId();

        public void EnsureId()
        {
            if (string.IsNullOrEmpty(blockId))
                blockId = System.Guid.NewGuid().ToString("N").Substring(0, 8);
        }

        protected float GetFloat(string port, float fallback, IGraphRuntime rt)
        {
            var v = rt.GetInputValue(blockId, port);
            return v switch
            {
                float f => f,
                int i => (float)i,
                double d => (float)d,
                _ => fallback
            };
        }

        protected bool GetBool(string port, bool fallback, IGraphRuntime rt)
        {
            var v = rt.GetInputValue(blockId, port);
            return v is bool b ? b : fallback;
        }

        protected string GetString(string port, string fallback, IGraphRuntime rt)
        {
            var v = rt.GetInputValue(blockId, port);
            return v is string s ? s : fallback;
        }

        protected GameObject GetGameObject(string port, GameObject fallback, IGraphRuntime rt)
        {
            var v = rt.GetInputValue(blockId, port);
            return v is GameObject go ? go : fallback;
        }

        protected void TriggerFlow(string flowOutPort, IGraphRuntime rt)
        {
            if (rt.ExecutionDepth >= rt.MaxExecutionDepth)
            {
                Debug.LogError($"Possible infinite loop aborting for'{name}'");
                return;
            }

            foreach (var (nextBlock, _) in rt.GetFlowTargets(blockId, flowOutPort))
                nextBlock.Execute(rt);
        }
    }
}