using System;
using System.Collections.Generic;
using _Scripts.Blocks;
using _Scripts.Core;
using UnityEngine;

namespace _Scripts.Services.Factory
{
    public sealed class BlockFactoryService : IBlockFactoryService
    {
        private readonly Dictionary<string, Func<GameObject, BaseBlock>> _registry;

        public BlockFactoryService()
        {
            _registry = new Dictionary<string, Func<GameObject, BaseBlock>>(
                StringComparer.OrdinalIgnoreCase)
            {
                ["SpawnObject"]  = go => go.AddComponent<SpawnObjectBlock>(),
                ["MoveObject"]   = go => go.AddComponent<MoveObjectBlock>(),
                ["RotateObject"] = go => go.AddComponent<RotateObjectBlock>(),
                ["ScaleObject"]  = go => go.AddComponent<ScaleObjectBlock>(),
                ["Variable"]     = go => go.AddComponent<VariableBlock>(),
                ["Compare"]      = go => go.AddComponent<CompareBlock>(),
                ["Branch"]       = go => go.AddComponent<BranchBlock>(),
                ["Log"]          = go => go.AddComponent<LogBlock>(),
            };
        }

        public BaseBlock Create(string blockType, Transform parent)
        {
            if (!_registry.TryGetValue(blockType, out var factory))
            {
                Debug.LogError($"[BlockFactory] Unknown block type: '{blockType}'");
                return null;
            }

            var go    = new GameObject(blockType);
            go.transform.SetParent(parent, false);

            var block = factory(go);
            block.EnsureId();

            return block;
        }
        
        public void Register(string typeKey, Func<GameObject, BaseBlock> factory)
            => _registry[typeKey] = factory;
    }
}