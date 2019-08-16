using System;

namespace Hyperscale.Microcore.Interfaces
{
    public interface IMetricsInitializer: IDisposable
    {
        void Init();
    }
}
