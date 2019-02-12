using System.Data.Entity;

namespace Nabla.Mis
{
    public interface IManager
    {
        ManagerContext Context { get; }
        
    }
}
