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
        private readonly IEventBus _eventBus;

        public GraphSerializationService(IGraphModel model, IEventBus eventBus)
        {
            _model = model;
            _eventBus = eventBus;
        }

        public async UniTask ExportAsync(string path)
        {
            try
            {
                _model.SetStatus("Exporting…");

                var data = GraphSerializer.Serialize(_model);
                var json = JsonUtility.ToJson(data, true);

                // Offload file I/O off the main thread
                await UniTask.RunOnThreadPool(() =>
                {
                    var dir = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    File.WriteAllText(path, json);
                });

                var msg = $"Exported to: {path}";
                Debug.Log($"[NodeGraph] {msg}");
                _model.SetStatus(msg);
                _model.SetJsonPath(path);

                _eventBus.Publish(new GraphEvents.GraphExportedEvent(path));

#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif
            }
            catch (Exception ex)
            {
                var msg = $"Export failed: {ex.Message}";
                Debug.LogError($"[NodeGraph] {msg}");
                _model.SetStatus(msg);
                _eventBus.Publish(new GraphEvents.GraphErrorEvent(msg));
            }
        }


        public async UniTask ImportAsync(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    var notFound = $"File not found: {path}";
                    Debug.LogError($"[NodeGraph] {notFound}");
                    _model.SetStatus(notFound);
                    _eventBus.Publish(new GraphEvents.GraphErrorEvent(notFound));
                    return;
                }

                _model.SetStatus("Importing…");

                string json = null;
                await UniTask.RunOnThreadPool(() => json = File.ReadAllText(path));

                var data = JsonUtility.FromJson<GraphData>(json);
                await GraphSerializer.DeserializeAsync(data, _model);

                var msg = $"Imported {_model.Blocks.Count} block(s) from: {path}";
                Debug.Log($"[NodeGraph] {msg}");
                _model.SetStatus(msg);
                _model.SetJsonPath(path);

                _eventBus.Publish(new GraphEvents.GraphImportedEvent(path, _model.Blocks.Count));
            }
            catch (Exception ex)
            {
                var msg = $"Import failed: {ex.Message}";
                Debug.LogError($"[NodeGraph] {msg}");
                _model.SetStatus(msg);
                _eventBus.Publish(new GraphEvents.GraphErrorEvent(msg));
            }
        }
    }
}