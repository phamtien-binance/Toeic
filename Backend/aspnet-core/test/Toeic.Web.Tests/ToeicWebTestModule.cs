using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Toeic.EntityFrameworkCore;
using Toeic.Web.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace Toeic.Web.Tests;

[DependsOn(
    typeof(ToeicWebMvcModule),
    typeof(AbpAspNetCoreTestBaseModule)
)]
public class ToeicWebTestModule : AbpModule
{
    public ToeicWebTestModule(ToeicEntityFrameworkModule abpProjectNameEntityFrameworkModule)
    {
        abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
    }

    public override void PreInitialize()
    {
        Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(ToeicWebTestModule).GetAssembly());
    }

    public override void PostInitialize()
    {
        IocManager.Resolve<ApplicationPartManager>()
            .AddApplicationPartsIfNotAddedBefore(typeof(ToeicWebMvcModule).Assembly);
    }
}