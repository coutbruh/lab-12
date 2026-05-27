using MVVM.Services;
using System.Windows.Input;

namespace MVVM.ViewModels
{
    public class MainWindowViewModel
    {
        private readonly INavigationService _navigation;

        public INavigationService NavigationService { get; }

        public ICommand ShowContactsCommand { get; }
        public ICommand ShowAboutCommand { get; }

        public MainWindowViewModel(INavigationService navigation)
        {
            _navigation = navigation;
            NavigationService = navigation;

            ShowContactsCommand = new RelayCommand(() => _navigation.NavigateTo<ContactsListViewModel>());
            ShowAboutCommand = new RelayCommand(() => _navigation.NavigateTo<AboutViewModel>());

        }
    }
}