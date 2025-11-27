using Axiom.GeoShape;
using Axiom.GeoShape.Entities;
using Axiom.GeoShape.Shapes;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxiomDrawApp.ViewModel
{
	/// <summary>
	/// Documento wpf
	/// </summary>
	public class WpfDocument : ObservableObject
	{
		#region Events
		/// <summary>
		/// E' stata aggiunta una nuova shape
		/// </summary>
		public event EventHandler<Entity3D> OnAddedShape;

		/// <summary>
		/// E' stata modificata una shape
		/// </summary>
		public event EventHandler<Entity3D> OnChangedShape;

		/// <summary>
		/// E' stata rimossa una nuova shape
		/// </summary>
		public event EventHandler<Entity3D> OnRemovedShape;
		#endregion

		#region Fields

		#endregion

		#region Properties
		/// <summary>
		/// Root
		/// </summary>
		private Node3D _root = new();
		/// <summary>
		/// Root
		/// </summary>
		public Node3D Root
		{
			get => _root;
			set => SetProperty(ref _root, value);
		}
		public string Color { get; set; } = "#FF0000";
		public double Thickness { get; set; } = 1.0;
		#endregion

		#region Constructors
		/// <summary>
		/// Costruttore
		/// </summary>
		public WpfDocument()
		{

		}
		#endregion

		#region Methods

		public void AddShape(Shape2D shape)
		{
			_root.AddEntity(shape);
		}
		#endregion
	}
}
