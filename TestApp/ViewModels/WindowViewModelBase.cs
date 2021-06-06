﻿using DataTools.PinEntry.Observable;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KuCoinApp.ViewModels
{
    public abstract class WindowViewModelBase : ObservableBase, IDisposable
    {

        protected CryptoCredentials cred;

        public abstract event EventHandler AskQuit;

        public virtual ICommand QuitCommand { get; protected set; }

        public CryptoCredentials Credentials => cred;


        protected abstract Task Initialize();


        public WindowViewModelBase()
        {
            //Initialize();
        }

        public abstract void Dispose();


    }
}
