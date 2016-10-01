using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(esThings.Startup))]
namespace esThings
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
