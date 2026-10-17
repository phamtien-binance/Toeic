using Abp.Events.Bus;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Toeic.Configuration;
using Toeic.EntityFrameworkCore;
using Toeic.Migrator.DependencyInjection;
using Castle.MicroKernel.Registration;
using Microsoft.Extensions.Configuration;

namespace Toeic.Migrator;

[DependsOn(typeof(ToeicEntityFrameworkModule))]
public class ToeicMigratorModule : AbpModule
{
    private readonly IConfigurationRoot _appConfiguration;

    public ToeicMigratorModule(ToeicEntityFrameworkModule abpProjectNameEntityFrameworkModule)
    {
        abpProjectNameEntityFrameworkModule.SkipDbSeed = true;

        _appConfiguration = AppConfigurations.Get(
            typeof(ToeicMigratorModule).GetAssembly().GetDirectoryPathOrNull()
        );
    }

    public override void PreInitialize()
    {
        Configuration.DefaultNameOrConnectionString = _appConfiguration.GetConnectionString(
            ToeicConsts.ConnectionStringName
        );

        Configuration.BackgroundJobs.IsJobExecutionEnabled = false;
        Configuration.ReplaceService(
            typeof(IEventBus),
            () => IocManager.IocContainer.Register(
                Component.For<IEventBus>().Instance(NullEventBus.Instance)
            )
        );
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(ToeicMigratorModule).GetAssembly());
        ServiceCollectionRegistrar.Register(IocManager);
    }
}
