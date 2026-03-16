using System.Collections.Generic;
using _Scripts.Core;
using UnityEngine;

namespace _Scripts.Blocks
{
    public class SpawnObjectBlock : BaseBlock
    {
        public override string BlockType => "SpawnObject";

        public enum PrimitiveChoice
        {
            Cube,
            Sphere,
            Cylinder,
            Capsule
        }

        [Header("Settings")] public PrimitiveChoice primitiveType = PrimitiveChoice.Cube;
        public string objectName = "SpawnedObject";

        [Header("Default Position")] public float defaultX = 0f;
        public float defaultY = 0f;
        public float defaultZ = 0f;

        private GameObject _spawnedObject;

        public override void Execute(IGraphRuntime rt)
        {
            float x = GetFloat("PosX", defaultX, rt);
            float y = GetFloat("PosY", defaultY, rt);
            float z = GetFloat("PosZ", defaultZ, rt);

            var prim = primitiveType switch
            {
                PrimitiveChoice.Sphere => PrimitiveType.Sphere,
                PrimitiveChoice.Cylinder => PrimitiveType.Cylinder,
                PrimitiveChoice.Capsule => PrimitiveType.Capsule,
                _ => PrimitiveType.Cube
            };

            _spawnedObject = GameObject.CreatePrimitive(prim);
            _spawnedObject.name = string.IsNullOrEmpty(objectName) ? prim.ToString() : objectName;
            _spawnedObject.transform.position = new Vector3(x, y, z);

            rt.RegisterRuntimeObject(_spawnedObject);
            TriggerFlow("FlowOut", rt);
        }

        public override object GetOutputValue(string portName, IGraphRuntime rt) =>
            portName == "SpawnedObject" ? _spawnedObject : null;

        public override List<PortDefinition> GetPortDefinitions() => new()
        {
            new PortDefinition("FlowIn", "flow", false),
            new PortDefinition("PosX", "float", false),
            new PortDefinition("PosY", "float", false),
            new PortDefinition("PosZ", "float", false),
            new PortDefinition("FlowOut", "flow", true),
            new PortDefinition("SpawnedObject", "gameobject", true),
        };
    }
}