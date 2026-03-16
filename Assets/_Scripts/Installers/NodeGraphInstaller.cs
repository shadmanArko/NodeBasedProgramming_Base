using System;
using _Scripts.Config;
using _Scripts.Controllers;
using _Scripts.Events;
using _Scripts.Models;
using _Scripts.Services.Execution;
using _Scripts.Services.Factory;
using _Scripts.Services.Serialization;
using _Scripts.Views;
using UniRx;
using UnityEngine;
using Zenject;

namespace _Scripts.Installers
{
    [CreateAssetMenu(fileName = "NodeGraphInstaller", menuName = "Installers/NodeGraphInstaller")]
    public class NodeGraphInstaller : ScriptableObjectInstaller<NodeGraphInstaller>
    {
        [SerializeField] private NodeGraphConfig graphConfig;
        [SerializeField] private GraphView graphView;

        public override void InstallBindings()
        {
            Container.Bind<CompositeDisposable>().AsSingle();
            
            Container.BindInterfacesTo<GameSceneInit>().AsSingle().NonLazy();
            
            Container.Bind<GraphRuntimeBridge>().AsSingle();

            Container.BindInstance(graphConfig).AsSingle();

            Container.BindInterfacesTo<SimpleEventBus>().AsSingle();

            Container.BindInterfacesTo<GraphModel>().AsSingle();

            Container.BindInterfacesTo<BlockFactoryService>().AsSingle();

            Container.BindInterfacesTo<GraphExecutionService>().AsSingle();

            Container.BindInterfacesTo<GraphSerializationService>().AsSingle();
            
            Container.BindInterfacesTo<GraphController>().AsSingle();
            
            Container.Bind<GraphView>().FromComponentInNewPrefab(graphView).AsSingle();
        }
        
    }
    
    public class GameSceneInit : IInitializable, IDisposable
    {
        [Inject] private CompositeDisposable _disposables;
        public void Dispose()
        {
            _disposables?.Dispose();
        }

        public void Initialize()
        {
        
        }
    }
}