using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BlocksERP.Startup))]
namespace BlocksERP
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
