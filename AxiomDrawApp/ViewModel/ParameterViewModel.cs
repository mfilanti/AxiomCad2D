using Axiom.GeoMath;
using Axiom.GeoShape;
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
    /// ViewModel deli parametri
    /// </summary>
    public class ParameterViewModel : ObservableObject
    {
        private WpfDocument _document;
        private Shape2D _shape;
        #region Fields
        private Parameter _parameter;
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
			private set => SetProperty(ref _name, value);
		}


		/// <summary>
		/// Valore
		/// </summary>
		private double _value;
		/// <summary>
		/// Valore
		/// </summary>
		public double Value
		{
			get => _value;
			set => SetProperty(ref _value, value);
		}

		#endregion

		#region Constructors

		/// <summary>
		/// ctor
		/// </summary>
		public ParameterViewModel(WpfDocument wpfDocument,Shape2D shape, Parameter item)
		{
			_document = wpfDocument;
			_shape = shape;
			_parameter = item;
			Name = item.Name;
			Value = item.Value;
            PropertyChanged += ParameterViewModel_PropertyChanged;
		}

        private void ParameterViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
			UpdateShape();
			_document.ChangeShape(_shape);
		}
		#endregion

		#region Methods
		private void UpdateShape()
		{
			_parameter.Value = Value;
		}
		#endregion


	}
}
