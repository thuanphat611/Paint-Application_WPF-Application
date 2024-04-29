﻿using Shapes;
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

        double WHITEBOARD_WIDTH = 1350;
        double WHITEBOARD_HEIGHT = 620;

        bool _isDrawing = false;
        bool _isDragAndDrop = false;
        bool _isResizing = false;
        Point _start;
        Point _end;
        Point _selectedStart;
        Point _selectedEnd;

        List<UIElement> _list = new List<UIElement>();
        List<IShape> _prototypes = new List<IShape>();
        List<IShape> _shapeList = new List<IShape>();
        UIElement _lastElement;

        SolidColorBrush _currentColor;
        IShape _painter = null;
        double _thickness;
        DoubleCollection _style;
        CursorMode mode = CursorMode.Draw;
        IShape _selectedPainter = null;

        Shape _areaSelector;
        Point _areaSelectorPoint1;
        Point _areaSelectorPoint2;
        bool _foundShape = false;
        Rectangle _resizeSquare;
        List<IShape> _clipboard = new List<IShape>();

        private bool IsInsideArea(Point _topLeft, Point _rightBottom, Point pointToCheck) {
            if (pointToCheck.X >= _topLeft.X && pointToCheck.Y >= _topLeft.Y && pointToCheck.X <= _rightBottom.X && pointToCheck.Y <= _rightBottom.Y)
                return true;

            return false;
        }

        private void CopyToClipboard()
        {
            Point _areaTopLeft = new Point(_areaSelectorPoint1.X < _areaSelectorPoint2.X ? _areaSelectorPoint1.X : _areaSelectorPoint2.X, _areaSelectorPoint1.Y < _areaSelectorPoint2.Y ? _areaSelectorPoint1.Y : _areaSelectorPoint2.Y);
            Point _areaRightBottom = new Point(_areaSelectorPoint1.X > _areaSelectorPoint2.X ? _areaSelectorPoint1.X : _areaSelectorPoint2.X, _areaSelectorPoint1.Y > _areaSelectorPoint2.Y ? _areaSelectorPoint1.Y : _areaSelectorPoint2.Y);

            _clipboard.Clear();
            foreach (IShape painter in _shapeList)
            {
                Point point1 = painter.GetPoints()[0];
                Point point2 = painter.GetPoints()[1];

                if (IsInsideArea(_areaTopLeft, _areaRightBottom, point1) && IsInsideArea(_areaTopLeft, _areaRightBottom, point2))
                    _clipboard.Add((IShape)painter.Clone());
            }
        }

        bool IsOutOfBoard(Point point)
        {
            if (point.X <= 0 || point.X >= WHITEBOARD_WIDTH)
                return true;
            if (point.Y <= 0 || point.Y > WHITEBOARD_HEIGHT)
                return true;
            return false;
        }

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
            string[] thicknessList = ["5", "6", "7", "8", "9", "10"];
            ThicknessCombobox.ItemsSource = thicknessList;
            ThicknessCombobox.SelectedIndex = thicknessList.Length - 1;
        }

        private void ResizeSquareMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isResizing = true;
            WhiteBoard.Children.Remove(_resizeSquare);
            _resizeSquare = null;
        }

        private void PainterMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (mode == CursorMode.Draw)
                return;

            if (_resizeSquare != null)
            {
                WhiteBoard.Children.Remove(_resizeSquare);
                _resizeSquare = null; 
                _isResizing = false;
            }

            IShape _oldPainter = _selectedPainter;

            _start = e.GetPosition(WhiteBoard);

            int index = -1;
            if (e.Source is FrameworkElement element && element.Tag != null)
            {
                index = Convert.ToInt32(element.Tag);
                //MessageBox.Show(index.ToString());
            }

            if (index < _shapeList.Count && index != -1)
            {
                _selectedPainter = _shapeList[index];
                _selectedStart = _selectedPainter.GetPoints()[0];
                _selectedEnd = _selectedPainter.GetPoints()[1];
                EditToolbar.Visibility = Visibility.Visible;
                RotateTextbox.Text = _selectedPainter.GetRotationDeg().ToString();
                _foundShape = true;

                _resizeSquare = new Rectangle();
                _resizeSquare.Width = 20;
                _resizeSquare.Height = 20;
                _resizeSquare.StrokeThickness = 1;
                _resizeSquare.Stroke = Brushes.Black;
                _resizeSquare.Fill = Brushes.Wheat;
                _resizeSquare.MouseDown += ResizeSquareMouseDown;
                Canvas.SetLeft(_resizeSquare, _selectedStart.X > _selectedEnd.X ? _selectedStart.X + 10: _selectedEnd.X + 10);
                Canvas.SetTop(_resizeSquare, _selectedStart.Y > _selectedEnd.Y ? _selectedStart.Y +10: _selectedEnd.Y + 10);
                WhiteBoard.Children.Add(_resizeSquare);
            }
            else
            {
                _selectedPainter = null;
                EditToolbar.Visibility = Visibility.Hidden;
                MessageBox.Show("Index out of bound in PainterMouseUp");
            }

            if (Object.Equals(_oldPainter, _selectedPainter))
            {
                _isDragAndDrop = true;
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

            /*Border textwrap = new Border();
            textwrap.Width = 100;
            textwrap.Height = 100;
            textwrap.BorderThickness = new Thickness(1);
            textwrap.BorderBrush = Brushes.Black;

            // Create a TextBlock instance
            TextBlock text = new TextBlock();
            text.Text = "sakfbasdkjajdbajkfbakjf aaa aaa aaa aaa aaa aaa";
            text.TextWrapping = TextWrapping.Wrap;
            text.TextAlignment = TextAlignment.Center;
            text.VerticalAlignment = VerticalAlignment.Center;

            // Add the TextBlock as a child of the Border
            textwrap.Child = text;

            // Assuming WhiteBoard is a Canvas
            Canvas.SetLeft(textwrap, 50); // Adjust these values as needed
            Canvas.SetTop(textwrap, 50);

            // Add the Border to the Canvas
            WhiteBoard.Children.Add(textwrap);*/
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (mode == CursorMode.Draw)
            {
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

                if (_painter.Name != "Star" && _painter.Name != "Pentagon")
                {
                    Shape _newPainter = (Shape) _painter.Convert(_style, _thickness, _currentColor);
                    _newPainter.Tag = _shapeList.Count;
                    _newPainter.MouseDown += PainterMouseDown;
                    //_newPainter.MouseUp += PainterMouseUp;
                    WhiteBoard.Children.Add(_newPainter);
                }
                else
                {
                    Polygon _newPainter = (Polygon)_painter.Convert(_style, _thickness, _currentColor);
                    _newPainter.Tag = _shapeList.Count;
                    _newPainter.MouseDown += PainterMouseDown;
                    //_newPainter.MouseUp += PainterMouseUp;
                    WhiteBoard.Children.Add(_newPainter);
                }
            }
            else if (mode == CursorMode.Select)
            {
                if (_isDragAndDrop || _isResizing)
                {
                    return;
                }
                _start = e.GetPosition(WhiteBoard);
                _end = e.GetPosition(WhiteBoard);

                _areaSelector = new Rectangle();
                _areaSelectorPoint1 = _start;
                _areaSelector.Fill = Brushes.LightCyan;
                _areaSelector.StrokeThickness = 2;
                _areaSelector.Stroke = Brushes.Cyan;
                _areaSelector.Opacity = 0.3;

                _areaSelector.Width = Math.Abs(_start.X - _end.X);
                _areaSelector.Height = Math.Abs(_start.Y - _end.Y);

                Canvas.SetLeft(_areaSelector, _start.X < _end.X ? _start.X : _end.X);
                Canvas.SetTop(_areaSelector, _start.Y < _end.Y ? _start.Y : _end.Y);
                WhiteBoard.Children.Add(_areaSelector);
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (mode == CursorMode.Draw)
            {
                if (_isDrawing)
                {
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
            else if (mode == CursorMode.Select)
            {
                if (_isDragAndDrop)
                {
                    _end = e.GetPosition(WhiteBoard);

                    double deltaX = _end.X - _start.X;
                    double deltaY = _end.Y - _start.Y;

                    Point newTopLeft = new Point(_selectedStart.X + deltaX, _selectedStart.Y + deltaY);
                    Point newRightBottom = new Point(_selectedEnd.X + deltaX, _selectedEnd.Y + deltaY);

                    _selectedPainter.UpdateShape(newTopLeft, newRightBottom);

                    Point newStart = _selectedPainter.GetPoints()[0];
                    Point newEnd = _selectedPainter.GetPoints()[1];
                    Canvas.SetLeft(_resizeSquare, newStart.X > newEnd.X ? newStart.X + 10 : newEnd.X + 10);
                    Canvas.SetTop(_resizeSquare, newStart.Y > newEnd.Y ? newStart.Y + 10 : newEnd.Y + 10);
                }
                else if (_isResizing)
                {
                    _end = e.GetPosition(WhiteBoard);
                    if (_selectedPainter != null)
                        _selectedPainter.UpdateShape(_selectedStart, _end);
                }
                else
                {
                    if (_areaSelector != null)
                    {
                        if (!_foundShape)
                        {
                            _end = e.GetPosition(WhiteBoard);

                            _areaSelector.Width = Math.Abs(_start.X - _end.X);
                            _areaSelector.Height = Math.Abs(_start.Y - _end.Y);

                            Canvas.SetLeft(_areaSelector, _start.X < _end.X ? _start.X : _end.X);
                            Canvas.SetTop(_areaSelector, _start.Y < _end.Y ? _start.Y : _end.Y);
                        }
                        else
                        {
                            _end = e.GetPosition(WhiteBoard);
                        }
                    }
                }
            }
        }
        
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (mode == CursorMode.Draw)
            {
                if (_start.X != _end.X && _start.Y != _end.Y)
                {
                    _shapeList.Add((IShape)_painter.Clone());
                }

                _isDrawing = false;
            }
            else if (mode == CursorMode.Select)
            {
                if (_isDragAndDrop)
                {
                    _isDragAndDrop = false;
                    _foundShape = false;
                    _selectedStart = _selectedPainter.GetPoints()[0];
                    _selectedEnd = _selectedPainter.GetPoints()[1];
                }
                else if (_isResizing)
                {
                    _isResizing = false;
                    _foundShape = false;
                }
                else
                {
                    if (_areaSelector != null)
                    {
                        _areaSelectorPoint2 = _end;
                        CopyToClipboard();
                        //MessageBox.Show(_clipboard.Count().ToString());
                        WhiteBoard.Children.Remove(_areaSelector);
                        _areaSelector = null;

                        if (!_foundShape)
                        {
                            EditToolbar.Visibility = Visibility.Hidden;
                            _selectedPainter = null;
                        }
                        else
                        {
                            _foundShape = false;
                        }
                    }
                }
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
                EditToolbar.Visibility = Visibility.Hidden;
                _selectedPainter = null;
                _foundShape = false;
                if (_resizeSquare != null)
                {
                    WhiteBoard.Children.Remove(_resizeSquare);
                    _resizeSquare = null;
                }
            }
            else
            {
                mode = CursorMode.Select;
            }
        }

        private void RotateMinus_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_selectedPainter.GetRotationDeg() > 0)
            {
                double currentDeg = _selectedPainter.GetRotationDeg();
                _selectedPainter.AddRotation(currentDeg - 1);
                RotateTextbox.Text = _selectedPainter.GetRotationDeg().ToString();
            }
        }

        private void RotatePlus_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_selectedPainter.GetRotationDeg() < 360)
            {
                double currentDeg = _selectedPainter.GetRotationDeg();
                _selectedPainter.AddRotation(currentDeg + 1);
                RotateTextbox.Text = _selectedPainter.GetRotationDeg().ToString(); 
            }
        }

        private void AddTextBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPainter == null)
                return;

            AddTextWindow dialog = new AddTextWindow(WhiteBoard, _selectedPainter);
            dialog.ShowDialog();
        }
    }
}