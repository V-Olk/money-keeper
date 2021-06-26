﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VOlkin.Dialogs.AddCard;
using VOlkin.Dialogs.Service;
using ExtensionMethods;
using VOlkin.HelpClasses;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using VOlkin.Dialogs.ReadTwoDates;

namespace VOlkin.ViewModels
{
    public class MainInfoViewModel : INotifyPropertyChanged
    {
        private static readonly IDialogService _dialogService = new DialogService();

        public DatabaseContext DbContext;
        public ObservableCollection<PaymentType> PaymentTypes { get; set; }

        public ObservableCollection<Transaction> Transactions { get; set; }

        public ObservableCollection<ChangeTimePeriod> TimePeriods { get; set; } = new ObservableCollection<ChangeTimePeriod>()
        {
            new ChangeTimePeriod("День", ChangeTimePeriodToDay),
            new ChangeTimePeriod("Неделю", ChangeTimePeriodToWeek),
            new ChangeTimePeriod("Месяц", ChangeTimePeriodToMonth),
            new ChangeTimePeriod("Год", ChangeTimePeriodToYear),
            new ChangeTimePeriod("Все время", ChangeTimePeriodToAllTime),
            new ChangeTimePeriod("Свой период", ChangeTimePeriodToCustom)
        };

        public decimal TotalMoney { get; set; }
        public static DateTime StartDate { get; set; }
        public static DateTime EndDate { get; set; } = DateTime.MaxValue;//default max val

        private ChangeTimePeriod _setCurTimePeriod;
        public ChangeTimePeriod SetCurTimePeriod
        {
            get => _setCurTimePeriod;
            set
            {
                if (value.Method())
                {
                    _setCurTimePeriod = value;
                    LoadTransactions();
                }
                OnPropertyChanged("SetCurTimePeriod");
            }
        }

        public MainInfoViewModel()
        {
            DbContext = new DatabaseContext();
            DbContext.PaymentTypes.Load();
            PaymentTypes = DbContext.PaymentTypes.Local;
            Transactions = DbContext.Transactions.Local;

            TotalMoney = PaymentTypes.Sum(pt => pt.MoneyAmount);
            SetCurTimePeriod = TimePeriods[0];
        }

        #region AddPaymentType

        private RelayCommand _addCardCommand;
        public RelayCommand AddCardCommand
        {
            get { return _addCardCommand ??= new RelayCommand(AddCard); }
        }

        private async void AddCard()
        {
            var addCardDialog = new AddCardDialogViewModel("Добавление нового счета");
            var addCardDialogRes = _dialogService.OpenDialog(addCardDialog);
            if (addCardDialogRes.Item1 == null || addCardDialogRes.Item2 == null)
                return;

            if (!decimal.TryParse(addCardDialogRes.Item2, out decimal moneyAmount))
            {
                var metroWindow = Application.Current.MainWindow as MetroWindow;
                await metroWindow.ShowMessageAsync("Ошибка", "Не удалось распознать строку кол-ва средств");
                return;
            }

            if (DbContext.PaymentTypes.FirstOrDefault(pt => pt.PaymentTypeName == addCardDialogRes.Item1) != null)
            {
                var metroWindow = Application.Current.MainWindow as MetroWindow;
                await metroWindow.ShowMessageAsync("Ошибка", "Счет с таким наименованием уже существует");
                return;
            }

            PaymentType newPT = new()
            {
                PaymentTypeName = addCardDialogRes.Item1,
                MoneyAmount = moneyAmount
            };

            DbContext.PaymentTypes.Add(newPT);
            DbContext.SaveChanges();

            ////TODO: it needs to be automated?
            TotalMoney += moneyAmount;
            OnPropertyChanged("TotalMoney");
        }

        #endregion

        #region ChangeTimePeriodMethods
        private static bool ChangeTimePeriodToDay() { StartDate = DateTime.Today; EndDate = DateTime.MaxValue; return true; }
        private static bool ChangeTimePeriodToWeek() { StartDate = DateTime.Today.FirstDayOfWeek(DayOfWeek.Monday); EndDate = DateTime.MaxValue; return true; }
        private static bool ChangeTimePeriodToMonth() { StartDate = DateTime.Today.FirstDayOfMonth(); EndDate = DateTime.MaxValue; return true; }
        private static bool ChangeTimePeriodToYear() { StartDate = DateTime.Today.FirstDayOfYear(); EndDate = DateTime.MaxValue; return true; }
        private static bool ChangeTimePeriodToAllTime() { StartDate = DateTime.MinValue; EndDate = DateTime.MaxValue; return true; }
        private static bool ChangeTimePeriodToCustom()
        {
            var addCardDialog = new ReadTwoDatesViewModel("Выбор периода времени", StartDate, EndDate);
            var addCardDialogRes = _dialogService.OpenDialog(addCardDialog);
            if (addCardDialogRes.Item1 == DateTime.MinValue && addCardDialogRes.Item2 == DateTime.MinValue)
                return false;

            StartDate = addCardDialogRes.Item1;
            EndDate = addCardDialogRes.Item2;

            return true;
        }
        #endregion

        private void LoadTransactions()
        {
            //DbContext.Transactions.Local.Clear();

            DbContext.Transactions.Local.ToList().ForEach(x =>
            {
                DbContext.Entry(x).State = EntityState.Detached;
                x = null; // this doesn't seem to be required for garbage collection
            });

            DbContext.Transactions.Where(tr => tr.DateTime > StartDate && tr.DateTime <= EndDate).OrderByDescending(tr => tr.DateTime).Load();
            OnPropertyChanged("Transactions");
        }

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        #endregion
    }
}

namespace ExtensionMethods
{
    public static class DateTimeExtensions
    {
        public static DateTime FirstDayOfYear(this DateTime value) => new DateTime(value.Year, 1, 1);

        public static DateTime FirstDayOfMonth(this DateTime value) => new DateTime(value.Year, value.Month, 1);

        public static DateTime FirstDayOfWeek(this DateTime value, DayOfWeek startOfWeek)
        {
            int diff = (7 + (value.DayOfWeek - startOfWeek)) % 7;
            return value.AddDays(-1 * diff).Date;
        }
    }
}