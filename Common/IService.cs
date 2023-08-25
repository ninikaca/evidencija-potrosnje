using System.IO;
using System.ServiceModel;

namespace Common
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        void LoadSaveDatabase(MemoryStream stream, string filename);
    }
}
