using System.IO;
using FluentMigrator.Runner;
using HomeworkApp.Bll.Extensions;
using HomeworkApp.Dal.Extensions;
using HomeworkApp.Dal.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HomeworkApp.IntegrationTests.Fixtures
{
    public class TestFixture
    {
        public IUserRepository UserRepository { get; }

        public ITaskRepository TaskRepository { get; }

        public ITaskLogRepository TaskLogRepository { get; }

        public ITakenTaskRepository TakenTaskRepository { get; }


        public IUserScheduleRepository UserScheduleRepository { get; }

        public TestFixture()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(
                    services =>
                    {
                        services
                            .AddBllServices()
                            .AddDalInfrastructure(config)
                            .AddDalRepositories();


                        services.AddStackExchangeRedisCache(options =>
                        {
                            options.Configuration = config["DalOptions:RedisConnectionString"];
                        });
                    })
                .Build();

            ClearDatabase(host);
            host.MigrateUp();

            var scope = host.Services.CreateScope();
            UserRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            TaskRepository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
            TaskLogRepository = scope.ServiceProvider.GetRequiredService<ITaskLogRepository>();
            TakenTaskRepository = scope.ServiceProvider.GetRequiredService<ITakenTaskRepository>();
            UserScheduleRepository = scope.ServiceProvider.GetRequiredService<IUserScheduleRepository>();

            FluentAssertionOptions.UseDefaultPrecision();
        }

        private static void ClearDatabase(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateDown(0);
        }
    }
}
