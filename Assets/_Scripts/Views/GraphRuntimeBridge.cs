using System;
using System.Collections.Generic;
using _Scripts.Models;
using UniRx;
using UnityEngine;
using Zenject;

namespace _Scripts.Views
{
    public class GraphRuntimeBridge : MonoBehaviour
    {
        [SerializeField] private bool isRunning;
        [SerializeField] private bool clearOnRun;
        [SerializeField] private string statusMessage;
        [SerializeField] private int blockCount;
        [SerializeField] private int connectionCount;
        [SerializeField] private int runtimeObjectCount;

        [SerializeField] private List<BlockSnapshot> blocks = new List<BlockSnapshot>();
        [SerializeField] private List<ConnectionSnapshot> connections = new List<ConnectionSnapshot>();

        [Serializable]
        public class BlockSnapshot
        {
            public string id;
            public string type;
            public string gameObjectName;
        }

        [Serializable]
        public class ConnectionSnapshot
        {
            public string from;
            public string to;
            public string fromId;
            public string toId;
        }


        [Inject] private IGraphModel _model;

        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        private void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            _model.IsRunning
                .Subscribe(v => isRunning = v)
                .AddTo(_disposables);

            _model.ClearOnRun
                .Subscribe(v => clearOnRun = v)
                .AddTo(_disposables);

            _model.StatusMessage
                .Subscribe(v => statusMessage = v)
                .AddTo(_disposables);

            _model.RuntimeObjectCount
                .Subscribe(v => runtimeObjectCount = v)
                .AddTo(_disposables);

            _model.Blocks.ObserveCountChanged()
                .StartWith(_model.Blocks.Count)
                .Subscribe(_ => RebuildBlockSnapshots())
                .AddTo(_disposables);

            _model.Connections.ObserveCountChanged()
                .StartWith(_model.Connections.Count)
                .Subscribe(_ => RebuildConnectionSnapshots())
                .AddTo(_disposables);
        }


        private void RebuildBlockSnapshots()
        {
            blocks.Clear();
            blockCount = _model.Blocks.Count;

            foreach (var b in _model.Blocks)
            {
                if (b == null) continue;
                blocks.Add(new BlockSnapshot
                {
                    id = b.blockId,
                    type = b.BlockType,
                    gameObjectName = b.name
                });
            }
        }

        private void RebuildConnectionSnapshots()
        {
            connections.Clear();
            connectionCount = _model.Connections.Count;

            foreach (var c in _model.Connections)
            {
                var fromBlock = _model.GetBlock(c.fromBlockId);
                var toBlock = _model.GetBlock(c.toBlockId);

                string fromName = fromBlock != null ? fromBlock.name : c.fromBlockId;
                string toName = toBlock != null ? toBlock.name : c.toBlockId;

                connections.Add(new ConnectionSnapshot
                {
                    from = $"{fromName}  ›  {c.fromPortName}",
                    to = $"{toName}  ›  {c.toPortName}",
                    fromId = c.fromBlockId,
                    toId = c.toBlockId
                });
            }
        }

        public IReadOnlyList<BlockSnapshot> BlockSnapshots => blocks;
        public IReadOnlyList<ConnectionSnapshot> ConnectionSnapshots => connections;
        public bool IsRunning => isRunning;
        public string StatusMessage => statusMessage;
        public int BlockCount => blockCount;
        public int ConnectionCount => connectionCount;
        public int RuntimeObjectCount => runtimeObjectCount;

        private void OnDestroy() => _disposables.Dispose();
    }
}