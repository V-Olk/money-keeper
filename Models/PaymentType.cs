﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VOlkin.HelpClasses;
using VOlkin.Models;

namespace VOlkin
{
    public class PaymentType : StateSupport
    {
        public PaymentType(string ptn, decimal moneyAmount)
        {
            PaymentTypeName = ptn;
            MoneyAmount = moneyAmount;
        }

        private PaymentType() { }

        [Key]
        public int PaymentTypeId { get; private set; }
        [Required]
        public string PaymentTypeName { get; private set; }
        [Required]
        public decimal MoneyAmount { get; private set; }

        //public Banks BankFk { get; set; }

        public static PaymentType operator +(PaymentType pt, decimal moneyAmount)
        {
            pt.MoneyAmount += moneyAmount;
            return pt;
        }

        public static PaymentType operator -(PaymentType pt, decimal moneyAmount)
        {
            pt.MoneyAmount -= moneyAmount;
            pt.OnPropertyChanged("MoneyAmount");
            return pt;
        }

        public void Increase(decimal moneyAmount)
        {
            MoneyAmount += moneyAmount;
            OnPropertyChanged("MoneyAmount");
        }

        public void Decrease(decimal moneyAmount)
        {
            MoneyAmount -= moneyAmount;
            OnPropertyChanged("MoneyAmount");
        }

        public override string ToString() => PaymentTypeName;
    }
}
