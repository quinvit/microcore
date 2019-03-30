using System.Threading.Tasks.Dataflow;
using HS.Microcore.Configuration;
using HS.Microcore.Interfaces.Configuration;
using Ninject;
using Ninject.Syntax;

namespace HS.Microcore.Ninject
{
    public  class ConfigEventFactory : IConfigEventFactory
    {
        private readonly IResolutionRoot _resolutionRoot;

        public ConfigEventFactory(IResolutionRoot resolutionRoot)
        {
            _resolutionRoot = resolutionRoot;
        }

        public ISourceBlock<T> GetChangeEvent<T>() where T : IConfigObject
        {
            return _resolutionRoot.Get<ISourceBlock<T>>();
        }
    }
}