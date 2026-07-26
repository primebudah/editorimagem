using System;
using System.Windows.Input;

namespace AIImageChat.ViewModels
{
    /// <summary>
    /// Implementação de ICommand para comandos simples
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        /// <summary>
        /// Construtor
        /// </summary>
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Evento de mudança no estado CanExecute
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Verificar se o comando pode ser executado
        /// </summary>
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute();
        }

        /// <summary>
        /// Executar o comando
        /// </summary>
        public void Execute(object? parameter)
        {
            _execute();
        }

        /// <summary>
        /// Notificar mudança no estado CanExecute
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Implementação de ICommand para comandos com parâmetro
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool>? _canExecute;

        /// <summary>
        /// Construtor
        /// </summary>
        public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Evento de mudança no estado CanExecute
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Verificar se o comando pode ser executado
        /// </summary>
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || (parameter is T t && _canExecute(t));
        }

        /// <summary>
        /// Executar o comando
        /// </summary>
        public void Execute(object? parameter)
        {
            if (parameter is T t)
                _execute(t);
        }

        /// <summary>
        /// Notificar mudança no estado CanExecute
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
