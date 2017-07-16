using System;
using System.Windows.Input;

namespace UnityLauncher.Core
{
    public class Command : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public event CancelCommandEventHandler Executing;
        public event CommandEventHandler Executed;
        protected Action action;
        protected Action<object> parameterizedAction;
        private bool canExecute;

        public Command(Action action, bool canExecute = true)
        {
            this.action = action;
            this.canExecute = canExecute;
        }

        
        public Command(Action<object> parameterizedAction, bool canExecute = true)
        {
            this.parameterizedAction = parameterizedAction;
            this.canExecute = canExecute;
        }
        
        public virtual void DoExecute(object param)
        {
            CancelCommandEventArgs args = new CancelCommandEventArgs() { Parameter = param, Cancel = false };
            InvokeExecuting(args);
         
            if (args.Cancel)
                return;

            InvokeAction(param);
            InvokeExecuted(new CommandEventArgs() { Parameter = param });
        }

        protected void InvokeAction(object param)
        {
            Action theAction = action;
            Action<object> theParameterizedAction = parameterizedAction;
            if (theAction != null)
                theAction();
            else
            {
                theParameterizedAction?.Invoke(param);
            }
        }

        protected void InvokeExecuted(CommandEventArgs args)
        {
            Executed?.Invoke(this, args);
        }

        protected void InvokeExecuting(CancelCommandEventArgs args)
        {
            Executing?.Invoke(this, args);
        }
        
        public bool CanExecute
        {
            get => canExecute;
            set
            {
                if (canExecute != value)
                {
                    canExecute = value;
                    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        bool ICommand.CanExecute(object parameter)
        {
            return canExecute;
        }
        
        void ICommand.Execute(object parameter)
        {
            DoExecute(parameter);

        }
    }
}