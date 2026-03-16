using _Scripts.Core;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace _Scripts.Models
{
    public interface IGraphModel
    {
        IReadOnlyReactiveProperty<bool> IsRunning { get; }
        IReadOnlyReactiveProperty<bool> ClearOnRun { get; }
        IReadOnlyReactiveProperty<string> JsonPath { get; }
        IReadOnlyReactiveProperty<string> StatusMessage { get; }
        IReadOnlyReactiveProperty<int> RuntimeObjectCount { get; }
        IReadOnlyReactiveCollection<BaseBlock> Blocks { get; }
        IReadOnlyReactiveCollection<BlockConnection> Connections { get; }
        
        BaseBlock GetBlock(string blockId);
        IReadOnlyList<GameObject> GetRuntimeObjects();
        
        void AddBlock(BaseBlock block);
        void RemoveBlock(string blockId);

        void AddConnection(BlockConnection connection);
        void RemoveConnectionAt(int index);
        void RemoveConnectionsFor(string blockId);
        
        void RegisterRuntimeObject(GameObject go);
        void ClearRuntimeObjects();

        void SetClearOnRun(bool value);
        void SetJsonPath(string path);
        void SetIsRunning(bool value);
        void SetStatus(string message);
    }
}