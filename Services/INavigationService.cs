using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVVM.Services
{
    public interface INavigationService
    {
        object? CurrentViewModel { get; }
        void NavigateTo<TViewModel>(object? parameter = null)
        where TViewModel : class;
    }
    public interface INavigationAware
    {
        void OnNavigatedTo(object? parameter);
    }
}