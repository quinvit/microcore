using System;
using Hyperscale.Microcore.Logging.Serilog;
using Hyperscale.Microcore.Ninject;
using Hyperscale.Microcore.Ninject.Host;
using Hyperscale.Microcore.SharedLogic;
using Ninject;
using AuthService.Interface;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using AuthService.Interface.Models;
using AuthService.Entities;

namespace AuthService
{

    class AuthServiceHost : MicrocoreServiceHost<IAuthService>
    {
        private readonly IConfiguration _configuration;

        public AuthServiceHost(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override ILoggingModule GetLoggingModule() => new SerilogModule();

        protected override void Configure(IKernel kernel, BaseCommonConfig commonConfig)
        {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<User, UserEntity>()
                    .ForMember(destination => destination.SocialProfiles, opts => opts.MapFrom(source => string.Join(" | ", source.SocialProfiles)));
                cfg.CreateMap<UserEntity, User>()
                    .ForMember(destination => destination.SocialProfiles, opts => opts.MapFrom(source => source.SocialProfiles.Split(" | ", StringSplitOptions.RemoveEmptyEntries)));
            });

            kernel.Bind<IMapper>().ToMethod(x => config.CreateMapper()).InSingletonScope();

            kernel.Bind<IConfiguration>().ToConstant(_configuration);
            kernel.Bind<IAuthService>().To<AuthService>();
        }
    }
}
