using System.Collections.Generic;
using System.Linq;
using _Scripts.Config;
using _Scripts.Core;
using _Scripts.Events;
using _Scripts.Models;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Scripts.Services.Execution
{
    public class GraphExecutionService : IGraphExecutionService, IGraphRuntime
    {
        private readonly IGraphModel _model;
        private readonly IEventBus _eventBus;
        private readonly NodeGraphConfig _config;

        private Dictionary<(string blockId, string port), BlockConnection> _inputIndex;
        private Dictionary<(string blockId, string port), List<(BaseBlock, string)>> _flowIndex;

        private int _depth;

        public int ExecutionDepth => _depth;
        public int MaxExecutionDepth => _config.maxExecutionDepth;

        public GraphExecutionService(
            IGraphModel model,
            IEventBus eventBus,
            NodeGraphConfig config)
        {
            _model = model;
            _eventBus = eventBus;
            _config = config;
        }


        public async UniTask ExecuteAsync()
        {
            if (_model.IsRunning.Value)
            {
                _model.SetStatus("Already running.");
                return;
            }

            _model.SetIsRunning(true);
            _model.SetStatus("Running…");

            if (_model.ClearOnRun.Value)
                _model.ClearRuntimeObjects();

            BuildIndexes();
            _depth = 0;

            var entryBlocks = FindEntryBlocks();

            if (entryBlocks.Count == 0)
            {
                const string warn =
                    "No entry blocks found. At least one block needs FlowIn with no incoming connection.";
                Debug.LogWarning($"[NodeGraph] {warn}");
                _model.SetStatus(warn);
                _model.SetIsRunning(false);
                _eventBus.Publish(new GraphEvents.GraphErrorEvent(warn));
                return;
            }

            await UniTask.Yield();

            foreach (var block in entryBlocks)
            {
                if (_config.verboseLogging)
                    Debug.Log($"[NodeGraph] Entry: {block.name} ({block.BlockType})");

                block.Execute(this);
            }

            var spawned = _model.GetRuntimeObjects().Count;
            _model.SetStatus($"Done — {spawned} object in scene.");
            _model.SetIsRunning(false);

            _eventBus.Publish(new GraphEvents.GraphExecutedEvent(spawned));
        }

        public object GetInputValue(string blockId, string portName)
        {
            if (_inputIndex.TryGetValue((blockId, portName), out var conn))
            {
                var src = _model.GetBlock(conn.fromBlockId);
                return src?.GetOutputValue(conn.fromPortName, this);
            }

            return null;
        }

        public IReadOnlyList<(BaseBlock block, string inPort)> GetFlowTargets(string blockId, string outPort)
        {
            _depth++;
            return _flowIndex.TryGetValue((blockId, outPort), out var list)
                ? list
                : System.Array.Empty<(BaseBlock, string)>();
        }

        public void RegisterRuntimeObject(GameObject go)
        {
            _model.RegisterRuntimeObject(go);
            _eventBus.Publish(new GraphEvents.RuntimeObjectSpawnedEvent(go, go.name));
        }

        private void BuildIndexes()
        {
            var connections = _model.Connections;

            _inputIndex = new Dictionary<(string, string), BlockConnection>(connections.Count);
            _flowIndex = new Dictionary<(string, string), List<(BaseBlock, string)>>();

            foreach (var conn in connections)
            {
                _inputIndex[(conn.toBlockId, conn.toPortName)] = conn;

                var key = (conn.fromBlockId, conn.fromPortName);
                if (!_flowIndex.ContainsKey(key))
                    _flowIndex[key] = new List<(BaseBlock, string)>();

                var target = _model.GetBlock(conn.toBlockId);
                if (target != null)
                    _flowIndex[key].Add((target, conn.toPortName));
            }
        }

        private List<BaseBlock> FindEntryBlocks()
        {
            var connectedFlowIns = _model.Connections
                .Where(c => c.toPortName == "FlowIn")
                .Select(c => c.toBlockId)
                .ToHashSet();

            return _model.Blocks
                .Where(b => b != null
                            && b.GetPortDefinitions().Any(p => p.name == "FlowIn" && !p.isOutput)
                            && !connectedFlowIns.Contains(b.blockId))
                .ToList();
        }
    }
}