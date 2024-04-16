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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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
        IShape _painter = null;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Single configuration
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

            // Tự tạo ra giao diện
            /*foreach (var item in _prototypes)
            {
                var control = new Button()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = 80,
                    Height = 35,
                    Content = item.Name,
                    Tag = item,
                };
                control.Click += Control_Click;
                Toolbar.Children.Add(control);
            }*/
            ShapeCombobox.ItemsSource = _prototypes;
            ShapeCombobox.SelectedIndex = 0;
            _painter = _prototypes[0];
        }

        private void Control_Click(object sender, RoutedEventArgs e)
        {
            IShape item = (IShape)(sender as Button)!.Tag;
            _painter = item;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = true;
            _start = e.GetPosition(WhiteBoard);
            _end = e.GetPosition(WhiteBoard);
            _painter.AddPoints(_start, _end);
            WhiteBoard.Children.Add(_painter.Convert(1, Brushes.Red));
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                _end = e.GetPosition(WhiteBoard);
                _painter.UpdateShape(_start, _end);
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = false;
            if (_start.X != _end.X && _start.Y != _end.Y)
            {
                _shapeList.Add((IShape)_painter.Clone());
            }
            //MessageBox.Show(_shapeList.Count.ToString());
        }

        private void ShapeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedIndex = ShapeCombobox.SelectedIndex;
            _painter = _prototypes[selectedIndex];
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