using System;
using Hyperscale.Microcore.Logging.Serilog;
using Hyperscale.Microcore.Ninject;
using Hyperscale.Microcore.Ninject.Host;
using Hyperscale.Microcore.SharedLogic;
using Ninject;
using AuthService.Interface;

namespace AuthService
{

    class AuthServiceHost : MicrocoreServiceHost<IAuthService>
    {
        protected override ILoggingModule GetLoggingModule() => new SerilogModule();

        protected override void Configure(IKernel kernel, BaseCommonConfig commonConfig)
        {
            kernel.Bind<IAuthService>().To<AuthService>();
        }
    }
}
