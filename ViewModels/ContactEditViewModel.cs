using MVVM.Models;
using MVVM.Services;
using System.Windows.Input;

namespace MVVM.ViewModels
{
    public class ContactEditViewModel : ObservableObject, INavigationAware
    {
        private readonly INavigationService _navigation;
        private readonly PhoneBookDbKonuh2307b2Context _context;
        private readonly IDialogService _dialogService;
        private Contact _contact = null!;
        private bool _isNewContact = false;

        public string EditName
        {
            get => _contact.Name;
            set { _contact.Name = value; OnPropertyChanged(); }
        }

        public string EditPhone
        {
            get => _contact.Phone;
            set { _contact.Phone = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public ContactEditViewModel(
            INavigationService navigation,
            PhoneBookDbKonuh2307b2Context context,
            IDialogService dialogService)
        {
            _navigation = navigation;
            _context = context;
            _dialogService = dialogService;
            SaveCommand = new RelayCommand(SaveContact);
            CancelCommand = new RelayCommand(() => _navigation.NavigateTo<ContactsListViewModel>());
        }

        public void OnNavigatedTo(object? parameter)
        {
            if (parameter is Contact c)
            {
                _contact = new Contact
                {
                    Id = c.Id,
                    Name = c.Name,
                    Phone = c.Phone
                };
                _isNewContact = false;
            }
            else
            {
                _contact = new Contact { Name = "", Phone = "" };
                _isNewContact = true;
            }
            OnPropertyChanged(nameof(EditName));
            OnPropertyChanged(nameof(EditPhone));
        }

        private void SaveContact()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(EditName))
                {
                    _dialogService.ShowWarning("Введите имя контакта", "Ошибка");
                    return;
                }

                if (_isNewContact)
                {
                    var newContact = new Contact
                    {
                        Name = EditName,
                        Phone = EditPhone
                    };
                    _context.Contacts.Add(newContact);
                }
                else
                {
                    var existingContact = _context.Contacts.Find(_contact.Id);
                    if (existingContact != null)
                    {
                        existingContact.Name = EditName;
                        existingContact.Phone = EditPhone;
                        _context.Entry(existingContact).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    }
                }

                _context.SaveChanges();
                _dialogService.ShowInfo("Контакт сохранён!", "Успех");
                _navigation.NavigateTo<ContactsListViewModel>();
            }
            catch (System.Exception ex)
            {
                _dialogService.ShowError($"Ошибка при сохранении: {ex.Message}", "Ошибка");
            }
        }
    }
}