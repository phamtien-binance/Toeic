using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Toeic.Authorization;

namespace Toeic;

[DependsOn(
    typeof(ToeicCoreModule),
    typeof(AbpAutoMapperModule))]
public class ToeicApplicationModule : AbpModule
{
    public override void PreInitialize()
    {
        Configuration.Authorization.Providers.Add<ToeicAuthorizationProvider>();
    }

    public override void Initialize()
    {
        var thisAssembly = typeof(ToeicApplicationModule).GetAssembly();

        IocManager.RegisterAssemblyByConvention(thisAssembly);

        Configuration.Modules.AbpAutoMapper().Configurators.Add(
            // Scan the assembly for classes which inherit from AutoMapper.Profile
            cfg => cfg.AddMaps(thisAssembly)
        );
    }
}
