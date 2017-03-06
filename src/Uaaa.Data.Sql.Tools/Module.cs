using System.IO;
using Autofac;
using Microsoft.Extensions.Configuration;

namespace Uaaa.Sql.Tools
{
    internal class Module : Autofac.Module
    {
        private const string UserSecretsId = "Uaaa.Data.Sql.Tools-47a0f8e2-6560-4a01-a378-f7643a99147f";
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<UpdateCommandDataProvider>().As<UpdateCommand.IDataProvider>();
            builder.RegisterType<CreateCommandDataProvider>().As<CreateCommand.IDataProvider>();
            builder.RegisterType<ConsoleOutput>().As<ITextOutput>().SingleInstance();
            builder.Register(context =>
            {
                string settingsFilenameWithPath = Path.Combine(Directory.GetCurrentDirectory(), "settings.json");
                string appSettingsFilenameWithPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
                var config = new ConfigurationBuilder();
                if (File.Exists(settingsFilenameWithPath))
                    config.AddJsonFile(settingsFilenameWithPath);
                if (File.Exists(appSettingsFilenameWithPath))
                    config.AddJsonFile(appSettingsFilenameWithPath);
                config.AddUserSecrets(UserSecretsId);
                return config.Build();
            }).SingleInstance();
        }
    }
}