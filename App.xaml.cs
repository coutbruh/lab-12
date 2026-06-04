using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MVVM.Models;
using MVVM.Services;
using MVVM.ViewModels;
using MVVM.Views;
using System.Windows;

namespace MVVM
{
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var services = new ServiceCollection();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddTransient<ContactsListViewModel>();
            services.AddTransient<AboutViewModel>();
            services.AddTransient<ContactEditViewModel>();
            services.AddSingleton<MainWindowViewModel>();
            //изменяем на фабрику
            services.AddDbContextFactory<PhoneBookDbKonuh2307b2Context>(options =>
                options.UseSqlServer("Data Source=DESKTOP-JOHDNJH;Initial Catalog=PhoneBookDB_Konuh_2307B2;Integrated Security=True;TrustServerCertificate=True"));
          
            services.AddSingleton<MainWindow>(provider =>
            {
                var window = new MainWindow();
                window.DataContext = provider.GetRequiredService<MainWindowViewModel>();
                return window;
            });

            // Построение провайдера
            _serviceProvider = services.BuildServiceProvider();

            // Получение и отображение главного окна
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

    

    protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}