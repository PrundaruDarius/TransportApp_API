using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Data.Common;
using TransportApp_API.Data;

namespace TransportApp_API.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(DbConnection));

            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            services.AddSingleton<DbConnection>(connection);

            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                var conn = sp.GetRequiredService<DbConnection>();
                options.UseSqlite(conn);
            });
        });
    }
}