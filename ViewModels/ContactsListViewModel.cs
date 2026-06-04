using MVVM.Models;
using MVVM.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;

namespace MVVM.ViewModels
{
    public class ContactsListViewModel : ObservableObject
    {
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigation;
        private readonly IDbContextFactory<PhoneBookDbKonuh2307b2Context> _contextFactory;  // ← только одно поле

        public ObservableCollection<Contact> Contacts { get; set; }

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        private string _phone = string.Empty;
        public string Phone
        {
            get => _phone;
            set => Set(ref _phone, value);
        }

        private Contact? _selectedContact;
        public Contact? SelectedContact
        {
            get => _selectedContact;
            set => Set(ref _selectedContact, value);
        }

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand EditContactCommand { get; }

        public ContactsListViewModel(
            IDialogService dialogService,
            INavigationService navigation,
            IDbContextFactory<PhoneBookDbKonuh2307b2Context> contextFactory)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));  // ← инициализация

            Contacts = new ObservableCollection<Contact>();

            AddCommand = new RelayCommand(AddContact, CanAddContact);
            DeleteCommand = new RelayCommand<Contact>(DeleteContact, CanDeleteContact);
            EditContactCommand = new RelayCommand(EditContact, () => SelectedContact != null);

            LoadContacts();
        }

        private void LoadContacts()
        {
            try
            {
                using (var dbContext = _contextFactory.CreateDbContext())
                {
                    var contactsFromDb = dbContext.Contacts.ToList();
                    Contacts.Clear();
                    foreach (var contact in contactsFromDb)
                    {
                        Contacts.Add(contact);
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка загрузки данных: {ex.Message}", "Ошибка БД");
            }
        }

        private void AddContact()
        {
            try
            {
                using (var dbContext = _contextFactory.CreateDbContext())
                {
                    // Проверка на дубликат
                    if (dbContext.Contacts.Any(c => c.Phone == Phone))
                    {
                        _dialogService.ShowWarning("Контакт с таким номером телефона уже существует!", "Дубликат");
                        return;
                    }

                    var newContact = new Contact
                    {
                        Name = Name,
                        Phone = Phone
                    };

                    dbContext.Contacts.Add(newContact);
                    dbContext.SaveChanges();
                    Contacts.Add(newContact);
                }

                Name = string.Empty;
                Phone = string.Empty;
                _dialogService.ShowInfo("Контакт успешно добавлен!", "Успех");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка при добавлении: {ex.Message}", "Ошибка");
            }
        }

        private bool CanAddContact()
        {
            return !string.IsNullOrWhiteSpace(Name) && IsPhoneValid(Phone);
        }

        private void DeleteContact(object? parameter)
        {
            Contact? contact = parameter as Contact;
            if (contact == null) return;

            if (!_dialogService.ShowConfirmation($"Вы уверены, что хотите удалить контакт \"{contact.Name}\"?", "Подтверждение"))
            {
                return;
            }

            try
            {
                using (var dbContext = _contextFactory.CreateDbContext())
                {
                    var contactToDelete = dbContext.Contacts.Find(contact.Id);
                    if (contactToDelete != null)
                    {
                        dbContext.Contacts.Remove(contactToDelete);
                        dbContext.SaveChanges();
                    }
                }

                Contacts.Remove(contact);
                _dialogService.ShowInfo("Контакт успешно удалён!", "Успех");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка при удалении: {ex.Message}", "Ошибка");
            }
        }

        private bool CanDeleteContact(object? parameter)
        {
            Contact? contact = parameter as Contact;
            return contact != null && Contacts.Contains(contact);
        }

        private void EditContact()
        {
            if (SelectedContact != null)
            {
                _navigation.NavigateTo<ContactEditViewModel>(SelectedContact);
            }
        }

        private bool IsPhoneValid(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;
            string phonePattern = @"^(\+7\d{10}|\d{10})$";
            return Regex.IsMatch(phone, phonePattern);
        }
    }
}