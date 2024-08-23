using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Utilities
{
    public class RelayCommand : ICommand
    {

        #region Fields

        private Action<object> execute;
        private Predicate<object> canExecute;
        private Func<Action<object>> startCountdown;
        private event EventHandler CanExecuteChangedInternal;

        #endregion


        #region Constructors

        public RelayCommand(Action<object> execute)
          : this(execute, DefaultCanExecute)
        {
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            if (canExecute == null)
            {
                throw new ArgumentNullException("canExecute");
            }

            this.execute = execute;
            this.canExecute = canExecute;
        }

        public RelayCommand(Func<Action<object>> startCountdown)
        {
            this.startCountdown = startCountdown;
        }

        #endregion


        #region Events

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
                this.CanExecuteChangedInternal += value;
            }

            remove
            {
                CommandManager.RequerySuggested -= value;
                this.CanExecuteChangedInternal -= value;
            }
        }

        #endregion


        #region Public methods

        public bool CanExecute(object parameter)
        {
            return this.canExecute != null && this.canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            this.execute(parameter);
        }

        public void OnCanExecuteChanged()
        {
            EventHandler handler = this.CanExecuteChangedInternal;
            if (handler != null)
            {
                handler.Invoke(this, EventArgs.Empty);
            }
        }

        public void Destroy()
        {
            this.canExecute = _ => false;
            this.execute = _ => { return; };
        }

        #endregion


        #region Other methods

        private static bool DefaultCanExecute(object parameter)
        {
            return true;
        }

        #endregion
    }
}
