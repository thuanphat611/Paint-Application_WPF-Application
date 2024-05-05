using Shapes;
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

namespace Paint_application
{
    /// <summary>
    /// Interaction logic for AddTextWindow.xaml
    /// </summary>
    public partial class AddTextWindow : Window
    {
        Canvas WhiteBoard;
        IShape painter;

        String font;
        SolidColorBrush background;
        SolidColorBrush foreground;
        double size;

        public AddTextWindow(Canvas Whiteboard, IShape painter)
        {
            this.WhiteBoard = Whiteboard;
            this.painter = painter;
            InitializeComponent();
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            if (TextInput.Text.Length > 0)
            {
                if (painter.GetText() == null)
                {
                    painter.SetText(font, background, foreground, size, TextInput.Text);
                    WhiteBoard.Children.Add(painter.RecreateText());
                }
                else
                {
                    painter.EditText(font, background, foreground, size, TextInput.Text);
                }
            }
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var colors = typeof(Brushes).GetProperties()
                                        .Where(prop => prop.PropertyType == typeof(SolidColorBrush))
                                        .Select(prop => (SolidColorBrush)prop.GetValue(null))
                                        .ToList();

            ColorCombobox.ItemsSource = colors;
            FillCombobox.ItemsSource = colors;

            foreach (SolidColorBrush color in ColorCombobox.Items)
            {
                if (color.Color == Colors.Black)
                {
                    ColorCombobox.SelectedItem = color;
                    break;
                }
            }

            foreach (SolidColorBrush color in FillCombobox.Items)
            {
                if (color.Color == Colors.White)
                {
                    FillCombobox.SelectedItem = color;
                    break;
                }
            }

            foreach (FontFamily fontFamily in Fonts.SystemFontFamilies)
            {
                FontCombobox.Items.Add(fontFamily.Source);
            }
            FontCombobox.SelectedIndex = 0;

            string[] thicknessList = ["12", "13", "14", "15", "16", "17", "18", "19", "20"];
            SizeCombobox.ItemsSource = thicknessList;
            SizeCombobox.SelectedIndex = 3;
        }

        private void FontCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            font = (string)FontCombobox.SelectedItem;
        }

        private void FillCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            background = (SolidColorBrush)FillCombobox.SelectedItem;
        }

        private void ColorCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreground = (SolidColorBrush)ColorCombobox.SelectedItem;
        }

        private void SizeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            size = Double.Parse((string) SizeCombobox.SelectedItem);
        }
    }
}
