using AxiomDrawApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace AxiomDrawApp.Views
{
	/// <summary>
	/// Direzione delle frecce
	/// </summary>
	public enum ArrowDirection
	{
		/// <summary>
		/// Freccia alta
		/// </summary>
		Top,

		/// <summary>
		/// Sinistra
		/// </summary>
		Left,

		/// <summary>
		/// Sotto
		/// </summary>
		Bottom,

		/// <summary>
		/// Destra
		/// </summary>
		Right,

		/// <summary>
		/// Alta-Sinistra
		/// </summary>
		TopLeft,

		/// <summary>
		/// Alta-destra
		/// </summary>
		TopRight,

		/// <summary>
		/// Bassa-sinistra
		/// </summary>
		BottomLeft,

		/// <summary>
		/// Bassa-destra
		/// </summary>
		BottomRight
	};

	internal class ResizingAdorner : Adorner
	{
		#region Fields
		private Thumb _topLeft, _topRight, _bottomLeft, _bottomRight;

		private Thumb _midLeft, _midRight; //linee orizzontali
		private Thumb _midTop, _midDown;   //linee verticali

		private Thumb _startPoint;

		// To store and manage the adorner's visual children.
		private VisualCollection _visualChildren;

		/// <summary>
		/// Precisione
		/// </summary>
		private double _precision = 0.001;

		/// <summary>
		/// Dimensione dell'adorner
		/// </summary>
		private double _adornerSize = 4.0;

		private Border _border;

		/// <summary>
		/// stile di default del Thumb
		/// </summary>
		private Style _defaultStyleThumb;

		/// <summary>
		/// Spessore del bordo
		/// </summary>
		private double _borderThickness = 1.0;
		private double _offsetX = 0;
		private double _offsetY = 0;
		private Point _startPointPosition = new(0, 0);
		#endregion

		#region Properties
		/// <summary>
		/// Evento sollevato quando si effettua il resize.
		/// </summary>
		public event EventHandler<GraphicElement> ResizingAdornerEvent;

		/// <summary>
		/// Evento sollevato quando il resize è terminato.
		/// </summary>
		public event EventHandler<GraphicElement> ResizedAdornerEvent;

		/// <summary>
		/// Elemento selezionato
		/// </summary>
		public UIElement SelectedElement { get; set; } = null;

		#endregion

		#region Constructors
		/// <summary>
		/// Initialize the ResizingAdorner.
		/// </summary>
		public ResizingAdorner(UIElement adornedElement, Style thumb, Brush foreground, Brush selectedForeground)
			: base(adornedElement)
		{
			SelectedElement = adornedElement;
			_defaultStyleThumb = thumb;
			_visualChildren = new VisualCollection(this);
			Init(adornedElement, foreground, selectedForeground);
		}
		#endregion

		#region Override Methods
		private void Init(UIElement adornedElement, Brush foreground, Brush selectedForeground)
		{
			GraphicElement tag = (SelectedElement as FrameworkElement).Tag as GraphicElement;
			bool createTopLeft = true;
			bool createTopRight = true;
			bool createBottomLeft = true;
			bool createBottomRight = true;
			bool createMidTop = true;
			bool createMidBottom = true;
			bool createMidLeft = true;
			bool createMidRight = true;
			if (tag != null)
			{
				_startPointPosition = new Point(tag.Position.X, tag.Position.Y);

				if (tag.Width < _precision)
				{
					createTopLeft = createTopRight = createMidLeft = createMidRight = createBottomLeft = createBottomRight = false;
				}
				if (tag.Height < _precision)
				{
					createTopLeft = createTopRight = createMidTop = createMidBottom = createBottomLeft = createBottomRight = false;
				}
				//if (tag is WpfImage)
				//{// Nell'immagine solo un angolo è movibile
				//	createTopLeft = createTopRight = createMidTop = createMidBottom = createMidLeft = createMidRight = createBottomLeft = createBottomRight = false;
				//	createMidRight= true;
				//}
				if (!tag.IsResizable)
				{// non è ridimensionabile
					createTopLeft = createTopRight = createMidTop = createMidBottom = createMidLeft = createMidRight = createBottomLeft = createBottomRight = false;
				}
			}
			else
			{
				createMidBottom = createMidLeft = createMidRight = createMidTop = false;
			}

			if (createTopLeft)
			{
				BuildAdornerCorner(ref _topLeft, Cursors.SizeNWSE, foreground);
				_topLeft.DragDelta += new DragDeltaEventHandler(HandleTopLeft);
			}
			if (createTopRight)
			{
				BuildAdornerCorner(ref _topRight, Cursors.SizeNESW, foreground);
				_topRight.DragDelta += new DragDeltaEventHandler(HandleTopRight);

			}
			if (createBottomLeft)
			{
				BuildAdornerCorner(ref _bottomLeft, Cursors.SizeNESW, foreground);
				_bottomLeft.DragDelta += new DragDeltaEventHandler(HandleBottomLeft);
			}
			if (createBottomRight)
			{
				BuildAdornerCorner(ref _bottomRight, Cursors.SizeNWSE, foreground);
				_bottomRight.DragDelta += new DragDeltaEventHandler(HandleBottomRight);
			}

			if (createMidTop)
			{
				BuildAdornerCorner(ref _midTop, Cursors.SizeNS, foreground);
				_midTop.DragDelta += new DragDeltaEventHandler(HandleMidTop);
			}
			if (createMidBottom)
			{
				BuildAdornerCorner(ref _midDown, Cursors.SizeNS, foreground);
				_midDown.DragDelta += new DragDeltaEventHandler(HandleMidDown);
			}
			if (createMidLeft)
			{
				BuildAdornerCorner(ref _midLeft, Cursors.SizeWE, foreground);
				_midLeft.DragDelta += new DragDeltaEventHandler(HandleMidLeft);
			}
			if (createMidRight)
			{
				BuildAdornerCorner(ref _midRight, Cursors.SizeWE, foreground);
				_midRight.DragDelta += new DragDeltaEventHandler(HandleMidRight);
			}

			_border = new Border() { BorderThickness = new Thickness(_borderThickness), BorderBrush = selectedForeground };
			_visualChildren.Add(_border);

			_startPoint = new Thumb();
			// Set some arbitrary visual characteristics.
			_startPoint.Cursor = Cursors.Cross;
			_startPoint.Height = _startPoint.Width = (_adornerSize * 4);
			_startPoint.Foreground = new SolidColorBrush(Colors.Red);
			// solo per debug _visualChildren.Add(_startPoint);

		}

		private void MouseDownInternal(object sender, MouseButtonEventArgs e)
		{
			ResizingAdornerEvent?.Invoke(this, null);
		}

		private void MouseUpInternal(object sender, MouseButtonEventArgs e)
		{
			GraphicElement tag = (SelectedElement as FrameworkElement).Tag as GraphicElement;
			ResizedAdornerEvent?.Invoke(this, tag);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="constraint"></param>
		/// <returns></returns>
		protected override Size MeasureOverride(Size constraint)
		{
			_border.Measure(constraint);
			return base.MeasureOverride(constraint);
		}

		/// <summary>
		/// Arrange the Adorners.
		/// </summary>
		/// <param name="finalSize"></param>
		/// <returns></returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			UpdateSize();

			return finalSize;
		}

		/// <summary>
		/// 
		/// </summary>
		protected override int VisualChildrenCount { get { return _visualChildren.Count; } }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		protected override Visual GetVisualChild(int index) { return _visualChildren[index]; }
		#endregion

		#region Methods

		private void UpdateSize()
		{
			GraphicElement tag = (SelectedElement as FrameworkElement).Tag as GraphicElement;
			double desiredWidth = SelectedElement.DesiredSize.Width;
			double desiredHeight = SelectedElement.DesiredSize.Height;
			double halfThickness = _borderThickness / 2.0;
			if (tag != null)
			{
				if (!double.IsNaN(tag.Width))
				{
					desiredWidth = tag.Width;
				}
				if (!double.IsNaN(tag.Height))
				{
					desiredHeight = tag.Height;
				}
				Point position = new Point(tag.Position.X, tag.Position.Y);
				if (SelectedElement.RenderTransform == Transform.Identity)
				{
					_offsetX = tag.Position.X;
					_offsetY = tag.Position.Y - tag.Height;
				}

				double halfSizeAdorner = _adornerSize / 2.0;
				_startPoint.Arrange(new Rect(position.X - halfSizeAdorner, position.Y - halfSizeAdorner, _adornerSize, _adornerSize));
				if (_midLeft != null)
				{
					_midLeft.Arrange(new Rect(position.X - halfSizeAdorner + halfThickness, (position.Y - desiredHeight / 2.0) - halfSizeAdorner, _adornerSize, _adornerSize));
				}
				if (_midRight != null)
				{
					_midRight.Arrange(new Rect(position.X + desiredWidth - halfSizeAdorner - halfThickness, (position.Y - desiredHeight / 2.0) - halfSizeAdorner, _adornerSize, _adornerSize));
				}
				if (_midTop != null)
				{
					_midTop.Arrange(new Rect(position.X + desiredWidth / 2.0 - halfSizeAdorner, position.Y - halfSizeAdorner + halfThickness, _adornerSize, _adornerSize));
				}
				if (_midDown != null)
				{
					_midDown.Arrange(new Rect(position.X + desiredWidth / 2.0 - halfSizeAdorner, (position.Y - desiredHeight) - halfSizeAdorner + halfThickness, _adornerSize, _adornerSize));
				}
				if (_topLeft != null)
				{
					_topLeft.Arrange(new Rect(position.X - halfSizeAdorner + halfThickness, position.Y - halfSizeAdorner + halfThickness, _adornerSize, _adornerSize));
				}
				if (_bottomLeft != null)
				{
					_bottomLeft.Arrange(new Rect(position.X - halfSizeAdorner + halfThickness, (position.Y - desiredHeight) - halfSizeAdorner + halfThickness, _adornerSize, _adornerSize));
				}
				if (_topRight != null)
				{
					_topRight.Arrange(new Rect(position.X + desiredWidth - halfSizeAdorner - halfThickness, position.Y - halfSizeAdorner + halfThickness, _adornerSize, _adornerSize));
				}
				if (_bottomRight != null)
				{
					_bottomRight.Arrange(new Rect(position.X + desiredWidth - halfSizeAdorner - halfThickness, (position.Y - desiredHeight) - halfSizeAdorner + halfThickness, _adornerSize, _adornerSize));
				}
				_border.Arrange(new Rect(_offsetX, _offsetY, desiredWidth, desiredHeight));
				_border.RenderTransform = SelectedElement.RenderTransform.Clone();
			}
		}

		// Helper method to instantiate the corner Thumbs, set the Cursor property, 
		// set some appearance properties, and add the elements to the visual tree.
		private void BuildAdornerCorner(ref Thumb cornerThumb, Cursor customizedCursor, Brush color = null)
		{
			if (cornerThumb != null) return;

			cornerThumb = new Thumb();
			// Set some arbitrary visual characteristics.
			cornerThumb.Cursor = customizedCursor;
			cornerThumb.Height = cornerThumb.Width = _adornerSize;
			cornerThumb.Style = _defaultStyleThumb;
			if (color != null)
			{
				cornerThumb.Foreground = color;
			}
			else
			{
				cornerThumb.Foreground = new SolidColorBrush(Colors.AliceBlue);
			}
			cornerThumb.PreviewMouseUp += MouseUpInternal;
			cornerThumb.PreviewMouseDown += MouseDownInternal;
			_visualChildren.Add(cornerThumb);
		}

		// This method ensures that the Widths and Heights are initialized.  Sizing to content produces
		// Width and Height values of Double.NaN.  Because this Adorner explicitly resizes, the Width and Height
		// need to be set first.  It also sets the maximum size of the adorned element.
		private void EnforceSize(FrameworkElement adornedElement)
		{
			if (adornedElement.Width.Equals(Double.NaN))
				adornedElement.Width = adornedElement.DesiredSize.Width;
			if (adornedElement.Height.Equals(Double.NaN))
				adornedElement.Height = adornedElement.DesiredSize.Height;
			FrameworkElement parent = adornedElement.Parent as FrameworkElement;
			if (parent != null)
			{
				adornedElement.MaxHeight = parent.ActualHeight;
				adornedElement.MaxWidth = parent.ActualWidth;

				if (adornedElement is System.Windows.Shapes.Line)
				{
					//se linea orizzontale
					if ((adornedElement as System.Windows.Shapes.Line).Y1 == (adornedElement as System.Windows.Shapes.Line).Y2)
					{
						adornedElement.MaxHeight = parent.ActualHeight;
						adornedElement.MaxWidth = parent.ActualWidth + (adornedElement as System.Windows.Shapes.Line).StrokeThickness;
					}
					//se linea verticale
					if ((adornedElement as System.Windows.Shapes.Line).X1 == (adornedElement as System.Windows.Shapes.Line).X2)
					{
						adornedElement.MaxHeight = parent.ActualHeight + (adornedElement as System.Windows.Shapes.Line).StrokeThickness;
						adornedElement.MaxWidth = parent.ActualWidth;
					}
				}

			}
		}


		#endregion

		#region Handle Resize Methods

		private void HandleMidDown(object sender, DragDeltaEventArgs args)
		{
			HandleMove(ArrowDirection.Bottom, sender, args);
		}

		// Handler for resizing from the bottom-right.
		private void HandleBottomRight(object sender, DragDeltaEventArgs args)
		{
			HandleMove(ArrowDirection.BottomRight, sender, args);
		}

		private void HandleMidRight(object sender, DragDeltaEventArgs args)
		{
			HandleMove(ArrowDirection.Right, sender, args);
		}

		private void HandleMidTop(object sender, DragDeltaEventArgs args)
		{
			HandleMove(ArrowDirection.Top, sender, args);
		}

		private void HandleTopRight(object sender, DragDeltaEventArgs args)
		{
			HandleMove(ArrowDirection.TopRight, sender, args);
		}

		private void HandleMidLeft(object sender, DragDeltaEventArgs args)
		{
			HandleMove(ArrowDirection.Left, sender, args);
		}

		private void HandleTopLeft(object sender, DragDeltaEventArgs args)
		{
			HandleMove(ArrowDirection.TopLeft, sender, args);
		}

		private void HandleBottomLeft(object sender, DragDeltaEventArgs args)
		{
			HandleMove(ArrowDirection.BottomLeft, sender, args);
		}

		private void HandleMove(ArrowDirection arrowDirection, object sender, DragDeltaEventArgs args)
		{
			FrameworkElement adornedElement = this.AdornedElement as FrameworkElement;
			Thumb hitThumb = sender as Thumb;

			if (adornedElement == null || hitThumb == null) return;
			EnforceSize(adornedElement);

			// Change the size by the amount the user drags the mouse, as long as it's larger 
			// than the width or height of an adorner, respectively.
			GraphicElement tag = (SelectedElement as FrameworkElement).Tag as GraphicElement;
			if (tag != null)
			{
				tag.ChangeSize(arrowDirection, args.VerticalChange, args.HorizontalChange);
				_offsetX = tag.Position.X - _startPointPosition.X; _offsetY = tag.Position.Y - _startPointPosition.Y;

				UpdateSize();
				ResizingAdornerEvent?.Invoke(this, tag);
			}
		}
		#endregion
	}
}
