using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using MahApps.Metro.Controls;

using UltimateUtil;

namespace MetaMusic
{
	/// <summary>
	/// Interaction logic for AddPlaylistWindow.xaml
	/// </summary>
	public partial class GetTextWindow : MetroWindow
	{
		public string ResultText => ResultTextBox.Text;

		public string Description
		{ get; private set; }

		public readonly Predicate<string> IsValidText;

		public GetTextWindow(string title, string description, Predicate<string> isValid)
		{
			InitializeComponent();

			Title = title;
			Description = description;
			IsValidText = isValid;
		}

		public GetTextWindow(string title, string description) : this(title, description, s => true)
		{ }

		private void OKBtn_OnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				DialogResult = true;
			}
			catch (InvalidOperationException)
			{ }

			Close();
		}

		private void CancelBtn_OnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				DialogResult = false;
			}
			catch (InvalidOperationException)
			{ }

			Close();
		}

		private void NameBox_OnTextChanged(object sender, TextChangedEventArgs e)
		{
			try
			{
				OKBtn.IsEnabled = !ResultText.IsNullOrEmpty() && IsValidText(ResultText);
			}
			catch (NullReferenceException) // Window not yet loaded
			{ }
		}
	}
}
