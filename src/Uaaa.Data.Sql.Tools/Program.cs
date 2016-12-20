using System;
using System.IO;
using System.Reflection;
using Autofac;
using Autofac.Features.ResolveAnything;
using Microsoft.Extensions.CommandLineUtils;

namespace Uaaa.Sql.Tools
{
    public class Program
    {
        public class Information
        {
            public readonly string ShortVersion;
            public readonly string Directory;

            public Information()
            {

                Func<TypeInfo, string> getVersionFromTypeInfo = info =>
                {
                    string infoVersion = info.Assembly
                                        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                        .InformationalVersion;
                    if (string.IsNullOrEmpty(infoVersion))
                        infoVersion = info.Assembly.GetName().Version.ToString();
                    return infoVersion;
                };

                TypeInfo typeInfo = typeof(Information).GetTypeInfo();

                ShortVersion = getVersionFromTypeInfo(typeInfo);
                Directory = Path.GetDirectoryName(typeInfo.Assembly.Location);
            }
        }
        public static readonly Information Info = new Information();
        public static IContainer Container { get; private set; }
        private static CommandLineApplication application;
        public static void Main(string[] args)
        {
            try
            {
                Container = BuildContainer();

                application = new CommandLineApplication(false)
                {
                    Name = "uaaa-sql",
                    Description = "SQL Script runner tool.",
                    FullName = "Uaaa SQL Script runner tool."
                };

                application.HelpOption("-?|-h|--help");
                application.VersionOption("-v|--version", $"{Info.ShortVersion}");

                application.Command("update", config =>
                {
                    config.Description = "Updates database to last version by executing new scripts in specified directory.";
                    config.HelpOption("-?|-h|--help");

                    CommandOption pathOption = config.Option(
                        "-p|--path",
                        "Path to scripts directory. If not specified, 'Scripts' folder searched in current directory.",
                        CommandOptionType.SingleValue
                    );

                    CommandOption connectionOption = config.Option(
                        "-c|--connection <connection>",
                        "Specifies setting key where database connection string is stored.",
                        CommandOptionType.SingleValue
                    );

                    config.OnExecute(async () =>
                    {

                        application.ShowRootCommandFullNameAndVersion();
                        using (ILifetimeScope scope = Container.BeginLifetimeScope())
                        {
                            var command = scope.Resolve<UpdateCommand>();
                            command.ConnectionKey = connectionOption.Value();
                            if (pathOption.HasValue())
                                command.ScriptsPath = pathOption.Value();
                            return await command.Execute();
                        }
                    });
                }, false);

                application.OnExecute(() =>
                {
                    application.ShowHelp();
                    return 0;
                });

                application.Execute(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            builder.RegisterModule<Module>();
            return builder.Build();
        }
    }
}
