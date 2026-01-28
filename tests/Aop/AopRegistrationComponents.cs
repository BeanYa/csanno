using Csanno.Attributes;

namespace Csanno.Tests.Aop
{

    public interface IAopMetadataService
    {
        string GetValue();
    }

    [Component]
    [Singleton]
    [AsService(typeof(IAopMetadataService))]
    [WithMetadata("Name", "AopService")]
    public class AopMetadataService : IAopMetadataService
    {
        [Logging]
        public virtual string GetValue()
        {
            return "ok";
        }
    }
}
