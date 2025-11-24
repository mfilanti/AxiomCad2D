using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxiomDrawApp.Model
{
	/// <summary>
	/// Entity for WPF elements
	/// </summary>
	public class WpfEntity
	{
		#region Fields

		#endregion

		#region Properties
		/// <summary>
		/// Undice univoco dell'elemento
		/// </summary>
		public string UniqueId { get; }

		/// <summary>
		/// Colore in esadecimale
		/// </summary>
		public string Color { get; private set; } = string.Empty;

		/// <summary>
		/// Largezza box
		/// </summary>
		public double Width { get; protected set; } = double.NaN;

		/// <summary>
		/// altezza box
		/// </summary>
		public double Height { get; protected set; } = double.NaN;

		/// <summary>
		/// E' selezionabile
		/// </summary>
		public bool IsSelectable { get; set; } = false;

		/// <summary>
		/// E' ridimensionabile
		/// </summary>
		public bool IsResizable { get; set; } = true;

		/// <summary>
		/// Spessore
		/// </summary>
		public double Thickness { get; set; } = 1.0;
		#endregion

		#region Constructors
		/// <summary>
		/// Inizializza la classe
		/// </summary>
		public WpfEntity()
		{
			UniqueId = Guid.NewGuid().ToString();
		}
		#endregion

		#region Methods
		/// <summary>
		/// Restituisce il minimo\massimo in x e y
		/// </summary>
		/// <param name="minX">Minimo in X</param>
		/// <param name="minY">Minimo in Y</param>
		/// <param name="maxX">Massimo in X</param>
		/// <param name="maxY">Massimo in Y</param>
		public virtual void ToMinMax(out double minX, out double minY, out double maxX, out double maxY)
		{
			minX = maxX = 0;// Position.X;
			minY = maxY = 0;//Position.Y;
		}


		/// <summary>
		/// Imposta il colore in formato esadecimale
		/// </summary>
		/// <param name="b">Blue</param>
		/// <param name="g">Verde</param>
		/// <param name="r">Rosso</param>
		public void SetRGBColor(byte r, byte g, byte b)
		{
			Color = BitConverter.ToString(new byte[3] { r, g, b }).Replace("-", "");
		}

		

		#endregion
	}
}
