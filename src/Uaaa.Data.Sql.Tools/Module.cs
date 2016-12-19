using System.IO;
using Autofac;
using Microsoft.Extensions.Configuration;

namespace Uaaa.Sql.Tools
{
    internal class Module : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConsoleOutput>().As<ITextOutput>().SingleInstance();
            builder.Register(context =>
            {
                const string settingsFilename = "settings.json";
                string currentDirectory = Directory.GetCurrentDirectory();
                var config = new ConfigurationBuilder();
                if (File.Exists(Path.Combine(currentDirectory, settingsFilename)))
                    config.AddJsonFile(settingsFilename);
                config.AddUserSecrets();
                return config.Build();
            }).SingleInstance();
        }
    }
}