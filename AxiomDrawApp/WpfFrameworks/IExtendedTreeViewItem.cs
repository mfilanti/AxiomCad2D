using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.WpfFrameworks
{
	/// <summary>
	/// Interfaccia per i viewmodel da utilizzare come elementi per ExtendedTreeViewUc/>.
	/// </summary>
	public interface IExtendedTreeViewItem
	{
		/// <summary>
		/// Parent
		/// </summary>
		IExtendedTreeViewItem NodeParent { get; }

		/// <summary>
		/// Indica che il nodo è espanso
		/// </summary>
		bool IsExpanded { get; set; }

		/// <summary>
		/// Indica che il nodo è selezionato
		/// </summary>
		bool Selected { get; set; }

		/// <summary>
		/// Indica che il nodo fa parte della selezione corrente
		/// </summary>
		bool IsCurrentSelection { get; set; }

		/// <summary>
		/// Indica che il nodo ha dei sotto-nodi
		/// </summary>
		bool HasChildren { get; set; }

		/// <summary>
		/// Indica se il nodo è selezionabile
		/// </summary>
		bool IsSelectable { get; set; }

	}
}
