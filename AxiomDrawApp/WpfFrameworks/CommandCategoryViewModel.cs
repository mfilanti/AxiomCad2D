using Axiom.WpfFrameworks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Axiom.WpfFrameworks
{
	/// <summary>
	/// Categorie
	/// </summary>
	public class CommandCategoryViewModel : HierarchyViewModel<ICommandItem>, ICommandItem
	{
		/// <summary>
		/// COmandi legati alla categoria
		/// </summary>
		public ObservableCollection<CommandViewModel> Commands { get; set; } = new();
		#region Fields

		#endregion Fields

		#region Properties

		/// <summary>
		/// Numero di Figli
		/// </summary>
		public int Count => Children.Count;

		/// <summary>
		/// Ottiene\Imposta l'inizio di un gruppo di comandi
		/// </summary>
		private bool _beginGroup;

		/// <summary>
		/// Ottiene\Imposta l'inizio di un gruppo di comandi
		/// </summary>
		public bool BeginGroup
		{
			get => _beginGroup;
			set => SetProperty(ref _beginGroup, value);
		}

		/// <summary>
		/// Tooltip
		/// </summary>
		public TooltipViewModel Tooltip { get; private set; } = null;

		
		#endregion Properties

		#region Constructors

		/// <summary>
		/// Inizializza la classe impostando il comportamento della selezione dell'ultimo comando
		/// </summary>
		/// <param name="icon">Icona della categoria</param>
		/// <param name="title">Titolo della categoria</param>
		/// <param name="tooltipViewModel">Tooltip</param>
		/// <param name="updatedLastCommand">Quando si esegue un comando viene aggiornata la property <see cref="CategoryCommand"/></param>
		/// <param name="updateBrowsable">Funzione di aggiornamento della visibilità (se null allora sempre visibile).</param>
		/// <param name="updateEnabled">Aggiorna l'abilitazione della categoria</param>
		public CommandCategoryViewModel(
			IMessenger messenger,
			IExtendedTreeViewItem parent,
			string title,
			TooltipViewModel tooltipViewModel)
			: base(messenger, parent, title)
		{
			Tooltip = tooltipViewModel;
			UpdateCanExecute();
		}

		#endregion Constructors

		#region Methods

		/// <summary>
		/// Aggiunge un comando alla categoria comandi
		/// </summary>
		/// <param name="command">Comando da aggiungere</param>
		public void AddChild(CommandViewModel command)
		{
			if (command != null)
			{
				Commands.Add(command);
				AddChild(command as ICommandItem);
			}
		}


		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public void UpdateCanExecute()
		{
			foreach (var comm in Children)
			{
				comm.UpdateCanExecute();
			}
		}

		/// <summary>
		/// Restituisce tutti i comandi della categoria
		/// </summary>
		/// <returns></returns>
		internal IEnumerable<CommandViewModel> GetAllCommands()
		{
			List<CommandViewModel> r = new(Commands);
			Stack<CommandCategoryViewModel> nav = new(Children.OfType<CommandCategoryViewModel>().ToList());
			while (nav.Count > 0)
			{
				var c = nav.Pop();
				r.AddRange(c.GetAllCommands());
			}
			return r;
		}

		#endregion Methods
	}
}
