using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Serialization
{
    [Serializable]
    public class GraphData
    {
        public string version = "2.0";
        public string exportedAt;
        public List<BlockData> blocks = new List<BlockData>();
        public List<ConnectionData> connections = new List<ConnectionData>();
        public List<RuntimeObjectData> runtimeObjects = new List<RuntimeObjectData>();
    }

    [Serializable]
    public class BlockData
    {
        public string blockId, blockType, gameObjectName;
        public SerializableVector3 position;
        public string propertiesJson;
    }

    [Serializable]
    public class ConnectionData
    {
        public string fromBlockId, fromPortName, toBlockId, toPortName;
    }

    [Serializable]
    public class RuntimeObjectData
    {
        public string name, primitiveType;
        public SerializableVector3 position, rotation, scale;
    }

    [Serializable]
    public class SerializableVector3
    {
        public float x, y, z;

        public SerializableVector3()
        {
        }

        public SerializableVector3(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public Vector3 ToVector3() => new Vector3(x, y, z);
    }

    [Serializable]
    public class SpawnObjectProps
    {
        public string primitiveType, objectName;
        public float defaultX, defaultY, defaultZ;
    }

    [Serializable]
    public class MoveObjectProps
    {
        public string mode;
        public float defaultX, defaultY, defaultZ;
    }

    [Serializable]
    public class RotateObjectProps
    {
        public string mode;
        public float defaultPitch, defaultYaw, defaultRoll;
    }

    [Serializable]
    public class ScaleObjectProps
    {
        public string mode;
        public float defaultX, defaultY, defaultZ;
    }

    [Serializable]
    public class VariableProps
    {
        public float initialValue, operand;
        public string operation;
    }

    [Serializable]
    public class CompareProps
    {
        public float defaultA, defaultB;
        public string compareOp;
    }

    [Serializable]
    public class BranchProps
    {
        public bool defaultCondition;
    }

    [Serializable]
    public class LogProps
    {
        public string defaultMessage;
        public bool logFloatValue;
    }
}