using AxiomDrawApp.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AxiomDrawApp.Views
{
    /// <summary>
    /// Visualizzatore
    /// </summary>
    public class Viewer2D : Border
    {
		#region Fields
		private FastCanvas _fastCanvas;


		/// <summary>
		/// Gestore per la modifica delle entità
		/// </summary>
		private CadEditor _cadEditor = null;

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
			DependencyProperty.Register(nameof(Scale), typeof(double), typeof(Viewer2D), new PropertyMetadata(1.0, new PropertyChangedCallback(OnTransformChanged)));

		private static void OnTransformChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			if (sender is Viewer2D control)
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
			DependencyProperty.Register(nameof(PenThickness), typeof(double), typeof(Viewer2D), new PropertyMetadata(1.0));

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
			DependencyProperty.Register(nameof(OffsetX), typeof(double), typeof(Viewer2D), new PropertyMetadata(0.0, new PropertyChangedCallback(OnTransformChanged)));

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
			DependencyProperty.Register(nameof(OffsetY), typeof(double), typeof(Viewer2D), new PropertyMetadata(0.0, new PropertyChangedCallback(OnTransformChanged)));

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
			DependencyProperty.Register(nameof(ScaleRate), typeof(double), typeof(Viewer2D), new PropertyMetadata(1.1));



		public Brush Foreground
		{
			get { return (Brush)GetValue(ForegroundProperty); }
			set { SetValue(ForegroundProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Foreground.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ForegroundProperty =
			DependencyProperty.Register(nameof(Foreground), typeof(Brush), typeof(Viewer2D), new PropertyMetadata(new SolidColorBrush(Colors.White)));

		/// <summary>
		/// Abilita lo zoom con la rotella
		/// </summary>
		public bool EnableZoomWheel
		{
			get { return (bool)GetValue(EnableZoomWheelProperty); }
			set { SetValue(EnableZoomWheelProperty, value); }
		}

		/// <summary>
		/// Abilita lo zoom con la rotella
		/// </summary>
		public static readonly DependencyProperty EnableZoomWheelProperty =
			DependencyProperty.Register(nameof(EnableZoomWheel), typeof(bool), typeof(Viewer2D), new PropertyMetadata(true));
		/// <summary>
		/// Abilita il pan
		/// </summary>
		public bool EnablePan
		{
			get { return (bool)GetValue(EnablePanProperty); }
			set { SetValue(EnablePanProperty, value); }
		}

		/// <summary>
		/// Abilita il pan
		/// </summary>
		public static readonly DependencyProperty EnablePanProperty =
			DependencyProperty.Register(nameof(EnablePan), typeof(bool), typeof(Viewer2D), new PropertyMetadata(true));

		public WpfDocument Document
		{
			get { return (WpfDocument)GetValue(DocumentProperty); }
			set { SetValue(DocumentProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Document.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty DocumentProperty =
			DependencyProperty.Register(nameof(Document), typeof(WpfDocument), typeof(Viewer2D), new PropertyMetadata(null, DocumentChanged));

		private static void DocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is Viewer2D canvas)
			{
				canvas.DocumentChanged(e.OldValue as WpfDocument, e.NewValue as WpfDocument);
			}
		}

		#endregion
		public Viewer2D()
		{
			_fastCanvas = new FastCanvas();
			Child = null;          // se ti serve
			Child = _fastCanvas;        // nuovo contenuto
		}

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

		private void OnTransformChanged()
		{
			PenThickness = 1;

			// Per permettere lo zoom sulla posizione del mouse è necessario partire dalla matrice esistente tenendo in considerazione
			// le traslazioni generate dalla ScaleAt che altrimenti andrebbero perse se si rigenera una nuova matrice
			Matrix previousMatrix = (_fastCanvas.RenderTransform as MatrixTransform).Matrix;
			if (previousMatrix.M22 > 0)
			{
				previousMatrix.Scale(1, -1);
			}
			if (previousMatrix.OffsetX != _fastCanvas.ActualWidth / 2.0 + OffsetX + _scaleOffsetX)
			{
				previousMatrix.Translate((_fastCanvas.ActualWidth / 2.0 + OffsetX + _scaleOffsetX) - previousMatrix.OffsetX, 0);
			}
			if (previousMatrix.OffsetY != _fastCanvas.ActualHeight / 2.0 + OffsetY + _scaleOffsetY)
			{
				previousMatrix.Translate(0, (_fastCanvas.ActualHeight / 2.0 + OffsetY + _scaleOffsetY) - previousMatrix.OffsetY);
			}
			if (previousMatrix.M11 != Scale)
			{
				double previousOffsetX = previousMatrix.OffsetX;
				double previousOffsetY = previousMatrix.OffsetY;
				previousMatrix.ScaleAt(Scale / previousMatrix.M11, Scale / previousMatrix.M11, _zoomPoint.X, _zoomPoint.Y);
				_scaleOffsetX += previousMatrix.OffsetX - previousOffsetX;
				_scaleOffsetY += previousMatrix.OffsetY - previousOffsetY;
			}
			_fastCanvas.RenderTransform = new MatrixTransform(previousMatrix);
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
		/// <param name="sizeInfo"></param>
		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);
			ScaleToFit();
		}

		#region Document
		private void DocumentChanged(WpfDocument? oldDoc, WpfDocument? newDoc)
		{
			if (oldDoc != null)
			{
				oldDoc.OnChangedShape -= OldDoc_OnChangedShape;
				oldDoc.OnAddedShape -= OldDoc_OnAddedShape;
				oldDoc.OnRemovedShape -= OldDoc_OnRemovedShape;
			}
			_fastCanvas.Clear();
			if (newDoc != null)
			{
				_fastCanvas.SetElements(newDoc, newDoc.Root);
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
		#region Trasform

		/// <summary>
		/// Esegue lo zoom sul punto specificato.
		/// </summary>
		/// <param name="zoomPoint">Punto su cui fare lo zoom.</param>
		/// <param name="isZoomIn">Indica se effettuare uno zoom in, altrimenti viene effettuato uno zoom out.</param>
		private void Zoom(Point zoomPoint, bool isZoomIn)
		{
			_zoomPoint = zoomPoint;
			Scale = isZoomIn ? Scale * ScaleRate : Scale / ScaleRate;
		}

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
			_fastCanvas.MinMax(out minX,out minY,out maxX,out maxY);
			

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
					double dpiX = RenderTransform.Value.M11;
					double dpiY = RenderTransform.Value.M22;

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


		#region Utility

		private bool IsNullPoint(Point panStartPoint)
		{
			return double.IsNaN(panStartPoint.X) || double.IsNaN(panStartPoint.Y);
		}

		private static Point NullPoint() => new Point(double.NaN, double.NaN);

		#endregion Utility
	}
}
