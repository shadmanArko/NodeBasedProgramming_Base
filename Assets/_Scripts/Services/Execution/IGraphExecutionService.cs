using Cysharp.Threading.Tasks;

namespace _Scripts.Services.Execution
{
    public interface IGraphExecutionService
    {
        UniTask ExecuteAsync();
    }
}