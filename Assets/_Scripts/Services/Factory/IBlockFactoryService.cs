using _Scripts.Core;
using UnityEngine;

namespace _Scripts.Services.Factory
{
    public interface IBlockFactoryService
    {
        BaseBlock Create(string blockType, Transform parent);
    }
}