using System;
using System.IO;
using _Scripts.Events;
using _Scripts.Models;
using _Scripts.Serialization;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Scripts.Services.Serialization
{
    public sealed class GraphSerializationService : IGraphSerializationService
    {
        private readonly IGraphModel _model;
        private readonly IEventBus   _eventBus;

        public GraphSerializationService(IGraphModel model, IEventBus eventBus)
        {
            _model    = model;
            _eventBus = eventBus;
        }
        
        public async UniTask ExportAsync(string path)
        {
            //todo implement
        }

       
        public async UniTask ImportAsync(string path)
        {
            //todo implement
        }
    }
}