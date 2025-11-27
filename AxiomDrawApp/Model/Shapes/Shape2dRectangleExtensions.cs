using Axiom.GeoMath;
using Axiom.GeoShape.Curves;
using Axiom.GeoShape.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxiomDrawApp.Model.Shapes
{
	/// <summary>
	/// Estensione
	/// </summary>
	public static class Shape2dRectangleExtensions
	{
		/// <summary>
		/// A partire da una figura 2D indica se si tratta di un rettangolo (eventualmente stondato). 
		/// Anche se ci sono linee o archi divisi in più parti.
		/// </summary>
		public static bool IsShape2DRectangle(this Shape2D figure) => IsShape2DRectangle(figure, 0.01, out _, out _, out _, out _, out _, out _);

		/// <summary>
		/// A partire da una figura 2D indica se si tratta di un rettangolo (eventualmente stondato). 
		/// Anche se ci sono linee o archi divisi in più parti.
		/// </summary>
		/// <returns></returns>
		public static bool IsShape2DRectangle(this Shape2D figure, double tolerance, out double lx, out double ly, out double radius, out Point3D center, out double rotation, out bool inverseVersus)
		{
			bool result = false;
			lx = double.MinValue;
			ly = double.MinValue;
			radius = 0;
			center = Point3D.NullPoint;
			rotation = 0;
			inverseVersus = false;
			Figure3D fourLines = new Figure3D();
			Figure3D figureClone = figure.GetFigure();
			figureClone.AutomaticSort(tolerance);
			figureClone.Reduce(true, true, tolerance);

			if (figureClone.Count > 0 && figureClone.IsClosedLoop(tolerance))
			{
				// Voglio il rettangolo percorso in senso orario
				if (figure.IsCounterClockWise())
				{
					inverseVersus = true;
					figureClone.SetInverse();
				}

				// Voglio che la prima curva sia una linea, per semplicità
				if (figureClone[0] is Line3D == false)
				{
					Curve3D curve = figureClone[0];
					figureClone.RemoveAt(0);
					figureClone.Add(curve);
					figureClone.AutomaticSort();
				}

				#region STRETTAMENTE RETTANGOLARE
				if (figureClone.Count == 4)
				{
					// Caso 4 linee
					result = true;
					foreach (Curve3D curve in figureClone)
					{
						if ((curve is Line3D) == false)
						{
							result = false;
							break;
						}
					}
					if (result)
					{
						fourLines.AddFigure(figureClone);
					}
				}
				else if (figureClone.Count == 8)
				{
					// Caso 4 linee e 4 archi a raccordare
					result = true;
					foreach (Curve3D curve in figureClone)
					{
						if (curve is Line3D)
						{
							fourLines.Add(curve);
						}
						else if (curve is Arc3D arc)
						{
							if (radius == 0)
							{
								radius = arc.Radius;
							}
							else if (radius.IsEquals(arc.Radius, tolerance) == false)
							{
								result = false;
								break;
							}
						}
						else
						{
							result = false;
							break;
						}
					}
					if (result)
					{
						if (fourLines.Count != 4)
							result = false;

						if ((figureClone[1] is Arc3D == false) || (figureClone[3] is Arc3D == false) || (figureClone[5] is Arc3D == false) || (figureClone[7] is Arc3D == false))
							result = false;
					}
					if (result)
					{
						// Ora verifico che gli archi siano tangenti alle linee
						if (figureClone[1].StartTangent.IsEquals(figureClone[0].EndTangent) == false ||
							figureClone[1].EndTangent.IsEquals(figureClone[2].StartTangent) == false ||
							figureClone[3].StartTangent.IsEquals(figureClone[2].EndTangent) == false ||
							figureClone[3].EndTangent.IsEquals(figureClone[4].StartTangent) == false ||
							figureClone[5].StartTangent.IsEquals(figureClone[4].EndTangent) == false ||
							figureClone[5].EndTangent.IsEquals(figureClone[6].StartTangent) == false ||
							figureClone[7].StartTangent.IsEquals(figureClone[6].EndTangent) == false ||
							figureClone[7].EndTangent.IsEquals(figureClone[0].StartTangent) == false)
						{
							result = false;
						}
					}
				}
				else
				{
					result = false;
				}

				if (result)
				{
					// Ora controllo che a due a due siano di uguale lunghezza
					result = false;
					double l0 = fourLines[0].Length;
					double l1 = fourLines[1].Length;
					double l2 = fourLines[2].Length;
					double l3 = fourLines[3].Length;
					if (l0.IsEquals(l2, tolerance) && l1.IsEquals(l3, tolerance))
					{
						// Ora controllo la perpendicolarità, tramite le diagonali
						// Vale sia con 4 curve che con 8
						double diag1 = (fourLines[0].StartPoint - fourLines[2].StartPoint).Length;
						double diag2 = (fourLines[0].EndPoint - fourLines[2].EndPoint).Length;
						if (diag1.IsEquals(diag2, tolerance) == true)
						{
							lx = l0 + 2 * radius;
							ly = l1 + 2 * radius;
							Point3D pM0 = fourLines[0].Evaluate(0.5);
							Point3D pM2 = fourLines[2].Evaluate(0.5);
							center = pM0 + 0.5 * (pM2 - pM0);
							rotation = (fourLines[0].StartPoint - fourLines[0].EndPoint).Angle();
							result = true;
						}
					}
				}
				#endregion STRETTAMENTE RETTANGOLARE

				#region TIPO ASOLA
				if (result == false)
				{
					Figure3D twoLines = new Figure3D();
					radius = 0;
					if (figureClone.Count == 4)
					{
						result = true;
						foreach (Curve3D curve in figureClone)
						{
							if (curve is Line3D)
							{
								twoLines.Add(curve);
							}
							else if (curve is Arc3D arc)
							{
								if (radius == 0)
								{
									radius = arc.Radius;
								}
								else if (radius.IsEquals(arc.Radius, tolerance) == false)
								{
									result = false;
									break;
								}
							}
							else
							{
								result = false;
								break;
							}
						}
						if (result)
						{
							if (twoLines.Count != 2)
								result = false;

							if ((figureClone[1] is Arc3D == false) || (figureClone[3] is Arc3D == false))
								result = false;
						}
						if (result)
						{
							// Ora verifico che gli archi siano tangenti alle linee
							if (figureClone[1].StartTangent.IsEquals(figureClone[0].EndTangent) == false ||
								figureClone[1].EndTangent.IsEquals(figureClone[2].StartTangent) == false ||
								figureClone[3].StartTangent.IsEquals(figureClone[2].EndTangent) == false ||
								figureClone[3].EndTangent.IsEquals(figureClone[0].StartTangent) == false)
							{
								result = false;
							}
						}
					}
					else
					{
						result = false;
					}

					if (result)
					{
						result = false;
						double l0 = twoLines[0].Length;
						double l1 = twoLines[1].Length;
						if (l0.IsEquals(l1, tolerance))
						{
							double diag1 = (twoLines[0].StartPoint - twoLines[1].StartPoint).Length;
							double diag2 = (twoLines[1].EndPoint - twoLines[0].EndPoint).Length;
							if (diag1.IsEquals(diag2, tolerance) == true)
							{
								lx = l0 + 2 * radius;
								ly = 2 * radius;
								Point3D pM0 = twoLines[0].Evaluate(0.5);
								Point3D pM1 = twoLines[1].Evaluate(0.5);
								center = pM0 + 0.5 * (pM1 - pM0);
								rotation = (twoLines[0].StartPoint - twoLines[0].EndPoint).Angle();
								double rotation2 = (twoLines[1].EndPoint - twoLines[1].StartPoint).Angle();
								if (rotation.IsEquals(rotation2, tolerance / 10))
									result = true;
							}
						}
					}
				}
				#endregion TIPO ASOLA
			}
			return result;
		}
	}
}
