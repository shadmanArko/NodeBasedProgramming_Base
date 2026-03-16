using UnityEngine;

namespace _Scripts.Events
{
    public class GraphEvents
    {
        public readonly struct GraphExecutedEvent
        {
            public readonly int SpawnedCount;
            public GraphExecutedEvent(int spawnedCount) => SpawnedCount = spawnedCount;
        }

        public readonly struct GraphClearedEvent
        {
            public readonly int DestroyedCount;
            public GraphClearedEvent(int destroyedCount) => DestroyedCount = destroyedCount;
        }

        public readonly struct GraphExportedEvent
        {
            public readonly string Path;
            public GraphExportedEvent(string path) => Path = path;
        }

        public readonly struct GraphImportedEvent
        {
            public readonly string Path;
            public readonly int BlockCount;

            public GraphImportedEvent(string path, int blockCount)
            {
                Path = path;
                BlockCount = blockCount;
            }
        }

        public readonly struct BlockAddedEvent
        {
            public readonly string BlockId;
            public readonly string BlockType;

            public BlockAddedEvent(string blockId, string blockType)
            {
                BlockId = blockId;
                BlockType = blockType;
            }
        }

        public readonly struct BlockRemovedEvent
        {
            public readonly string BlockId;
            public BlockRemovedEvent(string blockId) => BlockId = blockId;
        }

        public readonly struct ConnectionAddedEvent
        {
            public readonly string FromBlockId;
            public readonly string FromPort;
            public readonly string ToBlockId;
            public readonly string ToPort;

            public ConnectionAddedEvent(string fromBlockId, string fromPort,
                string toBlockId, string toPort)
            {
                FromBlockId = fromBlockId;
                FromPort = fromPort;
                ToBlockId = toBlockId;
                ToPort = toPort;
            }
        }

        public readonly struct ConnectionRemovedEvent
        {
            public readonly int Index;
            public ConnectionRemovedEvent(int index) => Index = index;
        }

        public readonly struct GraphErrorEvent
        {
            public readonly string Message;
            public GraphErrorEvent(string message) => Message = message;
        }

        public readonly struct GraphStatusChangedEvent
        {
            public readonly string Message;
            public GraphStatusChangedEvent(string message) => Message = message;
        }

        public readonly struct RuntimeObjectSpawnedEvent
        {
            public readonly GameObject GameObject;
            public readonly string SpawnedBy;

            public RuntimeObjectSpawnedEvent(GameObject go, string spawnedBy)
            {
                GameObject = go;
                SpawnedBy = spawnedBy;
            }
        }
    }
}