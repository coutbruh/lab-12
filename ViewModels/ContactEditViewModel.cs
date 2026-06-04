using MVVM.Models;
using MVVM.Services;
using System;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;

namespace MVVM.ViewModels
{
    public class ContactEditViewModel : ObservableObject, INavigationAware
    {
        private readonly INavigationService _navigation;
        private readonly IDialogService _dialogService;
        private readonly IDbContextFactory<PhoneBookDbKonuh2307b2Context> _contextFactory;  // Фaбрика

        private int _contactId;
        private string _editName = string.Empty;
        private string _editPhone = string.Empty;
        private bool _isNewContact = false;

        public string EditName
        {
            get => _editName;
            set => Set(ref _editName, value);
        }

        public string EditPhone
        {
            get => _editPhone;
            set => Set(ref _editPhone, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public ContactEditViewModel(
            INavigationService navigation,
            IDialogService dialogService,
            IDbContextFactory<PhoneBookDbKonuh2307b2Context> contextFactory)  // ← ФАБРИКА!
        {
            _navigation = navigation;
            _dialogService = dialogService;
            _contextFactory = contextFactory;

            SaveCommand = new RelayCommand(SaveContact, CanSaveContact);
            CancelCommand = new RelayCommand(() => _navigation.NavigateTo<ContactsListViewModel>());
        }

        public void OnNavigatedTo(object? parameter)
        {
            if (parameter is Contact contact)
            {
                _contactId = contact.Id;
                _editName = contact.Name;
                _editPhone = contact.Phone;
                _isNewContact = false;
            }
            else
            {
                _editName = string.Empty;
                _editPhone = string.Empty;
                _isNewContact = true;
            }

            OnPropertyChanged(nameof(EditName));
            OnPropertyChanged(nameof(EditPhone));
        }

        private void SaveContact()
        {
            try
            {
                // Валидация
                if (string.IsNullOrWhiteSpace(EditName))
                {
                    _dialogService.ShowWarning("Введите имя контакта", "Ошибка");
                    return;
                }

                if (!IsPhoneValid(EditPhone))
                {
                    _dialogService.ShowWarning("Введите корректный номер телефона", "Ошибка");
                    return;
                }

                // ===== СОЗДАЁМ КОНТЕКСТ НА ВРЕМЯ ОПЕРАЦИИ =====
                using (var dbContext = _contextFactory.CreateDbContext())
                {
                    if (_isNewContact)
                    {
                        // ===== CREATE =====
                        var newContact = new Contact
                        {
                            Name = EditName,
                            Phone = EditPhone
                        };
                        dbContext.Contacts.Add(newContact);
                        dbContext.SaveChanges();
                    }
                    else
                    {
                        // ===== FETCH-MODIFY-SAVE =====
                        // 1. FETCH: загружаем актуальную сущность из БД
                        var contactToUpdate = dbContext.Contacts.Find(_contactId);

                        if (contactToUpdate == null)
                        {
                            _dialogService.ShowError("Контакт не найден в базе данных!", "Ошибка");
                            _navigation.NavigateTo<ContactsListViewModel>();
                            return;
                        }

                        // 2. MODIFY: переносим изменения из UI
                        contactToUpdate.Name = EditName;
                        contactToUpdate.Phone = EditPhone;

                        // 3. SAVE: Change Tracker сам определит изменения
                        dbContext.SaveChanges();
                    }
                } // ← КОНТЕКСТ УНИЧТОЖАЕТСЯ, Change Tracker очищается

                _dialogService.ShowInfo("Контакт сохранён!", "Успех");
                _navigation.NavigateTo<ContactsListViewModel>();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка при сохранении: {ex.Message}", "Ошибка");
            }
        }

        private bool CanSaveContact()
        {
            return !string.IsNullOrWhiteSpace(EditName) && IsPhoneValid(EditPhone);
        }

        private void CancelEdit()
        {
            _navigation.NavigateTo<ContactsListViewModel>();
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