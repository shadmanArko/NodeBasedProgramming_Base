using System;
using _Scripts.Blocks;
using _Scripts.Core;
using UnityEngine;

namespace _Scripts.Serialization
{
    public static class GraphSerializer
    {
        //todo Serialize
        
        //todo Deserialize
        
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
            LogBlock    b => JsonUtility.ToJson(new LogProps
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
                    b.defaultX = p.defaultX; b.defaultY = p.defaultY; b.defaultZ = p.defaultZ;
                    break;
                }
                case MoveObjectBlock b:
                {
                    var p = JsonUtility.FromJson<MoveObjectProps>(json);
                    Enum.TryParse(p.mode, out b.mode);
                    b.defaultX = p.defaultX; b.defaultY = p.defaultY; b.defaultZ = p.defaultZ;
                    break;
                }
                case RotateObjectBlock b:
                {
                    var p = JsonUtility.FromJson<RotateObjectProps>(json);
                    Enum.TryParse(p.mode, out b.mode);
                    b.defaultPitch = p.defaultPitch; b.defaultYaw = p.defaultYaw; b.defaultRoll = p.defaultRoll;
                    break;
                }
                case ScaleObjectBlock b:
                {
                    var p = JsonUtility.FromJson<ScaleObjectProps>(json);
                    Enum.TryParse(p.mode, out b.mode);
                    b.defaultX = p.defaultX; b.defaultY = p.defaultY; b.defaultZ = p.defaultZ;
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
                    b.defaultA = p.defaultA; b.defaultB = p.defaultB;
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
                    b.logFloatValue  = p.logFloatValue;
                    break;
                }
            }
        }
    }
}