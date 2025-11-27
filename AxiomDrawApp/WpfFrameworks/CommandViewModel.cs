using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Axiom.WpfFrameworks
{
	/// <summary>
	/// Comando
	/// </summary>
	public class CommandViewModel : ObservableObject
	{
		#region Properties

		/// <summary>
		/// Icona
		/// </summary>
		private object _icon = null;

		/// <summary>
		/// Icona
		/// </summary>
		public object Icon
		{
			get => _icon;
			set => SetProperty(ref _icon, value);
		}

		/// <summary>
		/// Titolo
		/// </summary>
		private string _title = string.Empty;

		/// <summary>
		/// Titolo
		/// </summary>
		public string Title
		{
			get => _title;
			set => SetProperty(ref _title, value);
		}

		/// <summary>
		/// Descrizione
		/// </summary>
		private string _description = string.Empty;

		/// <summary>
		/// Descrizione
		/// </summary>
		public string Description
		{
			get => _description;
			set => SetProperty(ref _description, value);
		}

		/// <summary>
		/// Comando di esecuzione dell'action
		/// </summary>
		public ICommand ExecuteCommand { get;  }

		/// <summary>
		/// Tooltip
		/// </summary>
		private TooltipViewModel _tooltip = null;

		/// <summary>
		/// Tooltip
		/// </summary>
		public TooltipViewModel Tooltip
		{
			get => _tooltip;
			set => SetProperty(ref _tooltip, value);
		}

		/// <summary>
		/// Comando abilitato
		/// </summary>
		private bool _isEnabled = true;

		/// <summary>
		/// Comando abilitato
		/// </summary>
		public bool IsEnabled
		{
			get => _isEnabled;
			set => SetProperty(ref _isEnabled, value);
		}

		/// <summary>
		/// Indica se il comando può essere eseguito.
		/// </summary>
		public bool CanExecuteCommand { get => ExecuteCommand.CanExecute(null); }

		#endregion Properties

		#region Constructors

		/// <summary>
		/// Initialize
		/// </summary>
		public CommandViewModel(
			object iconResourceKey,
			string title,
			string description,
			Action action,
			Func<bool> canExecute = null)
		{
			Icon = iconResourceKey;
			Title = title;
			Description = description;
			if (canExecute != null)
			{
				ExecuteCommand = new RelayCommand(() => action?.Invoke(), canExecute);
			}
			else
			{
				ExecuteCommand = new RelayCommand(() => action?.Invoke());
			}
			if (!string.IsNullOrEmpty(description))
			{
				Tooltip = new TooltipViewModel() { Title = description };
			}
		}

		#endregion Constructors

		#region Methods

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public void UpdateCanExecute()
		{
			if (ExecuteCommand is RelayCommand command)
			{
				command.NotifyCanExecuteChanged();
			}
			OnPropertyChanged(nameof(CanExecuteCommand));
			IsEnabled = CanExecuteCommand;
		}

		#endregion Methods
	}
}
