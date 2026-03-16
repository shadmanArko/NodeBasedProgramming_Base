using System;
using _Scripts.Core;
using UniRx;
using UnityEngine;

namespace _Scripts.Views
{
    public sealed class GraphView : MonoBehaviour
    {
        public Transform blockParent;

        public Transform BlockParent
        {
            get
            {
                if (blockParent != null && blockParent.gameObject.scene.IsValid())
                    return blockParent;
                return transform;
            }
        }

        private readonly Subject<Unit> _onRunRequested = new Subject<Unit>();
        private readonly Subject<Unit> _onClearRequested = new Subject<Unit>();
        private readonly Subject<Unit> _onExportRequested = new Subject<Unit>();
        private readonly Subject<Unit> _onImportRequested = new Subject<Unit>();
        private readonly Subject<bool> _onClearOnRunChanged = new Subject<bool>();
        private readonly Subject<string> _onJsonPathChanged = new Subject<string>();
        private readonly Subject<string> _onAddBlockRequested = new Subject<string>();
        private readonly Subject<string> _onRemoveBlockRequested = new Subject<string>();
        private readonly Subject<BlockConnection> _onAddConnectionRequested = new Subject<BlockConnection>();
        private readonly Subject<int> _onRemoveConnectionRequested = new Subject<int>();

        public IObservable<Unit> OnRunRequested => _onRunRequested;
        public IObservable<Unit> OnClearRequested => _onClearRequested;
        public IObservable<Unit> OnExportRequested => _onExportRequested;
        public IObservable<Unit> OnImportRequested => _onImportRequested;
        public IObservable<bool> OnClearOnRunChanged => _onClearOnRunChanged;
        public IObservable<string> OnJsonPathChanged => _onJsonPathChanged;
        public IObservable<string> OnAddBlockRequested => _onAddBlockRequested;
        public IObservable<string> OnRemoveBlockRequested => _onRemoveBlockRequested;
        public IObservable<BlockConnection> OnAddConnectionRequested => _onAddConnectionRequested;
        public IObservable<int> OnRemoveConnectionRequested => _onRemoveConnectionRequested;

        public void SetStatus(string message) => Debug.Log($"[GraphView] Status: {message}");
        public void SetRunningState(bool isRunning) => Debug.Log($"[GraphView] Running: {isRunning}");
        public void SetClearOnRunToggle(bool value) => Debug.Log($"[GraphView] ClearOnRun: {value}");
        public void SetRuntimeObjectCount(int count) => Debug.Log($"[GraphView] Objects in scene: {count}");
        public void SetBlockCount(int count) => Debug.Log($"[GraphView] Blocks: {count}");
        public void SetConnectionCount(int count) => Debug.Log($"[GraphView] Connections: {count}");
        public void ShowError(string message) => Debug.LogError($"[GraphView] Error: {message}");

        public void RequestRun() => _onRunRequested.OnNext(Unit.Default);
        public void RequestClear() => _onClearRequested.OnNext(Unit.Default);
        public void RequestExport() => _onExportRequested.OnNext(Unit.Default);
        public void RequestImport() => _onImportRequested.OnNext(Unit.Default);
        public void RequestClearOnRunChange(bool v) => _onClearOnRunChanged.OnNext(v);
        public void RequestJsonPathChange(string path) => _onJsonPathChanged.OnNext(path);
        public void RequestAddBlock(string blockType) => _onAddBlockRequested.OnNext(blockType);
        public void RequestRemoveBlock(string blockId) => _onRemoveBlockRequested.OnNext(blockId);

        public void RequestAddConnection(string fromBlockId, string fromPort, string toBlockId, string toPort)
        {
            _onAddConnectionRequested.OnNext(new BlockConnection(fromBlockId, fromPort, toBlockId, toPort));
        }

        public void RequestRemoveConnection(int index) => _onRemoveConnectionRequested.OnNext(index);

        private void OnDestroy()
        {
            _onRunRequested.Dispose();
            _onClearRequested.Dispose();
            _onExportRequested.Dispose();
            _onImportRequested.Dispose();
            _onClearOnRunChanged.Dispose();
            _onJsonPathChanged.Dispose();
            _onAddBlockRequested.Dispose();
            _onRemoveBlockRequested.Dispose();
            _onAddConnectionRequested.Dispose();
            _onRemoveConnectionRequested.Dispose();
        }
    }
}