using System;
using System.Collections.Generic;
using _Scripts.Blocks;
using _Scripts.Core;
using _Scripts.Models;
using _Scripts.Services.Factory;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Scripts.Serialization
{
    public static class GraphSerializer
    {
        public static GraphData Serialize(IGraphModel model)
        {
            var data = new GraphData
            {
                version = "2.0",
                exportedAt = DateTime.UtcNow.ToString("o")
            };

            foreach (var block in model.Blocks)
            {
                if (block == null) continue;
                data.blocks.Add(new BlockData
                {
                    blockId = block.blockId,
                    blockType = block.BlockType,
                    gameObjectName = block.name,
                    position = new SerializableVector3(block.transform.position),
                    propertiesJson = SerializeProps(block)
                });
            }

            foreach (var conn in model.Connections)
            {
                data.connections.Add(new ConnectionData
                {
                    fromBlockId = conn.fromBlockId,
                    fromPortName = conn.fromPortName,
                    toBlockId = conn.toBlockId,
                    toPortName = conn.toPortName
                });
            }

            foreach (var go in model.GetRuntimeObjects())
            {
                if (go == null) continue;
                string primType = "Cube";
                var mf = go.GetComponent<MeshFilter>();
                if (mf?.sharedMesh != null)
                    primType = mf.sharedMesh.name.Replace(" Instance", "");

                data.runtimeObjects.Add(new RuntimeObjectData
                {
                    name = go.name,
                    primitiveType = primType,
                    position = new SerializableVector3(go.transform.position),
                    rotation = new SerializableVector3(go.transform.eulerAngles),
                    scale = new SerializableVector3(go.transform.localScale)
                });
            }

            return data;
        }


        public static async UniTask DeserializeAsync(GraphData data, IGraphModel model)
        {
            model.ClearRuntimeObjects();
            var toDestroy = new List<BaseBlock>(model.Blocks);

            foreach (var b in toDestroy)
            {
                if (b != null) UnityEngine.Object.Destroy(b.gameObject);
            }

            while (model.Blocks.Count > 0)
                model.RemoveBlock(model.Blocks[0].blockId);
            while (model.Connections.Count > 0)
                model.RemoveConnectionAt(0);

            await UniTask.Yield();

            var sceneRoot = UnityEngine.Object.FindObjectOfType<Views.GraphView>();
            var parent = sceneRoot != null ? sceneRoot.BlockParent : null;

            var factory = new BlockFactoryService();

            foreach (var bd in data.blocks)
            {
                var block = factory.Create(bd.blockType, parent);
                if (block == null) continue;

                block.blockId = bd.blockId;
                block.name = bd.gameObjectName;
                block.transform.position = bd.position.ToVector3();
                ApplyProps(block, bd.propertiesJson);
                model.AddBlock(block);
            }

            foreach (var cd in data.connections)
                model.AddConnection(new BlockConnection(
                    cd.fromBlockId, cd.fromPortName, cd.toBlockId, cd.toPortName));

            foreach (var rod in data.runtimeObjects)
            {
                Enum.TryParse<PrimitiveType>(rod.primitiveType, out var prim);
                var go = GameObject.CreatePrimitive(prim);
                go.name = rod.name;
                go.transform.position = rod.position.ToVector3();
                go.transform.eulerAngles = rod.rotation.ToVector3();
                go.transform.localScale = rod.scale.ToVector3();
                model.RegisterRuntimeObject(go);
            }
        }

        private static string SerializeProps(BaseBlock block) => block switch
        {
            SpawnObjectBlock b => JsonUtility.ToJson(new SpawnObjectProps
            {
                primitiveType = b.primitiveType.ToString(), objectName = b.objectName,
                defaultX = b.defaultX, defaultY = b.defaultY, defaultZ = b.defaultZ
            }),
            MoveObjectBlock b => JsonUtility.ToJson(new MoveObjectProps
            {
                mode = b.mode.ToString(),
                defaultX = b.defaultX, defaultY = b.defaultY, defaultZ = b.defaultZ
            }),
            RotateObjectBlock b => JsonUtility.ToJson(new RotateObjectProps
            {
                mode = b.mode.ToString(),
                defaultPitch = b.defaultPitch, defaultYaw = b.defaultYaw, defaultRoll = b.defaultRoll
            }),
            ScaleObjectBlock b => JsonUtility.ToJson(new ScaleObjectProps
            {
                mode = b.mode.ToString(),
                defaultX = b.defaultX, defaultY = b.defaultY, defaultZ = b.defaultZ
            }),
            VariableBlock b => JsonUtility.ToJson(new VariableProps
            {
                initialValue = b.initialValue, operation = b.operation.ToString(), operand = b.operand
            }),
            CompareBlock b => JsonUtility.ToJson(new CompareProps
            {
                defaultA = b.defaultA, defaultB = b.defaultB, compareOp = b.compareOp.ToString()
            }),
            BranchBlock b => JsonUtility.ToJson(new BranchProps { defaultCondition = b.defaultCondition }),
            LogBlock b => JsonUtility.ToJson(new LogProps
            {
                defaultMessage = b.defaultMessage, logFloatValue = b.logFloatValue
            }),
            _ => "{}"
        };

        private static void ApplyProps(BaseBlock block, string json)
        {
            if (string.IsNullOrEmpty(json) || json == "{}") return;
            switch (block)
            {
                case SpawnObjectBlock b:
                {
                    var p = JsonUtility.FromJson<SpawnObjectProps>(json);
                    Enum.TryParse(p.primitiveType, out b.primitiveType);
                    b.objectName = p.objectName;
                    b.defaultX = p.defaultX;
                    b.defaultY = p.defaultY;
                    b.defaultZ = p.defaultZ;
                    break;
                }
                case MoveObjectBlock b:
                {
                    var p = JsonUtility.FromJson<MoveObjectProps>(json);
                    Enum.TryParse(p.mode, out b.mode);
                    b.defaultX = p.defaultX;
                    b.defaultY = p.defaultY;
                    b.defaultZ = p.defaultZ;
                    break;
                }
                case RotateObjectBlock b:
                {
                    var p = JsonUtility.FromJson<RotateObjectProps>(json);
                    Enum.TryParse(p.mode, out b.mode);
                    b.defaultPitch = p.defaultPitch;
                    b.defaultYaw = p.defaultYaw;
                    b.defaultRoll = p.defaultRoll;
                    break;
                }
                case ScaleObjectBlock b:
                {
                    var p = JsonUtility.FromJson<ScaleObjectProps>(json);
                    Enum.TryParse(p.mode, out b.mode);
                    b.defaultX = p.defaultX;
                    b.defaultY = p.defaultY;
                    b.defaultZ = p.defaultZ;
                    break;
                }
                case VariableBlock b:
                {
                    var p = JsonUtility.FromJson<VariableProps>(json);
                    b.initialValue = p.initialValue;
                    Enum.TryParse(p.operation, out b.operation);
                    b.operand = p.operand;
                    break;
                }
                case CompareBlock b:
                {
                    var p = JsonUtility.FromJson<CompareProps>(json);
                    b.defaultA = p.defaultA;
                    b.defaultB = p.defaultB;
                    Enum.TryParse(p.compareOp, out b.compareOp);
                    break;
                }
                case BranchBlock b:
                {
                    var p = JsonUtility.FromJson<BranchProps>(json);
                    b.defaultCondition = p.defaultCondition;
                    break;
                }
                case LogBlock b:
                {
                    var p = JsonUtility.FromJson<LogProps>(json);
                    b.defaultMessage = p.defaultMessage;
                    b.logFloatValue = p.logFloatValue;
                    break;
                }
            }
        }
    }
}