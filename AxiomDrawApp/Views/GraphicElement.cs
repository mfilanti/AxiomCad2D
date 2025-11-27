using Axiom.GeoShape.Elements;
using Axiom.GeoShape.Entities;
using AxiomDrawApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace AxiomDrawApp.Views
{
	internal class GraphicElement
	{
		#region Properties

		/// <summary>
		/// Entità
		/// </summary>
		public Entity3D CadEntity { get; private set; }

		/// <summary>
		/// Entità grafica
		/// </summary>
		public UIElement UiEntity { get; private set; }

		private AABBox3D _box;

		/// <summary>
		/// Entità visualizzata
		/// </summary>
		public DrawingVisual Visual { get; private set; }
		#endregion Properties

		#region Constructors

		/// <summary>
		/// Init class
		/// </summary>
		public GraphicElement(Entity3D entity, UIElement uIElement)
		{
			CadEntity = entity;
			UiEntity = uIElement;
			Visual = null;
		}

		/// <summary>
		/// Init class
		/// </summary>
		public GraphicElement(Entity3D entity, DrawingVisual visual)
		{
			CadEntity = entity;
			Visual = visual;
			UiEntity = null;
			_box = CadEntity.GetAABBox();

		}

		#endregion Constructors

		#region Methods

		/// <summary>
		/// Restituisce il box in cui è inscritta la entità
		/// </summary>
		/// <param name="minItemX">valore minimo</param>
		/// <param name="minItemY">valore minimo</param>
		/// <param name="maxItemX">valore massimo</param>
		/// <param name="maxItemY">valore massimo</param>
		public void ToMinMax(out double minItemX, out double minItemY, out double maxItemX, out double maxItemY)
		{
			minItemX = _box.MinPoint.X;
			minItemY = _box.MinPoint.Y;
			maxItemX = _box.MaxPoint.X;
			maxItemY = _box.MaxPoint.Y;
		}
		#endregion
	}
}
