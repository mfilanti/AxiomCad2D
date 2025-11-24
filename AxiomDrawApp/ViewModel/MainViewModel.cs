using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxiomDrawApp.ViewModel
{
	public class MainViewModel : ObservableObject
	{
		#region Fields

		#endregion

		#region Properties

		/// <summary>
		/// Titolo
		/// </summary>
		private string _title;
		/// <summary>
		/// Titolo
		/// </summary>
		public string Title
		{
			get => _title;
			set => SetProperty(ref _title, value);
		}

		#endregion

		#region Constructors
		/// <summary>
		/// Ctor
		/// </summary>
		public MainViewModel()
		{
			Title = "AxiomDrawApp";
		}
		#endregion

		#region Methods

		#endregion

	}
}
