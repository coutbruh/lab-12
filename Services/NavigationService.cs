using Microsoft.Extensions.DependencyInjection;
using MVVM.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVVM.Services
{
    public class NavigationService : ObservableObject, INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private object? _currentViewModel;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object? CurrentViewModel
        {
            get => _currentViewModel;
            private set => Set(ref _currentViewModel, value);
        }

        public void NavigateTo<TViewModel>(object? parameter = null) where TViewModel : class
        {
            var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
            
            if (viewModel is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedTo(parameter);
            }

            CurrentViewModel = viewModel;
        }
    }
}
