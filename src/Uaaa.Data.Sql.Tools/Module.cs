using System.IO;
using Autofac;
using Microsoft.Extensions.Configuration;

namespace Uaaa.Sql.Tools
{
    internal class Module : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<UpdateCommandDataProvider>().As<UpdateCommand.IDataProvider>();
            builder.RegisterType<ConsoleOutput>().As<ITextOutput>().SingleInstance();
            builder.Register(context =>
            {
                string settingsFilenameWithPath = Path.Combine(Directory.GetCurrentDirectory(), "settings.json");
                var config = new ConfigurationBuilder();
                if (File.Exists(settingsFilenameWithPath))
                    config.AddJsonFile(settingsFilenameWithPath);
                config.AddUserSecrets();
                return config.Build();
            }).SingleInstance();
        }
    }
}