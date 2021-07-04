﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VOlkin.Dialogs.Service
{
    public abstract class DialogViewModelBase<T>
    {
        public string Title { get; protected set; }
        public string Message { get; protected set; }
        public T DialogResult { get; protected set; }

        public DialogViewModelBase() : this(string.Empty, string.Empty) { }
        public DialogViewModelBase(string title) : this(title, string.Empty) { }
        public DialogViewModelBase(string title, string message)
        {
            Title = title;
            Message = message;
        }

        public void CloseDialogWithResult(IDialogWindow dialog, T result)
        {
            DialogResult = result;

            if (dialog != null)
                dialog.DialogResult = true;
        }
    }
}
