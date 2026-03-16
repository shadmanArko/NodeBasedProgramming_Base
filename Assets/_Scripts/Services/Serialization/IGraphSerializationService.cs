using Cysharp.Threading.Tasks;

namespace _Scripts.Services.Serialization
{
    public interface IGraphSerializationService
    {
        UniTask ExportAsync(string path);
        UniTask ImportAsync(string path);
    }
}