using System.Collections.Generic;
using System.Linq;
using _Scripts.Core;
using UniRx;
using UnityEngine;

namespace _Scripts.Models
{
    public class GraphModel : IGraphModel
    {
        private readonly ReactiveProperty<bool> _isRunning = new ReactiveProperty<bool>(false);
        private readonly ReactiveProperty<bool> _clearOnRun = new ReactiveProperty<bool>(true);
        private readonly ReactiveProperty<string> _jsonPath = new ReactiveProperty<string>();
        private readonly ReactiveProperty<string> _statusMessage = new ReactiveProperty<string>("Ready");
        private readonly ReactiveProperty<int> _runtimeObjectCount = new ReactiveProperty<int>(0);
        private readonly ReactiveCollection<BaseBlock> _blocks = new ReactiveCollection<BaseBlock>();
        private readonly ReactiveCollection<BlockConnection> _connections = new ReactiveCollection<BlockConnection>();
        private readonly List<GameObject> _runtimeObjects = new List<GameObject>();

        public IReadOnlyReactiveProperty<bool> IsRunning => _isRunning;
        public IReadOnlyReactiveProperty<bool> ClearOnRun => _clearOnRun;
        public IReadOnlyReactiveProperty<string> JsonPath => _jsonPath;
        public IReadOnlyReactiveProperty<string> StatusMessage => _statusMessage;
        public IReadOnlyReactiveProperty<int> RuntimeObjectCount => _runtimeObjectCount;
        public IReadOnlyReactiveCollection<BaseBlock> Blocks => _blocks;
        public IReadOnlyReactiveCollection<BlockConnection> Connections => _connections;

        public BaseBlock GetBlock(string blockId) => _blocks.FirstOrDefault(b => b != null && b.blockId == blockId);
        public IReadOnlyList<GameObject> GetRuntimeObjects() => _runtimeObjects;


        public void AddBlock(BaseBlock block)
        {
            if (block == null) return;
            block.EnsureId();
            _blocks.Add(block);
        }

        public void RemoveBlock(string blockId)
        {
            var block = GetBlock(blockId);
            if (block == null) return;

            RemoveConnectionsFor(blockId);
            _blocks.Remove(block);
        }

        public void AddConnection(BlockConnection connection)
        {
            if (connection == null) return;
            _connections.Add(connection);
        }

        public void RemoveConnectionAt(int index)
        {
            if (index < 0 || index >= _connections.Count) return;
            _connections.RemoveAt(index);
        }

        public void RemoveConnectionsFor(string blockId)
        {
            var toRemove = _connections
                .Where(c => c.fromBlockId == blockId || c.toBlockId == blockId)
                .ToList();

            foreach (var c in toRemove)
                _connections.Remove(c);
        }
        
        public void RegisterRuntimeObject(GameObject go)
        {
            if (go == null) return;
            _runtimeObjects.Add(go);
            _runtimeObjectCount.Value = _runtimeObjects.Count;
        }

        public void ClearRuntimeObjects()
        {
            int count = 0;
            foreach (var go in _runtimeObjects)
            {
                if (go != null)
                {
                    Object.Destroy(go);
                    count++;
                }
            }

            _runtimeObjects.Clear();
            _runtimeObjectCount.Value = 0;

            // Reset all variable blocks
            foreach (var block in _blocks)
                if (block is Blocks.VariableBlock vb)
                    vb.Reset();

            if (count > 0)
                SetStatus($"Cleared {count} object");
        }
        
        public void SetClearOnRun(bool value) => _clearOnRun.Value = value;
        public void SetJsonPath(string path) => _jsonPath.Value = path;
        public void SetIsRunning(bool value) => _isRunning.Value = value;
        public void SetStatus(string message) => _statusMessage.Value = message;
    }
}