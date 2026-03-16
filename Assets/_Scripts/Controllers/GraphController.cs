using _Scripts.Events;
using _Scripts.Models;
using _Scripts.Services.Execution;
using _Scripts.Services.Factory;
using _Scripts.Services.Serialization;
using _Scripts.Views;
using Cysharp.Threading.Tasks;
using UniRx;
using Zenject;

namespace _Scripts.Controllers
{
    public class GraphController : IGraphController
    {
        private readonly IGraphModel _model;
        private readonly IGraphExecutionService _executionService;
        private readonly IGraphSerializationService _serializationService;
        private readonly IBlockFactoryService _blockFactory;
        private readonly IEventBus _eventBus;
        private readonly GraphView _view;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public GraphController(
            IGraphModel model,
            IGraphExecutionService executionService,
            IGraphSerializationService serializationService,
            IBlockFactoryService blockFactory,
            IEventBus eventBus,
            GraphView view)
        {
            _model = model;
            _executionService = executionService;
            _serializationService = serializationService;
            _blockFactory = blockFactory;
            _eventBus = eventBus;
            _view = view;
        }
        
        public void Initialize()
        {
            BindModelToView();
            BindViewToModel();
            BindEventBus();
            UnityEngine.Debug.Log("[GraphController] Initialized — all bindings active.");
        }
        
        public void Dispose() => _disposables.Dispose();
        
        private void BindModelToView()
        {
            _model.StatusMessage
                .Subscribe(_view.SetStatus)
                .AddTo(_disposables);

            _model.IsRunning
                .Subscribe(_view.SetRunningState)
                .AddTo(_disposables);

            _model.ClearOnRun
                .Subscribe(_view.SetClearOnRunToggle)
                .AddTo(_disposables);

            _model.RuntimeObjectCount
                .Subscribe(_view.SetRuntimeObjectCount)
                .AddTo(_disposables);

            _model.Blocks.ObserveCountChanged()
                .StartWith(_model.Blocks.Count)
                .Subscribe(_view.SetBlockCount)
                .AddTo(_disposables);

            _model.Connections.ObserveCountChanged()
                .StartWith(_model.Connections.Count)
                .Subscribe(_view.SetConnectionCount)
                .AddTo(_disposables);
        }


        private void BindViewToModel()
        {
            _view.OnRunRequested
                .Where(_ => !_model.IsRunning.Value)
                .Subscribe(_ => _executionService.ExecuteAsync().Forget())
                .AddTo(_disposables);

            _view.OnClearRequested
                .Subscribe(_ =>
                {
                    _model.ClearRuntimeObjects();
                    _eventBus.Publish(new GraphEvents.GraphClearedEvent(0));
                })
                .AddTo(_disposables);

            _view.OnExportRequested
                .Subscribe(_ => _serializationService
                    .ExportAsync(_model.JsonPath.Value).Forget())
                .AddTo(_disposables);

            _view.OnImportRequested
                .Subscribe(_ => _serializationService
                    .ImportAsync(_model.JsonPath.Value).Forget())
                .AddTo(_disposables);

            _view.OnClearOnRunChanged
                .Subscribe(_model.SetClearOnRun)
                .AddTo(_disposables);

            _view.OnJsonPathChanged
                .Subscribe(_model.SetJsonPath)
                .AddTo(_disposables);

            _view.OnAddBlockRequested
                .Subscribe(blockType =>
                {
                    var block = _blockFactory.Create(blockType, _view.BlockParent);
                    if (block == null) return;
                    _model.AddBlock(block);
                    _eventBus.Publish(new GraphEvents.BlockAddedEvent(block.blockId, block.BlockType));
                })
                .AddTo(_disposables);

            _view.OnRemoveBlockRequested
                .Subscribe(blockId =>
                {
                    _model.RemoveBlock(blockId);
                    _eventBus.Publish(new GraphEvents.BlockRemovedEvent(blockId));
                })
                .AddTo(_disposables);

            _view.OnAddConnectionRequested
                .Subscribe(conn =>
                {
                    _model.AddConnection(conn);
                    _eventBus.Publish(new GraphEvents.ConnectionAddedEvent(
                        conn.fromBlockId, conn.fromPortName,
                        conn.toBlockId, conn.toPortName));
                })
                .AddTo(_disposables);

            _view.OnRemoveConnectionRequested
                .Subscribe(index =>
                {
                    _model.RemoveConnectionAt(index);
                    _eventBus.Publish(new GraphEvents.ConnectionRemovedEvent(index));
                })
                .AddTo(_disposables);
        }
        
        private void BindEventBus()
        {
            _eventBus.OnEvent<GraphEvents.GraphErrorEvent>()
                .Subscribe(e => _view.ShowError(e.Message))
                .AddTo(_disposables);
        }
    }
}