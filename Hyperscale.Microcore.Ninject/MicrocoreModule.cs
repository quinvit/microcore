#region Copyright 
// Copyright 2017 Gygya Inc.  All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License.  
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDER AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
#endregion

using System;
using System.Collections.Concurrent;
using System.Linq;
using Hyperscale.Common.Contracts.HttpService;
using Hyperscale.Microcore.Configuration;
using Hyperscale.Microcore.Hosting.HttpService;
using Hyperscale.Microcore.ServiceDiscovery;
using Hyperscale.Microcore.ServiceDiscovery.HostManagement;
using Hyperscale.Microcore.ServiceDiscovery.LoadBalancer;
using Hyperscale.Microcore.ServiceProxy;
using Hyperscale.Microcore.SharedLogic;
using Hyperscale.Microcore.SharedLogic.Monitor;
using Ninject;
using Ninject.Activation;
using Ninject.Extensions.Factory;
using Ninject.Modules;

namespace Hyperscale.Microcore.Ninject
{
    /// <inheritdoc />
    /// <summary>
    /// Contains all binding except hosting layer
    /// </summary>
    public class MicrocoreModule : NinjectModule
    {
    
        private readonly Type[] NonSingletonBaseTypes =
        {
            typeof(RemoteHostPool),
            typeof(LoadBalancer),
            typeof(ConfigDiscoverySource)
        };

        public override void Load()
        {
            //Need to be initialized before using any regex!
            new RegexTimeoutInitializer().Init();

            Kernel
                .Bind(typeof(ConcurrentDictionary<,>))
                .To(typeof(DisposableConcurrentDictionary<,>))
                .InSingletonScope();

            if (Kernel.CanResolve<Func<long, DateTime>>() == false)
                Kernel.Load<FuncModule>();

            this.BindClassesAsSingleton(NonSingletonBaseTypes, typeof(ConfigurationAssembly), typeof(ServiceProxyAssembly));
            this.BindInterfacesAsSingleton(NonSingletonBaseTypes, typeof(ConfigurationAssembly), typeof(ServiceProxyAssembly), typeof(SharedLogicAssembly), typeof(ServiceDiscoveryAssembly));
            
            Bind<IRemoteHostPoolFactory>().ToFactory();

            Kernel.BindPerKey<string, ReachabilityChecker, IServiceDiscovery, ServiceDiscovery.ServiceDiscovery>();
            Kernel.BindPerString<IServiceProxyProvider, ServiceProxyProvider>();
            Kernel.BindPerString<AggregatingHealthStatus>();

            Bind<IServiceDiscoverySource>().To<LocalDiscoverySource>().InTransientScope();
            Bind<IServiceDiscoverySource>().To<ConfigDiscoverySource>().InTransientScope();

            Bind<ILoadBalancer>().To<LoadBalancer>().InTransientScope();
            Bind<IDiscovery>().To<Discovery>().InSingletonScope();
            
            Kernel.Load<ServiceProxyModule>();
            Kernel.Load<ConfigObjectsModule>();

            // ServiceSchema is at ServiceContracts, and cannot be depended on IServiceInterfaceMapper, which belongs to Microcore
            Kernel.Rebind<ServiceSchema>()
                .ToMethod(c =>new ServiceSchema(c.Kernel.Get<IServiceInterfaceMapper>().ServiceInterfaceTypes.ToArray())).InSingletonScope();
        }


        protected static Type GetTypeOfTarget(IContext context)
        {
            var type = context.Request.Target?.Member.DeclaringType;
            return type ?? typeof(MicrocoreModule);
        }
    }
}
