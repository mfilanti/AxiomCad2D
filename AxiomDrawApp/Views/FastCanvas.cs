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
		private Dictionary<string, GraphicElement> _visuals = new();

		/// <summary>
		/// Offset X generato dalla scala.
		/// </summary>
		private double _scaleOffsetX;

		/// <summary>
		/// Offset y generato dalla scala.
		/// </summary>
		private double _scaleOffsetY;

		/// <summary>
		/// Punto per lo zoom
		/// </summary>
		private Point _zoomPoint;

		/// <summary>
		/// Punto per eseguire il pan
		/// </summary>
		private Point _panStartPoint;
		#endregion

		#region Properties
		public WpfDocument Document
		{
			get { return (WpfDocument)GetValue(DocumentProperty); }
			set { SetValue(DocumentProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Document.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty DocumentProperty =
			DependencyProperty.Register(nameof(Document), typeof(WpfDocument), typeof(FastCanvas), new PropertyMetadata(null, DocumentChanged));

		private static void DocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is FastCanvas canvas)
			{
				canvas.DocumentChanged(e.OldValue as WpfDocument, e.NewValue as WpfDocument);
			}
		}

		/// <summary>
		/// Scala
		/// </summary>
		public double Scale
		{
			get { return (double)GetValue(ScaleProperty); }
			set { SetValue(ScaleProperty, value); }
		}

		/// <summary>
		/// Scala
		/// </summary>
		public static readonly DependencyProperty ScaleProperty =
			DependencyProperty.Register(nameof(Scale), typeof(double), typeof(FastCanvas), new PropertyMetadata(1.0, new PropertyChangedCallback(OnTransformChanged)));

		private static void OnTransformChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			if (sender is FastCanvas control)
				control.OnTransformChanged();
		}

		/// <summary>
		/// Spessore della penna
		/// </summary>
		public double PenThickness
		{
			get { return (double)GetValue(PenThicknessProperty); }
			set { SetValue(PenThicknessProperty, value); }
		}

		/// <summary>
		/// Spessore della penna
		/// </summary>
		public static readonly DependencyProperty PenThicknessProperty =
			DependencyProperty.Register(nameof(PenThickness), typeof(double), typeof(FastCanvas), new PropertyMetadata(1.0));

		/// <summary>
		/// Offset X
		/// </summary>
		public double OffsetX
		{
			get { return (double)GetValue(TraslateXProperty); }
			set { SetValue(TraslateXProperty, value); }
		}

		/// <summary>
		/// OffsetX
		/// </summary>
		public static readonly DependencyProperty TraslateXProperty =
			DependencyProperty.Register(nameof(OffsetX), typeof(double), typeof(FastCanvas), new PropertyMetadata(0.0, new PropertyChangedCallback(OnTransformChanged)));

		/// <summary>
		/// OffsetY
		/// </summary>
		public double OffsetY
		{
			get { return (double)GetValue(TraslateYProperty); }
			set { SetValue(TraslateYProperty, value); }
		}

		/// <summary>
		/// Offset Y
		/// </summary>
		public static readonly DependencyProperty TraslateYProperty =
			DependencyProperty.Register(nameof(OffsetY), typeof(double), typeof(FastCanvas), new PropertyMetadata(0.0, new PropertyChangedCallback(OnTransformChanged)));

		/// <summary>
		/// Rapporto per la scala
		/// </summary>
		public double ScaleRate
		{
			get { return (double)GetValue(ScaleRateProperty); }
			set { SetValue(ScaleRateProperty, value); }
		}

		/// <summary>
		///
		/// </summary>
		public static readonly DependencyProperty ScaleRateProperty =
			DependencyProperty.Register(nameof(ScaleRate), typeof(double), typeof(FastCanvas), new PropertyMetadata(1.1));



		public Brush Foreground
		{
			get { return (Brush)GetValue(ForegroundProperty); }
			set { SetValue(ForegroundProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Foreground.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ForegroundProperty =
			DependencyProperty.Register(nameof(Foreground), typeof(Brush), typeof(FastCanvas), new PropertyMetadata(new SolidColorBrush(Colors.White)));


		#endregion

		#region Constructors

		#endregion

		#region Methods
		#region Utility

		private bool IsNullPoint(Point panStartPoint)
		{
			return double.IsNaN(panStartPoint.X) || double.IsNaN(panStartPoint.Y);
		}

		private static Point NullPoint() => new Point(double.NaN, double.NaN);

		#endregion Utility
		/// <summary>
		///
		/// </summary>
		public override void BeginInit()
		{
			_panStartPoint = NullPoint();
			HorizontalAlignment = HorizontalAlignment.Center;
			VerticalAlignment = VerticalAlignment.Center;
			Background = new SolidColorBrush(Colors.Black);
			Foreground = new SolidColorBrush(Colors.White);
			base.BeginInit();

			Loaded += WpfCadUc_Loaded;
			Unloaded += WpfCadUc_Unloaded;
		}

		private void WpfCadUc_Unloaded(object sender, RoutedEventArgs e)
		{
			//
		}

		private void WpfCadUc_Loaded(object sender, RoutedEventArgs e)
		{
			if (Document != null)
			{
				DocumentChanged(null, Document);
			}
			OnTransformChanged();
			ScaleToFit();
		}
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
		private void OnTransformChanged()
		{
			PenThickness = 1;

			// Per permettere lo zoom sulla posizione del mouse è necessario partire dalla matrice esistente tenendo in considerazione
			// le traslazioni generate dalla ScaleAt che altrimenti andrebbero perse se si rigenera una nuova matrice
			Matrix previousMatrix = (RenderTransform as MatrixTransform).Matrix;
			if (previousMatrix.M22 > 0)
			{
				previousMatrix.Scale(1, -1);
			}
			if (previousMatrix.OffsetX != ActualWidth / 2.0 + OffsetX + _scaleOffsetX)
			{
				previousMatrix.Translate((ActualWidth / 2.0 + OffsetX + _scaleOffsetX) - previousMatrix.OffsetX, 0);
			}
			if (previousMatrix.OffsetY != ActualHeight / 2.0 + OffsetY + _scaleOffsetY)
			{
				previousMatrix.Translate(0, (ActualHeight / 2.0 + OffsetY + _scaleOffsetY) - previousMatrix.OffsetY);
			}
			if (previousMatrix.M11 != Scale)
			{
				double previousOffsetX = previousMatrix.OffsetX;
				double previousOffsetY = previousMatrix.OffsetY;
				previousMatrix.ScaleAt(Scale / previousMatrix.M11, Scale / previousMatrix.M11, _zoomPoint.X, _zoomPoint.Y);
				_scaleOffsetX += previousMatrix.OffsetX - previousOffsetX;
				_scaleOffsetY += previousMatrix.OffsetY - previousOffsetY;
			}
			RenderTransform = new MatrixTransform(previousMatrix);
		}

		#endregion
		#region Size Change

		/// <summary>
		///
		/// </summary>
		/// <param name="sizeInfo"></param>
		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);
			ScaleToFit();
		}

		#endregion Size Change

		#region Mouse Gesture

		/// <summary>
		///
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
		{
			base.OnPreviewMouseWheel(e);
			if (EnableZoomWheel)
			{
				Zoom(e.GetPosition(this), e.Delta > 0);
			}
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPreviewMouseMove(MouseEventArgs e)
		{
			base.OnPreviewMouseMove(e);
			InternalMouseMove(e);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseDown(e);
			InternalMouseDown(e);
		}

		private Point _startPoint = new Point(0, 0);

		/// <summary>
		///
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseUp(e);
			InternalMouseUp(e);
		}

		private void InternalMouseMove(MouseEventArgs e)
		{

			if (e.MiddleButton == MouseButtonState.Pressed && EnablePan)
			{
				if (!IsNullPoint(_panStartPoint))
				{
					Vector v = e.GetPosition(this) - _panStartPoint;
					OffsetX += v.X;
					OffsetY += v.Y;
					_panStartPoint = e.GetPosition(this);
				}
			}
			else
			{
				if (e.LeftButton == MouseButtonState.Pressed)
				{
					double dpiX = cadCanvas.RenderTransform.Value.M11;
					double dpiY = cadCanvas.RenderTransform.Value.M22;

					double vectX = 0;
					double vectY = 0;// point.Y - _startPoint.Y;
					if (!IsNullPoint(_startPoint))
					{
						Vector v = e.GetPosition(this) - _startPoint;
						vectX = v.X / dpiX;
						vectY = (v.Y / dpiY);
						if (Math.Abs(vectX) > 0.001 || Math.Abs(vectY) > 0.001)
						{
							_cadEditor?.Dragging(vectX, vectY);
							_startPoint = e.GetPosition(this);
						}
					}
				}
			}
		}

		/// <summary>
		/// Converts DIP (Device Independant Pixels) to millimeters.
		/// </summary>
		/// <param name="dip">A DIP value.</param>
		/// <returns>A millimeter value.</returns>
		public static int DipToMm(double dip)
		{
			return System.Convert.ToInt32(dip * 25.4 / 96.0);
		}

		private void InternalMouseUp(MouseButtonEventArgs e)
		{
			_panStartPoint = NullPoint();
			_startPoint = NullPoint();
		}

		private void InternalMouseDown(MouseButtonEventArgs e)
		{
			if (EnablePan)
			{
				_panStartPoint = e.GetPosition(this);
			}
			if (e.LeftButton == MouseButtonState.Pressed && _cadEditor != null)
			{
				_startPoint = e.GetPosition(this);
				_cadEditor.MouseLeftDown(e.OriginalSource as UIElement);
			}
		}

		#endregion Mouse Gesture

		#region Trasform
		private void ScaleToFit()
		{
			if (!IsLoaded || ActualWidth == 0 || ActualHeight == 0) return;

			//_zoomMatrix = Matrix.Identity;
			//_imageCenter = new Point(0, 0);
			//AABBox2D bbox = GetBBox();
			//if (bbox.LX > 0 && bbox.LY > 0)
			//{
			//	_imageCenter = bbox.Center;

			//	Scale = Math.Min(cadCanvas.ActualWidth / bbox.LX, cadCanvas.ActualHeight / bbox.LY) * 0.95;
			//}
			double minX = double.MaxValue;
			double minY = double.MaxValue;
			double maxX = double.MinValue;
			double maxY = double.MinValue;
			foreach (var entity in _visuals.Values)
			{
				entity.ToMinMax(out double minItemX, out double minItemY, out double maxItemX, out double maxItemY);
				minX = Math.Min(minX, minItemX);
				minY = Math.Min(minY, minItemY);
				maxX = Math.Max(maxX, maxItemX);
				maxY = Math.Max(maxY, maxItemY);

			}

			double lx = maxX - minX;
			double ly = maxY - minY;

			if (lx > 0 && ly > 0)
			{
				Scale = 1;
				_scaleOffsetX = _scaleOffsetY = 0;
				double actualWidth = ActualWidth;
				double actualHeight = ActualHeight;
				OffsetX = -minX - lx * 0.5;
				OffsetY = minY + ly * 0.5;

				double scaleFit = Math.Min(actualWidth / lx, actualHeight / ly) * 0.85;
				_zoomPoint = new Point(actualWidth / 2, actualHeight / 2);
				Scale = Math.Min(3.0, scaleFit);
			}
		}
		#endregion
		#region Document
		private void DocumentChanged(WpfDocument? oldDoc, WpfDocument? newDoc)
		{
			if (oldDoc != null)
			{
				oldDoc.OnChangedShape -= OldDoc_OnChangedShape;
				oldDoc.OnAddedShape -= OldDoc_OnAddedShape;
				oldDoc.OnRemovedShape -= OldDoc_OnRemovedShape;
			}
			_visuals.Clear();
			if (newDoc != null)
			{
				SetElements(newDoc, newDoc.Root);
				newDoc.OnChangedShape += OldDoc_OnChangedShape;
				newDoc.OnAddedShape += OldDoc_OnAddedShape;
				newDoc.OnRemovedShape += OldDoc_OnRemovedShape;
			}
		}

		private void OldDoc_OnRemovedShape(object? sender, Axiom.GeoShape.Entities.Entity3D e)
		{
		}

		private void OldDoc_OnAddedShape(object? sender, Axiom.GeoShape.Entities.Entity3D e)
		{
		}

		private void OldDoc_OnChangedShape(object? sender, Axiom.GeoShape.Entities.Entity3D e)
		{
		}
		#endregion

		#region Convert WpfDocument

		private void SetElements(WpfDocument document, Node3D node)
		{
			SetElements(document, node.Entities.Values);
			foreach (var item in node.Nodes)
			{
				SetElements(document, item.Value);
			}
			ScaleToFit();
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
