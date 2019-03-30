using HS.Microcore.Interfaces.Events;
using HS.Microcore.Interfaces.Logging;
using HS.Microcore.Ninject;
using Ninject.Activation;
using Ninject.Modules;
using Ninject.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace HS.Microcore.Logging.Serilog
{
    public class SerilogModule : NinjectModule, ILoggingModule
    {
        /// <summary>
        /// Used by clients to initialize logging and tracing.
        /// </summary>
        public override void Load()
        {
            Bind(Bind<ILog>(), Bind<IEventPublisher>());
        }

        /// <summary>
        /// Used by Microcore hosts to initialize logging and tracing.
        /// </summary>
        /// <param name="logBinding"></param>
        /// <param name="eventPublisherBinding"></param>
        public void Bind(IBindingToSyntax<ILog> logBinding, IBindingToSyntax<IEventPublisher> eventPublisherBinding)
        {
            logBinding
                .To<SerilogLogger>()
                .InScope(GetTypeOfTarget)
                .WithConstructorArgument("receivingType", (context, target) => GetTypeOfTarget(context));

            eventPublisherBinding
                .To<LogEventPublisher>()
                .InSingletonScope();
        }

        /// <summary>
        /// Returns the type that requested the log, or the type <see cref="NLogModule"/> if the requester can't be determined.
        /// </summary>
        /// <param name="context">The Ninject context of the request.</param>
        /// <returns></returns>
        private static Type GetTypeOfTarget(IContext context)
        {
            var type = context.Request.Target?.Member.DeclaringType;
            return type ?? typeof(SerilogModule);
        }
    }
}
