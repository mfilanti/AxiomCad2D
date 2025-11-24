using AxiomDrawApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AxiomDrawApp.Views
{
	internal class GraphicElement
	{
		#region Properties

		/// <summary>
		/// Entità
		/// </summary>
		public WpfEntity CadEntity { get; private set; }

		/// <summary>
		/// Entità grafica
		/// </summary>
		public UIElement UiEntity { get; private set; }

		#endregion Properties

		#region Constructors

		/// <summary>
		/// Init class
		/// </summary>
		public GraphicElement(WpfEntity entity, UIElement uIElement)
		{
			CadEntity = entity;
			UiEntity = uIElement;
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
			CadEntity.ToMinMax(out minItemX, out minItemY, out maxItemX, out maxItemY);
		}
		#endregion
	}
}
