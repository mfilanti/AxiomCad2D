using Axiom.GeoMath;
using Axiom.GeoShape.Curves;
using System.Runtime.Serialization;

namespace Axiom.GeoShape.Shapes
{
	/// <summary>
	/// Classe shape rettangolo
	/// </summary>
	[DataContract]
	[Serializable]
	public class Shape2DRectangle : Shape2D
	{
		#region static 
		private const string Pwidth = "width";
		private const string Pheight = "height";
		private const string Pradius = "radius";
		#endregion
		#region Properties
		/// <summary>
		/// Dimensione X del rettangolo
		/// </summary>
		public double LX { get => _geometricParameters[Pwidth].Value; set => _geometricParameters[Pwidth].Value = value; }

		/// <summary>
		/// Dimensione Y del rettangolo
		/// </summary>
		[DataMember]
		public double LY { get => _geometricParameters[Pheight].Value; set => _geometricParameters[Pheight].Value = value; }

		/// <summary>
		/// Raggio di stondatura del rettangolo. 
		/// Può essere zero.
		/// </summary>
		[DataMember]
		public double Radius { get => _geometricParameters[Pradius].Value; set => _geometricParameters[Pradius].Value = value; }

		/// <summary>
		/// Formula dimensione X
		/// </summary>
		[DataMember]
		public string LXFormula { get => _geometricParameters[Pwidth].Formula; set => _geometricParameters[Pwidth].Formula = value; }

		/// <summary>
		/// Formula dimensione Y
		/// </summary>
		[DataMember]
		public string LYFormula { get => _geometricParameters[Pheight].Formula; set => _geometricParameters[Pheight].Formula = value; }

		/// <summary>
		/// Formula raggio di stondatura
		/// </summary>
		[DataMember]
		public string RadiusFormula { get => _geometricParameters[Pradius].Formula; set => _geometricParameters[Pradius].Formula = value; }

		#endregion PUBLIC FIELDS

		#region CONSTRUCTORS
		/// <summary>
		/// Costruttore di default
		/// </summary>
		public Shape2DRectangle() : base()
		{
			_geometricParameters.Add(Pwidth, new Axiom.GeoShape.Parameter(Pwidth, true, "", 0));
			_geometricParameters.Add(Pheight, new Axiom.GeoShape.Parameter(Pheight, true, "", 0));
			_geometricParameters.Add(Pradius, new Axiom.GeoShape.Parameter(Pradius, true, "", 0));
		}

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="width">Dimensione X</param>
		/// <param name="height">Dimensione Y</param>
		/// <param name="radius">Raggio</param>
		/// <param name="widthFormula">Formula dimensione X</param>
		/// <param name="heightFormula">Formula dimensione Y</param>
		/// <param name="radiusFormula">Formula raggio</param>
		public Shape2DRectangle(double width, double height, double radius, string widthFormula, string heightFormula, string radiusFormula) : base()
		{
			_geometricParameters.Add(Pwidth, new Axiom.GeoShape.Parameter(Pwidth, true, widthFormula, width));
			_geometricParameters.Add(Pheight, new Axiom.GeoShape.Parameter(Pheight, true, heightFormula, height));
			_geometricParameters.Add(Pradius, new Axiom.GeoShape.Parameter(Pradius, true, radiusFormula, radius));
		}
		#endregion CONSTRUCTORS

		#region PUBLIC METHODS
		/// <summary>
		/// Indica se può essere svuotato
		/// </summary>
		public override bool CanEmpty
		{
			get { return true; }
		}

		/// <summary>
		/// Restituisce la figura dello shape
		/// </summary>
		/// <returns></returns>
		public override Figure3D GetFigure()
		{
			Figure3D result = new Figure3D();
			double l = LX;
			double h = LY;
			double r;
			// Questa if ha senso, non semplificare (offset può essere negativo)
			if (Radius == 0)
				r = 0;
			else
				r = Radius;

			if (r < 0)
				r = 0;

			result = CreateFigure(l, h, r);

			if (InverseVersus)
				result = result.Inverse();

			//if (MirrorY)
			//	result = result.MirrorY();

			return result;
		}

		/// <summary>
		/// Valida lo Shape2D
		/// </summary>
		/// <returns></returns>
		public override bool Validate()
		{
			bool result = true;
			if ((LX <= 0) || (LY <= 0) || (Radius < 0))
			{
				result = false;
			}
			else
			{
				// Il raggio * 2 deve essere maggiore o uguale alle dimensioni
				// Ho lasciato l'uguale, permettendo quindi il caso di linee 
				// orizzontali e/o verticali nulle
				if ((2 * Radius > LX) || (2 * Radius > LY))
				{
					result = false;
				}
				else
				{
					double l = LX;
					double h = LY;
					double r = Radius;
					if (r < 0)
						r = 0;

					if (l < 0 || h < 0)
					{
						result = false;
					}
					else
					{
						if (2 * r > l || 2 * r > h)
							result = false;
					}
				}

			}
			return result;
		}

		/// <summary>
		/// Clona lo Shape2D
		/// </summary>
		/// <returns></returns>
		public override Shape2D Clone()
		{
			Shape2D result = new Shape2DRectangle(LX, LY, Radius, LXFormula, LYFormula, RadiusFormula);
			CloneTo(result);
			return result;
		}
		#endregion Public methods

		#region Private methods
		private Figure3D CreateFigure(double lX, double lY, double r)
		{
			Figure3D result = new Figure3D();
			if ((lX > 0) && (lY > 0))
			{
				if (lX - 2 * r > 0)
					result.Add(new Line3D(0, -lY / 2, -lX / 2 + r, -lY / 2));

				if (r > 0)
					result.Add(new Arc3D(new Point3D(-lX / 2 + r, -lY / 2 + r), r, 1.5 * Math.PI, Math.PI, false));

				if (lY - 2 * r > 0)
					result.Add(new Line3D(-lX / 2, -lY / 2 + r, -lX / 2, lY / 2 - r));

				if (r > 0)
					result.Add(new Arc3D(new Point3D(-lX / 2 + r, lY / 2 - r), r, Math.PI, 0.5 * Math.PI, false));

				if (lX - 2 * r > 0)
					result.Add(new Line3D(-lX / 2 + r, lY / 2, lX / 2 - r, lY / 2));

				if (r > 0)
					result.Add(new Arc3D(new Point3D(lX / 2 - r, lY / 2 - r), r, 0.5 * Math.PI, 0, false));

				if (lY - 2 * r > 0)
					result.Add(new Line3D(lX / 2, lY / 2 - r, lX / 2, -lY / 2 + r));

				if (r > 0)
					result.Add(new Arc3D(new Point3D(lX / 2 - r, -lY / 2 + r), r, 0, 1.5 * Math.PI, false));

				if (lX - 2 * r > 0)
					result.Add(new Line3D(lX / 2 - r, -lY / 2, 0, -lY / 2));
			}
			else if ((lX == 0) && (lY == 0) && (r == 0))
			{
				// Linea nulla per farlo comunque entrare e uscire
				result.Add(new Line3D());
			}
			else if (lY > 0 && lX == 0 && r == 0)
			{
				result.Add(new Line3D(0, -lY / 2, 0, lY / 2));
			}
			else if (lX > 0 && lY == 0 && r == 0)
			{
				result.Add(new Line3D(lX / 2, 0, -lX / 2, 0));
			}

			return result;
		}

		#endregion

	}
}
