using Shapes;
using System.IO;
using System.Reflection;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Paint_application
{
   public enum CursorMode
    {
        Draw, Select
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        bool _isDrawing = false;
        Point _start;
        Point _end;

        List<UIElement> _list = new List<UIElement>();
        List<IShape> _prototypes = new List<IShape>();
        List<IShape> _shapeList = new List<IShape>();
        UIElement _lastElement;

        SolidColorBrush _currentColor;
        IShape _painter = null;
        double _thickness;
        DoubleCollection _style;
        CursorMode mode = CursorMode.Draw;

        private void LoadColors()
        {
            var colors = typeof(Brushes).GetProperties()
                                         .Where(prop => prop.PropertyType == typeof(SolidColorBrush))
                                         .Select(prop => (SolidColorBrush)prop.GetValue(null))
                                         .ToList();

            ColorCombobox.ItemsSource = colors;

            foreach (SolidColorBrush color in ColorCombobox.Items)
            {
                if (color.Color == Colors.Black)
                {
                    ColorCombobox.SelectedItem = color;
                    break;
                }
            }
        }

        private void LoadThickness()
        {
            string[] thicknessList = ["3", "4", "5", "6", "7", "8", "9", "10"];
            ThicknessCombobox.ItemsSource = thicknessList;
            ThicknessCombobox.SelectedIndex = 0;
        }

        private void PainterClick(object sender, RoutedEventArgs e)
        {
            if (mode == CursorMode.Draw)
                return;

            if (e.Source is FrameworkElement element && element.Tag != null)
            {
                int index = Convert.ToInt32(element.Tag);
                MessageBox.Show(index.ToString());
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadColors();
            LoadThickness();
            StyleCombobox.SelectedIndex = 0;

            string folder = AppDomain.CurrentDomain.BaseDirectory;
            var fis = new DirectoryInfo(folder).GetFiles("*.dll");

            foreach (var fi in fis)
            {
                var assembly = Assembly.LoadFrom(fi.FullName);
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    if ((type.IsClass) && (typeof(IShape).IsAssignableFrom(type)))
                    {
                        _prototypes.Add((IShape)Activator.CreateInstance(type)!);
                    }
                }
            }

            ShapeCombobox.ItemsSource = _prototypes;
            ShapeCombobox.SelectedIndex = 0;
            _painter = _prototypes[0];
            CursorType.SelectedIndex = 0;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (mode == CursorMode.Select)
                return;

            _isDrawing = true;
            _start = e.GetPosition(WhiteBoard);
            _end = e.GetPosition(WhiteBoard);

            if ((Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down) > 0 || (Keyboard.GetKeyStates(Key.RightShift) & KeyStates.Down) > 0)
            {
                _painter.ShiftPressed = true;
            }
            else
            {
                _painter.ShiftPressed = false;
            }
                _painter.AddPoints(_start, _end);

            if (_painter.Name != "Star")
            {
                Shape _newPainter = (Shape) _painter.Convert(_style, _thickness, _currentColor);
                _newPainter.Tag = _shapeList.Count;
                _newPainter.MouseDown += PainterClick;
                WhiteBoard.Children.Add(_newPainter);
            }
            else
            {
                Polygon _newPainter = (Polygon)_painter.Convert(_style, _thickness, _currentColor);
                _newPainter.Tag = _shapeList.Count;
                _newPainter.MouseDown += PainterClick;
                WhiteBoard.Children.Add(_newPainter);
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                if (mode == CursorMode.Select)
                    return;

                if ((Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down) > 0 || (Keyboard.GetKeyStates(Key.RightShift) & KeyStates.Down) > 0)
                {
                    _painter.ShiftPressed = true;
                }
                else
                {
                    _painter.ShiftPressed = false;
                }

                _end = e.GetPosition(WhiteBoard);
                _painter.UpdateShape(_start, _end);
            }
        }
        
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (mode == CursorMode.Select)
                return;
            else
            {
                if (_start.X != _end.X && _start.Y != _end.Y)
                {
                    _shapeList.Add((IShape)_painter.Clone());
                }

                _isDrawing = false;
            }
        }

        private void ShapeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedIndex = ShapeCombobox.SelectedIndex;
            _painter = _prototypes[selectedIndex];
        }

        private void colorListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _currentColor = (SolidColorBrush)ColorCombobox.SelectedItem;
        }

        private void ThicknessCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _thickness = double.Parse(ThicknessCombobox.SelectedItem.ToString());
        }

        private void StyleCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StyleCombobox.SelectedIndex == 0)
            {
                _style = null;
            }
            else if (StyleCombobox.SelectedIndex == 1)
            {
                _style = new DoubleCollection() { 5, 2 };
            }
            else if (StyleCombobox.SelectedIndex == 2)
            {
                _style = new DoubleCollection() { 1, 1 };
            }
            else if (StyleCombobox.SelectedIndex == 3)
            {
                _style = new DoubleCollection() { 5, 2, 1, 2 };
            }

            else if (StyleCombobox.SelectedIndex == 4)
            {
                _style = new DoubleCollection() { 5, 2, 1, 2, 1, 2 };
            }
        }

        private void CursorType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CursorType.SelectedIndex == 0)
            {
                mode = CursorMode.Draw;
            }
            else
            {
                mode = CursorMode.Select;
            }
        }
    }
}

/*
 Polygon star = new Polygon();
            star.Stroke = Brushes.Black;
            star.StrokeThickness = 2;
            star.Fill = Brushes.Yellow; // Màu nền của ngôi sao

            // Tạo các điểm của ngôi sao
            PointCollection points = new PointCollection();
            double outerRadius = 100;
            double innerRadius = outerRadius / 2; // Độ lớn của ngôi sao

            // Tính toán các góc cho các đỉnh của ngôi sao
            double angleOffset = -Math.PI / 2; // Làm cho ngôi sao quay 90 độ
            for (int i = 0; i < 5; i++)
            {
                double angle = angleOffset + 2 * Math.PI * i / 5;
                double x = outerRadius * Math.Cos(angle) + outerRadius;
                double y = outerRadius * Math.Sin(angle) + outerRadius;
                points.Add(new Point(x, y));

                angle += Math.PI / 5;
                x = innerRadius * Math.Cos(angle) + outerRadius;
                y = innerRadius * Math.Sin(angle) + outerRadius;
                points.Add(new Point(x, y));
            }

            // Thiết lập các điểm của ngôi sao
            star.Points = points;
 */