using Axiom.GeoShape.Shapes;
using Axiom.WpfFrameworks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AxiomDrawApp.ViewModel
{
	public class MainViewModel : ObservableObject
	{
		#region Fields

		#endregion

		#region Properties

		/// <summary>
		/// Titolo
		/// </summary>
		private string _title = "";
		/// <summary>
		/// Titolo
		/// </summary>
		public string Title
		{
			get => _title;
			set => SetProperty(ref _title, value);
		}

		/// <summary>
		/// Lista di comandi
		/// </summary>
		public ObservableCollection<CommandViewModel> Commands { get; set; } = new();


		/// <summary>
		/// Documento
		/// </summary>
		private WpfDocument _document;
		/// <summary>
		/// Documento
		/// </summary>
		public WpfDocument Document
		{
			get => _document;
			set => SetProperty(ref _document, value);
		}

		#endregion

		#region Constructors
		/// <summary>
		/// Ctor
		/// </summary>
		public MainViewModel()
		{
			Title = "Cad 2D";
			Document = new WpfDocument();
			Commands.Add(new(null, "Rect", "", CreateRectangle));
			CreateRectangle();
		}

		#endregion

		#region Methods
		private void CreateRectangle()
		{
			Document.AddShape(new Shape2DRectangle(100, 50, 0, "", "", ""));
		}

		#endregion

	}
}
