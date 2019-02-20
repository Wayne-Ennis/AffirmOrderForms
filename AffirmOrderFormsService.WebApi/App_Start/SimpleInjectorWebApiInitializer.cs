[assembly: WebActivator.PostApplicationStartMethod(typeof(AffirmOrderFormsService.App_Start.SimpleInjectorWebApiInitializer), "Initialize")]

namespace AffirmOrderFormsService.App_Start
{
    using System.Web.Http;
    using ADM.Kit;
    using AffirmOrderFormsService.ServiceLayer;
    using Ipipeline.Logging;
    using SimpleInjector;
    using SimpleInjector.Integration.WebApi;
    using SimpleInjector.Lifestyles;

    public static class SimpleInjectorWebApiInitializer
    {
        /// <summary>Initialize the container and register it as Web API Dependency Resolver.</summary>
        public static void Initialize()
        {
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            InitializeContainer(container);

            container.RegisterWebApiControllers(GlobalConfiguration.Configuration);

           container.Verify();

            GlobalConfiguration.Configuration.DependencyResolver =
                new SimpleInjectorWebApiDependencyResolver(container);
        }

        private static void InitializeContainer(Container container)
        {
            // For instance:
            // container.Register<IUserRepository, SqlUserRepository>(Lifestyle.Scoped);
           // container.Register<IADMServerKit>(() => new ADMServerKit("AnnuityOrderFormServices", true), Lifestyle.Scoped);
            container.Register<LogManager>(Lifestyle.Singleton);
            container.Register<IOrderFormsService, OrderFormsService>(Lifestyle.Scoped);



        }
    }
}