using AxiomDrawApp.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace AxiomDrawApp.Views
{
	/// <summary>
	/// Gestisce la modifica degli elementi cad
	/// </summary>
	internal class CadEditor
	{
		#region Fields

		/// <summary>
		/// Wrapper dell'adorner
		/// </summary>
		private ResizingAdorner _resizingAdorner;

		/// <summary>
		/// Elemento selezionato
		/// </summary>
		private FrameworkElement _selectedElement = null;

		/// <summary>
		/// Sono in selezione
		/// </summary>
		private bool _resizing = false;

		/// <summary>
		/// Sono in modalità spostamento
		/// </summary>
		private bool _isDragging = false;

		/// <summary>
		/// Elementi visualizzati
		/// </summary>
		private ObservableCollection<UIElement> _elements = new ObservableCollection<UIElement>();

		/// <summary>
		/// COlore di selezione
		/// </summary>
		public Brush SelectedForeground { get; set; } = new SolidColorBrush(Colors.Red);

		/// <summary>
		/// Colore
		/// </summary>
		public Brush Foreground { get; set; } = new SolidColorBrush(Colors.Black);

		/// <summary>
		/// Stile per il resize
		/// </summary>
		public Style ThumbStyle { get; set; } = null;

		#endregion Fields

		#region Properties

		/// <summary>
		/// Evento sollevato quando si effettua il resize.
		/// </summary>
		public event EventHandler<GraphicElement> WpfEntityIsChangedEvent;

		/// <summary>
		/// Evento di selezione dell'entità
		/// </summary>
		public event EventHandler<GraphicElement> WpfEntityIsSelectedEvent;

		/// <summary>
		/// Evento di modifica della posizione di una entità
		/// </summary>
		public event EventHandler<GraphicElement> WpfEntityIsChangedPositionEvent;

		#endregion Properties

		#region Constructors

		/// <summary>
		/// Init
		/// </summary>
		/// <param name="elements">Lista degli elementi visibili in grafica</param>
		public CadEditor(ObservableCollection<UIElement> elements)
		{
			_elements = elements;
		}

		#endregion Constructors

		#region Methods

		/// <summary>
		/// Rimuove la selezione
		/// </summary>
		public void RemoveSelection()
		{
			if (_resizingAdorner != null)
			{
				try
				{
					_resizingAdorner.ResizingAdornerEvent -= Adorner_ResizingAdornerEvent;
					_resizingAdorner.ResizedAdornerEvent -= Adorner_ResizedAdornerEvent;
					_elements.Remove(_resizingAdorner);
				}
				finally
				{
					_selectedElement = null;
					_resizingAdorner = null;
				}
			}
			_resizing = false;
			_isDragging = false;
		}

		/// <summary>
		/// Attiva la selezione su un nuovo elemento
		/// </summary>
		/// <param name="baseElement">nuovo elemento</param>
		/// <param name="enableEventSelection">Indica se abilitare l'evento di selezione</param>
		private void ActiveSelection(FrameworkElement baseElement, bool enableEventSelection)
		{
			if (baseElement != null)
			{
				_selectedElement = baseElement;
				//_aLayer = System.Windows.Documents.AdornerLayer.GetAdornerLayer(_selectedElement);
				//if (_aLayer != null)
				{
					_resizingAdorner = new ResizingAdorner(_selectedElement, ThumbStyle, Foreground, SelectedForeground);
					_resizingAdorner.ResizingAdornerEvent += Adorner_ResizingAdornerEvent;
					_resizingAdorner.ResizedAdornerEvent += Adorner_ResizedAdornerEvent;
					_elements.Add(_resizingAdorner);
				}
				//else
				//{
				//	_selectedElement = null;
				//}
			}

			if (enableEventSelection)
			{
				GraphicElement wpfEntity = _selectedElement?.Tag as GraphicElement;
				WpfEntityIsSelectedEvent?.Invoke(this, wpfEntity);
			}
		}

		private void Adorner_ResizedAdornerEvent(object sender, GraphicElement wpfEntity)
		{
			if (wpfEntity != null && _selectedElement.Tag == wpfEntity)
			{
				WpfEntityIsChangedEvent?.Invoke(this, wpfEntity);
			}
			_resizing = false;
		}

		private void Adorner_ResizingAdornerEvent(object sender, GraphicElement wpfEntity)
		{
			_resizing = true;
		}

		#endregion Methods

		/// <summary>
		///  L'elemento è stato selezionato dal di fuori
		/// </summary>
		public void ElementIsSelected(UIElement source)
		{
			RemoveSelection();
			ActiveSelection(source as FrameworkElement, false);
		}

		/// <summary>
		///  Mouse down
		/// </summary>
		public void MouseLeftDown(UIElement source)
		{
			if (source is FrameworkElement element && element.Tag is GraphicElement)
			{
				RemoveSelection();
				ActiveSelection(element, true);
			}
			else if (source.GetType() == typeof(FastCanvas))
			{
				RemoveSelection();
			}
		}

		/// <summary>
		/// Mouse move
		/// </summary>
		/// <param name="vectX"></param>
		/// <param name="vectY"></param>
		public void Dragging(double vectX, double vectY)
		{
			if (!_isDragging && !_resizing)
			{
				_isDragging = true;
				try
				{
					if (_selectedElement != null)
					{
						if (_selectedElement.Tag is GraphicElement wpfEntity)
						{
							wpfEntity.Traslate(vectX, vectY);
							WpfEntityIsChangedPositionEvent?.Invoke(this, wpfEntity);
						}
					}
				}
				finally { _isDragging = false; }
			}
		}

		internal void Clear()
		{
			RemoveSelection();
		}
	}
}
