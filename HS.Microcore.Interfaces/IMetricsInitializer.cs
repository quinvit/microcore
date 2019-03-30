using System;

namespace HS.Microcore.Interfaces
{
    public interface IMetricsInitializer: IDisposable
    {
        void Init();
    }
}
