using Axiom.GeoMath;
using Axiom.GeoShape.Shapes;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxiomDrawApp.ViewModel
{
	/// <summary>
	/// Viewmodel per le forme
	/// </summary>
	public class ShapeViewModel : ObservableObject
	{
		#region Fields
		private Shape2D _shape;

		private WpfDocument _document;
		#endregion

		#region Properties

		/// <summary>
		/// Nome
		/// </summary>
		private string _name;
		/// <summary>
		/// Nome
		/// </summary>
		public string Name
		{
			get => _name;
			set => SetProperty(ref _name, value);
		}


		/// <summary>
		/// OffsetX
		/// </summary>
		private double _offsetX;
		/// <summary>
		/// OffsetX
		/// </summary>
		public double OffsetX
		{
			get => _offsetX;
			set => SetProperty(ref _offsetX, value);
		}


		/// <summary>
		/// Offset Y
		/// </summary>
		private double _offsetY;
		/// <summary>
		/// Offset Y
		/// </summary>
		public double OffsetY
		{
			get => _offsetY;
			set => SetProperty(ref _offsetY, value);
		}


		/// <summary>
		/// Offset Z
		/// </summary>
		private double _offsetZ;
		/// <summary>
		/// Offset Z
		/// </summary>
		public double OffsetZ
		{
			get => _offsetZ;
			set => SetProperty(ref _offsetZ, value);
		}


		/// <summary>
		/// Rotazione
		/// </summary>
		private double _degRot;
		/// <summary>
		/// Rotazione
		/// </summary>
		public double DegRot
		{
			get => _degRot;
			set => SetProperty(ref _degRot, value);
		}

		/// <summary>
		/// PArametri
		/// </summary>
		public ObservableCollection<ParameterViewModel> Parameters { get; set; } = new();
		#endregion

		#region Constructors

		/// <summary>
		/// Ctor
		/// </summary>
		public ShapeViewModel(WpfDocument document, Shape2D shape)
		{
			_document = document;
			_shape = shape;
			Name = _shape.GetType().Name;
			OffsetX = _shape.X; OffsetY = _shape.Y; OffsetZ = _shape.Z;
			_shape.GetRotation(out _, out _, out double zRagAngle);
			DegRot = MathUtils.RadToDegree(zRagAngle);
			foreach (var parameter in _shape.Parameters)
			{
				Parameters.Add(new ParameterViewModel(_document, _shape, parameter));
			}
			PropertyChanged += OnPropertyChanged;
		}

		#endregion

		#region Methods

		private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			UpdateShape();
			_document.ChangeShape(_shape);
		}

        private void UpdateShape()
        {
            _shape.X = OffsetX;
			_shape.Y = OffsetY;
			_shape.Z = OffsetZ;
			_shape.GetRotation(out double x, out double y, out _);
			_shape.SetRotation(x, y, MathUtils.DegreeToRad(DegRot));
        }
        #endregion
    }
}
