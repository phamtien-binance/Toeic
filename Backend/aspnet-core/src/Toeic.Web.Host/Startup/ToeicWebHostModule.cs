using Abp.Modules;
using Abp.Reflection.Extensions;
using Toeic.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Toeic.Web.Host.Startup
{
    [DependsOn(
       typeof(ToeicWebCoreModule))]
    public class ToeicWebHostModule : AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public ToeicWebHostModule(IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(ToeicWebHostModule).GetAssembly());
        }
    }
}
