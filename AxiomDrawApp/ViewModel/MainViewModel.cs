using Axiom.GeoMath;
using Axiom.GeoShape.Shapes;
using Axiom.WpfFrameworks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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
		/// VM delle shape
		/// </summary>
		public ObservableCollection<ShapeViewModel> Shapes { get; set; } = new();
		/// <summary>
		/// Lista di comandi
		/// </summary>
		public ObservableCollection<CommandCategoryViewModel> Commands { get; set; } = new();


		/// <summary>
		/// Documento
		/// </summary>
		private WpfDocument _document;
        private WeakReferenceMessenger _messenger;

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
			_messenger = new WeakReferenceMessenger();
			Title = "Cad 2D";
			Document = new WpfDocument();
			AddShapeCommands();
			CreateRectangle(100, 50, 0);
			CreateRectangle(100, 200, 45);
		}

        private void AddShapeCommands()
        {
			int angleTest = 0;

			CommandCategoryViewModel category = new(_messenger, null, "Shape", null);
			category.AddChild(new CommandViewModel(null, "Rect", "", () =>
			{
				CreateRectangle(10, 50, angleTest);
				angleTest += 10;
			}));
			Commands.Add (category);
		}

		#endregion

		#region Methods
		private void CreateRectangle(double width, double height, double angle)
		{
			var shape = new Shape2DRectangle(width, height, 0, "", "", "");
			shape.SetRotation(0,0, MathUtils.DegreeToRad(angle));

			ShapeViewModel shapeViewModel = new(Document, shape);
			Shapes.Add(shapeViewModel);
			Document.AddShape(shape);
		}

		#endregion

	}
}
