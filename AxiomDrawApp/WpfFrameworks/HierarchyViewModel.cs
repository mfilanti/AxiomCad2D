using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Axiom.WpfFrameworks
{
	/// <summary>
	/// Viewmodel gerarchico
	/// </summary>
	public class HierarchyViewModel<T> : ObservableRecipient, IExtendedTreeViewItem
	{
		/// <summary>
		/// Lista dei figli.
		/// </summary>
		public ObservableCollection<T> Children { get; set; } = new ObservableCollection<T>();

		/// <summary>
		/// Parent
		/// </summary>
		private IExtendedTreeViewItem _parent;
		/// <summary>
		/// Parent
		/// </summary>
		public IExtendedTreeViewItem NodeParent
		{
			get => _parent;
			set => SetProperty(ref _parent, value);
		}

		/// <summary>
		/// Indica che il viewmodel ha dei figli
		/// </summary>
		private bool _hasChildren = false;
		/// <summary>
		/// Indica che il viewmodel ha dei figli
		/// </summary>
		public bool HasChildren
		{
			get => _hasChildren;
			set => SetProperty(ref _hasChildren, value);
		}

		/// <summary>
		/// Indica al viewmodel di attivarsi
		/// </summary>
		private bool _isExpanded;
		/// <summary>
		/// Indica al viewmodel di attivarsi
		/// </summary>
		public bool IsExpanded
		{
			get => _isExpanded;
			set
			{
				if (SetProperty(ref _isExpanded, value) && value)
				{
					if (Children.Count == 0)
					{
						LoadChildren();
					}
				}
			}
		}

		/// <summary>
		/// Indica al viewmodel di attivarsi
		/// </summary>
		private bool _selected;
		/// <summary>
		/// Indica al viewmodel di attivarsi
		/// </summary>
		public bool Selected
		{
			get => _selected;
			set => SetProperty(ref _selected, value);
		}

		/// <summary>
		/// Indica al viewmodel che è parte della selezione corrente.
		/// </summary>
		private bool _isCurrentSelection;
		/// <summary>
		/// Indica al viewmodel che è parte della selezione corrente.
		/// </summary>
		public bool IsCurrentSelection
		{
			get => _isCurrentSelection;
			set => SetProperty(ref _isCurrentSelection, value);
		}

		/// <summary>
		/// Nome
		/// </summary>
		private string _title;
		/// <summary>
		/// Nome
		/// </summary>
		public string Title
		{
			get => _title;
			set => SetProperty(ref _title, value);
		}


		private bool _isSelectable = true;
		/// <summary>
		/// Indica se l'elemento è selezionabile nell'albero.
		/// </summary>
		public bool IsSelectable
		{
			get => _isSelectable;
			set => SetProperty(ref _isSelectable, value);
		}

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="messenger">Gestore dei messaggi.</param>
		/// <param name="parent">Elemento contenitore.</param>
		public HierarchyViewModel(IMessenger messenger, IExtendedTreeViewItem parent, string name) : 
			base(messenger)
		{
			NodeParent = parent;
			Title = name;
		}

		/// <summary>
		/// Aggiunge un elemento figlio.
		/// </summary>
		/// <param name="child">Elemento da aggiungere ai figli.</param>
		/// <param name="tx">Eventuale transazione.</param>
		protected void AddChild(T child)
		{
			Children.Add(child);
			HasChildren = true;
		}

		/// <summary>
		/// Aggiunge un elemento figlio.
		/// </summary>
		/// <param name="child">Elemento da aggiungere ai figli.</param>
		/// <param name="index">Indice di posizionamento dell'elemento nella lista.</param>
		/// <param name="tx">Eventuale transazione.</param>
		protected void AddChild(T child, int index)
		{
			if (index != -1)
			{
				Children.Insert(index, child);
				HasChildren = true;
			}
			else
			{
				AddChild(child);
			}
		}

		/// <summary>
		/// Elimina un elemento figlio.
		/// </summary>
		/// <param name="child">Elemento da eliminare.</param>
		/// <param name="tx">Eventuale transazione.</param>
		protected void RemoveChild(T child)
		{
			Children.Remove(child);
			HasChildren = Children.Any();
		}
		/// <summary>
		/// Elimina tutti i children
		/// </summary>
		protected virtual void ClearChildren()
		{
			HasChildren = false;
			Children.Clear();
		}

		/// <summary>
		/// Carica i figli
		/// </summary>
		protected virtual void LoadChildren()
		{

		}
	}
}
