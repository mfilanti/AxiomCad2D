using Axiom.GeoMath;
using Axiom.GeoShape;
using Axiom.GeoShape.Curves;
using Axiom.GeoShape.Entities;
using Axiom.GeoShape.Shapes;
using AxiomDrawApp.Model;
using AxiomDrawApp.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Xps.Packaging;
using System.Xml.Linq;
using static System.Formats.Asn1.AsnWriter;

namespace AxiomDrawApp.Views
{
	/// <summary>
	/// Classe che estende Canvas per ottimizzare il rendering di più elementi grafici
	/// </summary>
	public class FastCanvas : Canvas
	{
		#region Fields


		/// <summary>
		/// Aggetti visualizzati
		/// </summary>
		private Dictionary<string, GraphicElement> _visuals = new();

		
		#endregion

		#region Methods

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dc"></param>
		protected override void OnRender(DrawingContext dc)
		{
			base.OnRender(dc);

			// Disegna tutti i DrawingVisual memorizzati
			foreach (var visual in _visuals.Values.Where(p => p.Visual != null).Select(p => p.Visual))
			{
				dc.DrawDrawing(visual.Drawing);  // Disegna il contenuto di ogni DrawingVisual
			}
		}

		/// <summary>
		/// Resetta la grafica
		/// </summary>
		public void Clear()
		{
			_visuals.Clear();
		}


		internal void MinMax(out double minX, out double minY, out double maxX, out double maxY)
		{
			minX = double.MaxValue;
			minY = double.MaxValue;
			maxX = double.MinValue;
			maxY = double.MinValue;
			foreach (var entity in _visuals.Values)
			{
				entity.ToMinMax(out double minItemX, out double minItemY, out double maxItemX, out double maxItemY);
				minX = Math.Min(minX, minItemX);
				minY = Math.Min(minY, minItemY);
				maxX = Math.Max(maxX, maxItemX);
				maxY = Math.Max(maxY, maxItemY);

			}
		}
		#endregion

		#region Convert WpfDocument

		public void SetElements(WpfDocument document, Node3D node)
		{
			SetElements(document, node.Entities.Values);
			foreach (var item in node.Nodes)
			{
				SetElements(document, item.Value);
			}
		}

		/// <summary>
		/// Visualizza gli elementi
		/// </summary>
		/// <param name="elements"></param>
		private void SetElements(WpfDocument document, IEnumerable<Entity3D> elements)
		{
			foreach (var element in elements)
			{
				try
				{
					var visual = new DrawingVisual();
					using (DrawingContext dc = visual.RenderOpen()) // Ottieni il DrawingContext
					{
						switch (element)
						{
							case Shape2D shape:
								CreateShape(document, dc, shape);

								break;

							//	CreateLine(dc, line);
							//case WpfArc arc:
							//	CreateArc(dc, arc);
							//	break;
							//case WpfText text:
							//	CreateText(dc, visual, text);
							//	break;
							//case WpfImage image:
							//	CreateImage(dc, image);
							//	break;
							//case WpfRectangle rectangle:
							//	CreateRectangle(dc, rectangle);
							//	break;

							default:
								break;
						}
					}

					_visuals.Add(element.PathId, new GraphicElement(element, visual));
				}
				catch
				{

				}
			}
			InvalidateVisual(); // Rende necessario un ridisegno della UI
		}

		private void CreateShape(WpfDocument document, DrawingContext dc, Shape2D shape)
		{
			Figure3D figure = shape.GetFigure();
			foreach (var curve in figure)
			{
				switch (curve)
				{
					case Line3D line:
						CreateLine(document, dc, line);
						break;
					case Arc3D arc:
						CreateArc(document, dc, arc);
						break;
					default:
						break;
				}
			}
		}

