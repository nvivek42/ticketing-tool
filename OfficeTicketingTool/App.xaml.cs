using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using OfficeTicketingTool.Data;
using OfficeTicketingTool.Data.OfficeTicketingTool.Data;
using OfficeTicketingTool.Services;
using OfficeTicketingTool.Utilities;
using OfficeTicketingTool.ViewModels;
using OfficeTicketingTool.Views;
using System.IO;
using System.Windows;

namespace OfficeTicketingTool
{
    public partial class App : Application
    {
        private IServiceProvider? _serviceProvider;
        private IConfiguration? _configuration;
        public static IServiceProvider? ServiceProvider { get; private set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Build configuration
                _configuration = BuildConfiguration();

                var services = new ServiceCollection();
                ConfigureServices(services);
                _serviceProvider = services.BuildServiceProvider();
                ServiceProvider = _serviceProvider;

                // Test connection before initializing
                if (await TestDatabaseConnectionAsync())
                {
                    InitializeDatabase();
                    ShowLoginWindow();
                }
                else
                {
                    throw new Exception("Unable to establish database connection.");
                }
            }
            catch (Exception ex)
            {
                HandleStartupError(ex);
            }
        }

        private IConfiguration BuildConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).AddEnvironmentVariables(); // This allows overriding with environment variables

            return builder.Build();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            if (_configuration == null)
                throw new InvalidOperationException("Configuration is not initialized.");

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            // Database context configuration with improved settings
            services.AddDbContext<TicketingDbContext>(options =>
                options.UseMySql(
                    connectionString,
                    new MySqlServerVersion(new Version(8, 0, 42)),
                    mysqlOptions => {
                        mysqlOptions.EnableRetryOnFailure(
                            maxRetryCount: _configuration.GetValue<int>("DatabaseSettings:MaxRetryCount", 3),
 
                            maxRetryDelay: _configuration.GetValue<TimeSpan>("DatabaseSettings:MaxRetryDelay", TimeSpan.FromSeconds(5)),
                            errorNumbersToAdd: null);
                        mysqlOptions.CommandTimeout(_configuration.GetValue<int>("DatabaseSettings:CommandTimeout", 30));
                    }));

            // Register services
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITicketService, TicketService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICategoryService, CategoryService>();

            // Register view models
            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<TicketViewModel>();
            services.AddTransient<UserViewModel>();

            // Register views
            services.AddTransient<LoginView>();
            services.AddSingleton<MainWindow>();
        }

        private async Task<bool> TestDatabaseConnectionAsync()
        {
            try
            {
                var connectionString = _configuration?.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                    return false;

                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                // Test if database exists
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'TicketingToolDb'";
                var result = await command.ExecuteScalarAsync();

                if (result == null)
                {
                    // Database doesn't exist, create it
                    command.CommandText = "CREATE DATABASE IF NOT EXISTS TicketingToolDb";
                    await command.ExecuteNonQueryAsync();
                    Console.WriteLine("Database 'TicketingToolDb' created successfully.");
                }

                Console.WriteLine("Database connection test successful!");
                return true;
            }
            catch (MySqlException mysqlEx)
            {
                Console.WriteLine($"MySQL Error {mysqlEx.Number}: {mysqlEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection test failed: {ex.Message}");
                return false;
            }
        }

        private void InitializeDatabase()
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException("Service provider is not initialized.");

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TicketingDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            try
            {
                Console.WriteLine("Applying database migrations...");
                dbContext.Database.Migrate();
                Console.WriteLine("Migrations applied successfully.");

                Console.WriteLine("Seeding initial data...");
                DbInitializer.Initialize(dbContext, passwordHasher);
                Console.WriteLine("Database initialization completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing database: {ex.Message}");
                throw;
            }
        }

        private void HandleStartupError(Exception ex)
        {
            string errorDetails = $"Failed to start application: {ex.Message}\n\n";

            if (ex.InnerException != null)
            {
                errorDetails += $"Inner Exception: {ex.InnerException.Message}\n\n";
            }

            // Specific MySQL error handling
            MySqlException? mysqlEx = ex as MySqlException;
            MySqlException? innerMysqlEx = ex.InnerException as MySqlException;
            var mysqlError = mysqlEx ?? innerMysqlEx;

            if (mysqlError != null)
            {
                errorDetails += GetMySqlErrorMessage(mysqlError.Number) + "\n\n";
            }

            errorDetails += "Troubleshooting Steps:\n" +
                          "1. Verify MySQL Server is running (check services)\n" +
                          "2. Test connection with MySQL Workbench or command line\n" +
                          "3. Verify username/password in appsettings.json\n" +
                          "4. Check if port 3306 is available\n" +
                          "5. Ensure MySQL user has CREATE/ALTER privileges\n" +
                          "6. Try adding 'SslMode=none' to connection string if using local MySQL";

            MessageBox.Show(errorDetails, "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Console.WriteLine(errorDetails);
            Current.Shutdown(1);
        }

        private string GetMySqlErrorMessage(int errorNumber)
        {
            return errorNumber switch
            {
                1045 => "MySQL Error: Access denied - Check username and password",
                2003 => "MySQL Error: Cannot connect to server - Verify MySQL is running and port is correct",
                1049 => "MySQL Error: Database does not exist - Will attempt to create it",
                1251 => "MySQL Error: Authentication plugin issue - Try using mysql_native_password",
                2013 => "MySQL Error: Lost connection - Check network/firewall settings",
                _ => $"MySQL Error {errorNumber}: Please check MySQL documentation"
            };
        }

        private void ShowLoginWindow()
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException("Service provider is not initialized.");

            var loginView = _serviceProvider.GetRequiredService<LoginView>();
            var loginWindow = new Window
            {
                Content = loginView,
                Title = "Office Ticketing Tool - Login",
                Width = 400,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            loginWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_serviceProvider is IDisposable disposableServiceProvider)
            {
                disposableServiceProvider.Dispose();
            }
            base.OnExit(e);
        }
    }
}