		//#region Rectangle Converter
		//private void CreateRectangle(DrawingContext dc, WpfRectangle rectangle)
		//{
		//	RectangleGeometry rectangleGeometry = new RectangleGeometry();
		//	rectangleGeometry.Rect = new Rect(new Point(rectangle.Start.X, rectangle.Start.Y), new Point(rectangle.End.X, rectangle.End.Y));
		//	dc.DrawGeometry(null, new Pen(ToBrush(rectangle.Color), rectangle.Thickness), rectangleGeometry);
		//}

		//#endregion

		//#region Image Converter
		//private void CreateImage(DrawingContext dc, WpfImage image)
		//{
		//	// Creazione della TransformGroup
		//	TransformGroup transformGroup = new TransformGroup();
		//	transformGroup.Children.Add(new MatrixTransform(new Matrix(1, 0, 0, -1, 0, 0)));  // Inverte l'asse Y
		//	transformGroup.Children.Add(new TranslateTransform(image.Position.X, image.Position.Y));  // Trasla l'immagine

		//	// Crea il BitmapSource (l'immagine)
		//	BitmapSource bitmapImage = ToBitmapImage(image.Image);
		//	if (bitmapImage == null && image.CanCreateEmptyImage)
		//	{
		//		bitmapImage = CreateEmptyImage((int)image.Width, (int)image.Height);
		//	}


		//	// Disegna l'immagine nel DrawingContext
		//	if (bitmapImage != null)
		//	{
		//		// Applica la trasformazione all'intero DrawingContext
		//		dc.PushTransform(transformGroup);
		//		// Usa DrawImage per disegnare l'immagine con la trasformazione
		//		dc.DrawImage(bitmapImage, new Rect(0, 0, image.Width, image.Height));
		//		// Rimuovi la trasformazione (pop) dopo il disegno
		//		dc.Pop();
		//	}
		//}

		///// <summary>
		///// Trasforma un byte array di immagini in una immagine-wpf
		///// </summary>
		///// <param name="self">byte array di una immagin</param>
		///// <returns>Immagine wpf.</returns>
		//private BitmapImage ToBitmapImage(byte[] self)
		//{
		//	if (self == null || self.Length == 0) return null;
		//	using (var ms = new System.IO.MemoryStream(self))
		//	{
		//		var image = new BitmapImage();
		//		image.BeginInit();
		//		image.CacheOption = BitmapCacheOption.OnLoad; // here
		//		image.StreamSource = ms;
		//		image.EndInit();
		//		return image;
		//	}
		//}

		//private BitmapSource CreateEmptyImage(int width, int height)
		//{
		//	System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(System.Convert.ToInt32(width), System.Convert.ToInt32(height));
		//	System.Drawing.Bitmap blankBmp = bmp;
		//	for (int x = 0; x < blankBmp.Width; x++)
		//		for (int y = 0; y < blankBmp.Height; y++)
		//			blankBmp.SetPixel(x, y, System.Drawing.Color.LightGray);
		//	System.IO.MemoryStream ms = new System.IO.MemoryStream();
		//	blankBmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
		//	ms.Position = 0;
		//	BitmapImage bi = new BitmapImage();
		//	bi.BeginInit();
		//	bi.StreamSource = ms;
		//	bi.EndInit();
		//	return bi;
		//}

		//#endregion

		//#region Text Converter

		//private void CreateText(DrawingContext dc, DrawingVisual visual, WpfText text)
		//{
		//	// Crea la trasformazione (Matrix, Rotate, Translate)
		//	TransformGroup transformGroup = new TransformGroup();
		//	transformGroup.Children.Add(new MatrixTransform(new Matrix(1, 0, 0, -1, 0, 0)));  // Inverte l'asse Y
		//	transformGroup.Children.Add(new RotateTransform(-text.DegRotation));
		//	transformGroup.Children.Add(new TranslateTransform(text.Position.X, text.Position.Y));

		//	// Crea il font per il testo
		//	FontFamily fontFamily = new FontFamily(text.FontFamilyName ?? "Arial");
		//	Typeface typeface = new Typeface(fontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
		//	Brush textBrush = ToBrush(text.Color);  // Converti il colore in un Brush

		//	// Crea il FormattedText per misurare e disegnare il testo
		//	FormattedText formattedText = new FormattedText(
		//		text.Text,
		//		CultureInfo.CurrentCulture,
		//		FlowDirection.LeftToRight,
		//		typeface,
		//		double.IsNaN(text.FontSize) ? 12 : text.FontSize,
		//		textBrush,
		//	new NumberSubstitution(),
		//		VisualTreeHelper.GetDpi(visual).PixelsPerDip);
		//	text.SetSize(formattedText.Width, formattedText.Height);

		//	// Applica la trasformazione al DrawingContext
		//	dc.PushTransform(transformGroup);

		//	// Disegna il testo nel DrawingContext
		//	dc.DrawText(formattedText, new Point(0, 0));

		//	// Rimuovi la trasformazione (pop)
		//	dc.Pop();
		//}
		//#endregion
		#region Arc Converter
		private void CreateArc(WpfDocument document, DrawingContext dc, Arc3D arc)
		{
			PathGeometry pathGeometry = CreateArcPath(arc);

			// Se la geometria non è nulla e ha almeno una figura
			if (pathGeometry != null && pathGeometry.Figures.Count > 0)
			{
				// Disegna la geometria nel DrawingContext
				dc.DrawGeometry(null, new Pen(ToBrush(document.Color), document.Thickness), pathGeometry);
			}
		}

		private PathGeometry CreateArcPath(Arc3D item)
		{
			PathGeometry pathGeometry = new PathGeometry();
			PathFigure figure = null;
			Point3D lastPoint = null;

			// Se la posizione dell'ultimo punto è diversa dalla posizione corrente, crea una nuova figura
			if (lastPoint == null || !lastPoint.IsEquals(item.StartPoint))
			{
				if (figure != null)
				{
					pathGeometry.Figures.Add(figure);
					figure = null;
				}
			}

			lastPoint = item.EndPoint;

			// Crea una nuova PathFigure se necessario
			if (figure == null)
			{
				figure = new PathFigure();
				figure.StartPoint = new Point(item.StartPoint.X, item.StartPoint.Y);
			}

			// Aggiungi il segmento arco
			figure.Segments.Add(SegmentArc(item));
			lastPoint = item.EndPoint;

			// Aggiungi la figura alla geometria
			if (figure != null)
			{
				pathGeometry.Figures.Add(figure);
				figure = null;
			}

			return pathGeometry;
		}

		private PathSegment SegmentArc(Arc3D arc)
		{
			double lx = arc.Radius;
			double ly = arc.Radius;
			double startAngle = MathUtils.RadToDegree(arc.StartAngle);
			double sweepAngle = MathUtils.RadToDegree(arc.SpanAngle);

			bool isLarge = sweepAngle > 180;
			SweepDirection sweepDirectionFlag = (!arc.CounterClockWise) ? SweepDirection.Counterclockwise : SweepDirection.Clockwise;

			return new ArcSegment(
				new Point(arc.EndPoint.X, arc.EndPoint.Y),
				new Size(lx, ly),
				sweepAngle,
				isLarge,
				sweepDirectionFlag,
				true);
		}
		#endregion

		#region Line Converter
		private void CreateLine(WpfDocument document, DrawingContext dc, Line3D line)
		{
			dc.DrawLine(new Pen(ToBrush(document.Color), document.Thickness), new Point(line.StartPoint.X, line.StartPoint.Y), new Point(line.EndPoint.X, line.EndPoint.Y));
		}

		private Brush ToBrush(string color)
		{
			Brush result = null;
			try
			{
				if (!string.IsNullOrEmpty(color))
				{
					var colorConverted = (Color)ColorConverter.ConvertFromString($"#FF{color}");
					result = new SolidColorBrush(colorConverted);
				}
			}
			catch
			{
				result = null;
			}

			if (result == null)
			{
				result = new SolidColorBrush(Colors.Black);
			}

			return result;
		}


        #endregion
        #endregion
    }
}
