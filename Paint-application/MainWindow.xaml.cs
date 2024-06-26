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
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;

namespace Paint_application
{
    public enum CursorMode
    {
        Draw, Select
    }

    public enum PastingMode
    {
        None, Copy, Cut
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }

        double WHITEBOARD_WIDTH = 1150;
        double WHITEBOARD_HEIGHT = 550;

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
        List<int> _clipBoardIndex = new List<int>();    
        List<IShape> _pastingClipboard = new List<IShape>();

        List<List<IShape>> _history = new List<List<IShape>>();
        List<ObservableCollection<string>> _layerHistory = new List<ObservableCollection<string>>();
        List<ObservableCollection<bool>> _layerStateHistory = new List<ObservableCollection<bool>>();
        int _historyIndex;
        PastingMode _pastingMode = PastingMode.None;
        ObservableCollection<string> _layerList = new ObservableCollection<string>();
        ObservableCollection<bool> _layerState = new ObservableCollection<bool>();
        string _currentLayer;

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
            _clipBoardIndex.Clear();
            for (int i = 0; i < _shapeList.Count; i++)
            {
                Point point1 = _shapeList[i].GetPoints()[0];
                Point point2 = _shapeList[i].GetPoints()[1];

                if (IsInsideArea(_areaTopLeft, _areaRightBottom, point1) && IsInsideArea(_areaTopLeft, _areaRightBottom, point2))
                {
                    int layerIndex = GetLayerIndex(_shapeList[i].Layer);
                    if (layerIndex != -1 && _layerState[layerIndex])
                    {
                        _clipboard.Add((IShape)_shapeList[i].Clone());
                        _clipBoardIndex.Add(i);
                    }
                }
            }

            if (_clipBoardIndex.Count > 0)
            {
                _clipBoardIndex.Sort((a, b) => b.CompareTo(a));
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
                int layerIndex = GetLayerIndex(_shapeList[index].Layer);
                if (layerIndex == -1 || !_layerState[layerIndex])
                {
                    return;
                }

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
            LayerList.ItemsSource = _layerList;
            CurrentLayerCombobox.ItemsSource = _layerList;

            _layerList.Add("Layer 1");
            _layerState.Add(true);
            CurrentLayerCombobox.SelectedIndex = 0;

            _history.Add(new List<IShape>());

            ObservableCollection<string> layerHistoryItem = new ObservableCollection<string>();
            layerHistoryItem.Add(_layerList[0]);
            _layerHistory.Add(layerHistoryItem);

            ObservableCollection<bool> layerStateHistoryItem = new ObservableCollection<bool>();
            layerStateHistoryItem.Add(_layerState[0]);
            _layerStateHistory.Add(layerStateHistoryItem);

            _historyIndex = 0;

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

                _painter.Layer = _currentLayer;
                Shape _newPainter = (Shape)_painter.Convert(StyleCombobox.SelectedIndex, _thickness, _currentColor);
                _newPainter.Tag = _shapeList.Count;
                _newPainter.MouseDown += PainterMouseDown;
                WhiteBoard.Children.Add(_newPainter);
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
                        if (_resizeSquare != null)
                        {
                            WhiteBoard.Children.Remove(_resizeSquare);
                            _resizeSquare = null;
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
                    AddToHistory();
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
                    AddToHistory();
                }
                else if (_isResizing)
                {
                    _isResizing = false;
                    _foundShape = false;
                    AddToHistory();
                }
                else
                {
                    if (_areaSelector != null)
                    {
                        _areaSelectorPoint2 = _end;
                        CopyToClipboard();
                        WhiteBoard.Children.Remove(_areaSelector);
                        _areaSelector = null;

                        if (!_foundShape)
                        {
                            EditToolbar.Visibility = Visibility.Hidden;
                            _selectedPainter = null;
                            if (_resizeSquare != null)
                            {
                                WhiteBoard.Children.Remove(_resizeSquare);
                                _resizeSquare = null;
                            }
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

        private void CurrentLayerCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _currentLayer = (string)CurrentLayerCombobox.SelectedItem;
        }

        private void RotateMinus_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_selectedPainter.GetRotationDeg() > 0)
            {
                double currentDeg = _selectedPainter.GetRotationDeg();
                _selectedPainter.AddRotation(currentDeg - 1);
                RotateTextbox.Text = _selectedPainter.GetRotationDeg().ToString();
                AddToHistory();
            }
        }

        private void RotatePlus_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_selectedPainter.GetRotationDeg() < 360)
            {
                double currentDeg = _selectedPainter.GetRotationDeg();
                _selectedPainter.AddRotation(currentDeg + 1);
                RotateTextbox.Text = _selectedPainter.GetRotationDeg().ToString();
                AddToHistory();
            }
        }

        private void RotateTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string text = RotateTextbox.Text;
                string pattern = @"^\d+$";

                if (!Regex.IsMatch(text, pattern))
                {
                    RotateTextbox.Text = _selectedPainter.GetRotationDeg().ToString();
                    return;
                }

                double deg = Double.Parse(text);

                if (deg < 0 || deg > 360)
                {
                    RotateTextbox.Text = _selectedPainter.GetRotationDeg().ToString();
                    return;
                }

                _selectedPainter.AddRotation(deg);
                RotateTextbox.Text = _selectedPainter.GetRotationDeg().ToString();
                AddToHistory();
            }
        }

        private void AddTextBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPainter == null)
                return;

            AddTextWindow dialog = new AddTextWindow(WhiteBoard, _selectedPainter);
            dialog.ShowDialog();
            AddToHistory();
        }

        private void SaveToFile(string filePath)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(fileStream))
            {
                writer.Write(_shapeList.Count);

                foreach (IShape painter in _shapeList)
                {
                    Object[] properties = painter.GetProperty();

                    writer.Write((string)properties[0]);//Name
                    writer.Write((bool)properties[1]);//ShiftPressed

                    Point TempPoint = (Point)properties[2];//_topLeft
                    writer.Write(TempPoint.X);
                    writer.Write(TempPoint.Y);
                    TempPoint = (Point)properties[3];//_rightBottom
                    writer.Write(TempPoint.X);
                    writer.Write(TempPoint.Y);

                    writer.Write((int)properties[4]);//style
                    writer.Write((double)properties[5]);//thickness

                    Color color = ((SolidColorBrush) properties[6]).Color;//brush
                    writer.Write(color.A);
                    writer.Write(color.R);
                    writer.Write(color.G);
                    writer.Write(color.B);

                    writer.Write((double)properties[7]);//rotateDeg
                    writer.Write((string)properties[8]);//Layer

                    bool haveText = (bool)properties[9];//have text or not
                    writer.Write(haveText);
                    if (!haveText)
                        continue;

                    writer.Write((string)properties[10]);//textBlock.FontFamily.Source

                    color = ((SolidColorBrush)properties[11]).Color;//background
                    writer.Write(color.A);
                    writer.Write(color.R);
                    writer.Write(color.G);
                    writer.Write(color.B);

                    color = ((SolidColorBrush)properties[12]).Color;//foreground
                    writer.Write(color.A);
                    writer.Write(color.R);
                    writer.Write(color.G);
                    writer.Write(color.B);

                    writer.Write((double)properties[13]);//size
                    writer.Write((string)properties[14]);//text
                }
            }
        }

        private void LoadFromFile(string filePath)
        {
            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                using (BinaryReader reader = new BinaryReader(fileStream))
                {
                    int count = reader.ReadInt32();

                    for (int i = 0; i < count; i++)
                    {
                        string shape = reader.ReadString();//Name
                        IShape painter = null;

                        foreach (IShape item in _prototypes)
                        {
                            if (item.Name == shape)
                                painter = item;
                        }

                        if (painter == null)
                            continue;

                        bool isShiftPressed = reader.ReadBoolean();
                        painter.ShiftPressed = isShiftPressed;//ShiftPressed

                        Double tempPointX = reader.ReadDouble();//_topLeft
                        Double tempPointY = reader.ReadDouble();
                        Point point1 = new Point(tempPointX, tempPointY);

                        tempPointX = reader.ReadDouble();//_rightBottom
                        tempPointY = reader.ReadDouble();
                        Point point2 = new Point(tempPointX, tempPointY);

                        painter.AddPoints(point1, point2);

                        int style = reader.ReadInt32();//style
                        double thickness = reader.ReadDouble();//thickness

                        byte a = reader.ReadByte();//brush
                        byte r = reader.ReadByte();
                        byte g = reader.ReadByte();
                        byte b = reader.ReadByte();
                        Color color = Color.FromArgb(a, r, g, b);
                        SolidColorBrush brush = new SolidColorBrush(color);

                        painter.Convert(style, thickness, brush);//add to list
                        Shape _newPainter = (Shape)painter.GetShape();//add to list
                        _newPainter.Tag = _shapeList.Count;
                        _newPainter.MouseDown += PainterMouseDown;
                        WhiteBoard.Children.Add(_newPainter);

                        double rotateDeg = reader.ReadDouble();//rotateDeg
                        painter.AddRotation(rotateDeg);

                        string layer = reader.ReadString();//Layer
                        painter.Layer = layer;
                        if (GetLayerIndex(layer) == -1)
                        {
                            _layerList.Add(layer);
                            _layerState.Add(true);
                        }
                        bool haveText =  reader.ReadBoolean();//have text or not

                        if (haveText)
                        {
                            string font = reader.ReadString();// font

                            a = reader.ReadByte();//background
                            r = reader.ReadByte();
                            g = reader.ReadByte();
                            b = reader.ReadByte();
                            color = Color.FromArgb(a, r, g, b);
                            SolidColorBrush background = new SolidColorBrush(color);

                            a = reader.ReadByte();//foreground
                            r = reader.ReadByte();
                            g = reader.ReadByte();
                            b = reader.ReadByte();
                            color = Color.FromArgb(a, r, g, b);
                            SolidColorBrush foreground = new SolidColorBrush(color);

                            double textSize = reader.ReadDouble();//size
                            string text = reader.ReadString();//text

                            painter.SetText(font, background, foreground, textSize, text);
                            WhiteBoard.Children.Add(painter.GetText());
                        }
                        _shapeList.Add((IShape)painter.Clone());
                    }
                    CurrentLayerCombobox.SelectedIndex = _layerList.Count - 1;
                    CheckLayerState();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error loading file \n Please make sure that the file is valid");
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Binary Files (*.bin)|*.bin";

            if (saveFileDialog.ShowDialog() == true)
            {
                SaveToFile(saveFileDialog.FileName);
            }
        }

        private void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Binary Files (*.bin)|*.bin";

            if (openFileDialog.ShowDialog() == true)
            {
                _shapeList.Clear();
                WhiteBoard.Children.Clear();
                _layerList.Clear();
                _layerState.Clear();
                EditToolbar.Visibility = Visibility.Hidden;
                _selectedPainter = null;
                LoadFromFile(openFileDialog.FileName);
                AddToHistory();
            }
        }

        private void AddToHistory()
        {
            if (_historyIndex == _history.Count - 1)
            {
                List<IShape> historyItem = new List<IShape>();
                ObservableCollection<string> layerHistoryItem = new ObservableCollection<string>();
                ObservableCollection<bool> layerStateHistoryItem = new ObservableCollection<bool>();

                foreach (IShape shape in _shapeList)
                {
                    historyItem.Add((IShape)shape.Clone());
                }
                _history.Add(historyItem);

                foreach (string layer in _layerList)
                {
                    layerHistoryItem.Add(layer);
                }
                _layerHistory.Add(layerHistoryItem);

                foreach (bool state in _layerState)
                {
                    layerStateHistoryItem.Add(state);
                }
                _layerStateHistory.Add(layerStateHistoryItem);

                _historyIndex = _history.Count - 1;
            }
            else
            {
                _history.RemoveRange(_historyIndex + 1, _history.Count - _historyIndex - 1);
                List<IShape> historyItem = new List<IShape>();
                ObservableCollection<string> layerHistoryItem = new ObservableCollection<string>();
                ObservableCollection<bool> layerStateHistoryItem = new ObservableCollection<bool>();

                foreach (IShape shape in _shapeList)
                {
                    historyItem.Add((IShape)shape.Clone());
                }
                _history.Add(historyItem);

                foreach (string layer in _layerList)
                {
                    layerHistoryItem.Add(layer);
                }
                _layerHistory.Add(layerHistoryItem);

                foreach (bool state in _layerState)
                {
                    layerStateHistoryItem.Add(state);
                }
                _layerStateHistory.Add(layerStateHistoryItem);

                _historyIndex = _history.Count - 1;
            }
            //MessageBox.Show(_history.Count + " " + _layerHistory.Count + " " + _layerStateHistory.Count);
        }

        private void UndoHistory()
        {
            if (_historyIndex == 0)
                return;
            _historyIndex--;

            _shapeList.Clear();
            WhiteBoard.Children.Clear();
            EditToolbar.Visibility = Visibility.Hidden;
            _selectedPainter = null;

            List<IShape> currentState = _history[_historyIndex];
            foreach (IShape painter in currentState)
            {
                Shape _newPainter = (Shape)painter.GetShape();
                _newPainter.Tag = _shapeList.Count;
                _newPainter.MouseDown += PainterMouseDown;
                WhiteBoard.Children.Add(_newPainter);

                if (painter.GetText() != null)
                {
                    WhiteBoard.Children.Add(painter.GetText());
                }
                _shapeList.Add((IShape)painter.Clone());
            }

            int workingIndex = CurrentLayerCombobox.SelectedIndex;
            _layerList = _layerHistory[_historyIndex];
            _layerState = _layerStateHistory[_historyIndex];
            LayerList.ItemsSource = _layerList;
            CurrentLayerCombobox.ItemsSource = _layerList;
            CurrentLayerCombobox.SelectedIndex = workingIndex < _layerList.Count - 1? workingIndex : _layerList.Count - 1;
            CheckLayerState();
        }

        private void RedoHistory()
        {
            if (_historyIndex == _history.Count - 1)
                return;
            _historyIndex++;

            _shapeList.Clear();
            WhiteBoard.Children.Clear();
            EditToolbar.Visibility = Visibility.Hidden;
            _selectedPainter = null;

            List<IShape> currentState = _history[_historyIndex];
            foreach (IShape painter in currentState)
            {
                Shape _newPainter = (Shape)painter.GetShape();
                _newPainter.Tag = _shapeList.Count;
                _newPainter.MouseDown += PainterMouseDown;
                WhiteBoard.Children.Add(_newPainter);

                if (painter.GetText() != null)
                {
                    WhiteBoard.Children.Add(painter.GetText());
                }
                _shapeList.Add((IShape)painter.Clone());
            }

            int workingIndex = CurrentLayerCombobox.SelectedIndex;
            _layerList = _layerHistory[_historyIndex];
            _layerState = _layerStateHistory[_historyIndex];
            LayerList.ItemsSource = _layerList;
            CurrentLayerCombobox.ItemsSource = _layerList;
            CurrentLayerCombobox.SelectedIndex = workingIndex < _layerList.Count - 1 ? workingIndex : _layerList.Count - 1;
            CheckLayerState();
        }

        private void Copy()
        {
            if (_clipboard.Count == 0)
                return;

            _pastingClipboard.Clear();
            foreach (IShape painter in _clipboard)
            {
                _pastingClipboard.Add(painter);
            }
            _pastingMode = PastingMode.Copy;
        }

        private void Cut()
        {
            if (_clipboard.Count == 0)
                return;

            _pastingClipboard.Clear();
            foreach (IShape painter in _clipboard)
            {
                _pastingClipboard.Add(painter);
            }
            _pastingMode = PastingMode.Cut;

            foreach (int index in _clipBoardIndex)
            {
                _shapeList.RemoveAt(index);
            }

            WhiteBoard.Children.Clear();
            EditToolbar.Visibility = Visibility.Hidden;
            _selectedPainter = null;

            for (int i = 0; i < _shapeList.Count; i++)
            {
                Shape _newPainter = (Shape)_shapeList[i].GetShape();
                _newPainter.Tag = i;
                _newPainter.MouseDown += PainterMouseDown;
                WhiteBoard.Children.Add(_newPainter);

                if (_shapeList[i].GetText() != null)
                {
                    WhiteBoard.Children.Add(_shapeList[i].GetText());
                }
            }
        }

        private void Paste()
        {
            if (_pastingMode == PastingMode.Copy)
            {
                if (_pastingClipboard.Count > 0)
                {
                    foreach (IShape painter in _pastingClipboard)
                    {
                        Point point1 = new Point(painter.GetPoints()[0].X + 30, painter.GetPoints()[0].Y + 30);
                        Point point2 = new Point(painter.GetPoints()[1].X + 30, painter.GetPoints()[1].Y + 30);

                        painter.AddPoints(point1, point2);
                        Shape _newPainter = (Shape)painter.GetShape();
                        _newPainter.Tag = _shapeList.Count;
                        _newPainter.MouseDown += PainterMouseDown;
                        WhiteBoard.Children.Add(_newPainter);

                        if (painter.GetText() != null)
                        {
                            WhiteBoard.Children.Add(painter.RecreateText());
                        }
                        _shapeList.Add((IShape)painter.Clone());
                    }
                }
                    
                _pastingMode = PastingMode.None;
                _clipboard.Clear();
                AddToHistory();
            }
            else if (_pastingMode == PastingMode.Cut)
            {
                if (_pastingClipboard.Count > 0)
                {
                    foreach (IShape painter in _pastingClipboard)
                    {
                        Shape _newPainter = (Shape)painter.GetShape();
                        _newPainter.Tag = _shapeList.Count;
                        _newPainter.MouseDown += PainterMouseDown;
                        WhiteBoard.Children.Add(_newPainter);

                        if (painter.GetText() != null)
                        {
                            WhiteBoard.Children.Add(painter.RecreateText());
                        }
                        _shapeList.Add((IShape)painter.Clone());
                    }
                }

                _pastingMode = PastingMode.None;
                _clipboard.Clear();
                AddToHistory();
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.Z)
                {
                    UndoHistory();
                }
                else if (e.Key == Key.Y)
                {
                    RedoHistory();
                }
                else if (e.Key == Key.C)
                {
                    Copy();
                }
                else if (e.Key == Key.X)
                {
                    Cut();
                }
                else if (e.Key == Key.V)
                {
                    Paste();
                }
            }
        }

        private int GetLayerIndex(string layerName)
        {
            int result = -1;
            
            for (int i = 0; i < _layerList.Count; i++)
            {
                if (layerName == _layerList[i])
                    result = i;
            }

            return result;
        }

        private void LockBtn_Click(object sender, RoutedEventArgs e)
        {
            if (LayerList.SelectedItems.Count == 0)
            {
                return;
            }

            int selected = LayerList.SelectedIndex;
            
            if (selected >= _layerState.Count)
                return;

            bool currentState = _layerState[selected];
            _layerState[selected] = !currentState;

            if (_layerState[selected] )
            {
                ListViewItem listViewItem = LayerList.ItemContainerGenerator.ContainerFromItem(LayerList.SelectedItem) as ListViewItem;
                if (listViewItem != null)
                {
                    listViewItem.Foreground = Brushes.Black;
                }
            }
            else
            {
                ListViewItem listViewItem = LayerList.ItemContainerGenerator.ContainerFromItem(LayerList.SelectedItem) as ListViewItem;
                if (listViewItem != null)
                {
                    listViewItem.Foreground = Brushes.Red;
                }
            }
            AddToHistory();
        }

        private void AddLayerBtn_Click(object sender, RoutedEventArgs e)
        {
            AddLayerWindow dialog = new AddLayerWindow(this._layerList, this._layerState, -1);
            dialog.ShowDialog();
            AddToHistory();
        }

        private void RenameLayerBtn_Click(object sender, RoutedEventArgs e)
        {
            if (LayerList.SelectedItems.Count == 0)
            {
                return;
            }

            int workingIndex = CurrentLayerCombobox.SelectedIndex;
            int selected = LayerList.SelectedIndex;
            string oldLayerName = _layerList[selected];

            AddLayerWindow dialog = new AddLayerWindow(this._layerList, this._layerState, selected);
            dialog.ShowDialog();

            string newLayerName = _layerList[selected];
            foreach (IShape painter in _shapeList)
            {
                if (painter.Layer == oldLayerName)
                {
                    painter.Layer = newLayerName;
                }
            }
            CurrentLayerCombobox.SelectedIndex = workingIndex;
        }

        private void RemoveLayerBtn_Click(object sender, RoutedEventArgs e)
        {
            if (LayerList.SelectedItems.Count == 0)
            {
                return;
            }

            if (_layerList.Count == 1)
            {
                MessageBox.Show("Must have at least one layer!");
                return;
            }

            int indexToRemove = LayerList.SelectedIndex;
            string layerToRemove = _layerList[indexToRemove];
            List<int> indexesToRemove = new List<int>();

            for  (int i = 0; i < _shapeList.Count; i++)
            {
                if (_shapeList[i].Layer == layerToRemove)
                {
                    indexesToRemove.Add(i);
                }
            }

            if (indexesToRemove.Count > 0)
            {
                indexesToRemove.Sort((a, b) => b.CompareTo(a));
            }

            foreach (int index in indexesToRemove)
            {
                _shapeList.RemoveAt(index);
            }

            WhiteBoard.Children.Clear();
            EditToolbar.Visibility = Visibility.Hidden;
            _selectedPainter = null;

            for (int i = 0; i < _shapeList.Count; i++)
            {
                Shape _newPainter = (Shape)_shapeList[i].GetShape();
                _newPainter.Tag = i;
                _newPainter.MouseDown += PainterMouseDown;
                WhiteBoard.Children.Add(_newPainter);

                if (_shapeList[i].GetText() != null)
                {
                    WhiteBoard.Children.Add(_shapeList[i].GetText());
                }
            }

            int workingIndex = CurrentLayerCombobox.SelectedIndex;
            _layerList.RemoveAt(indexToRemove);
            _layerState.RemoveAt(indexToRemove);
            CurrentLayerCombobox.SelectedIndex = workingIndex != indexToRemove ? workingIndex : 0;
            AddToHistory();
        }

        private void CheckLayerState()
        {
            for (int i = 0; i < _layerState.Count; i++)
            {
                if (_layerState[i])
                {
                    ListViewItem listViewItem = LayerList.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                    if (listViewItem != null)
                    {
                        listViewItem.Foreground = Brushes.Black;
                    }
                }
                else
                {
                    ListViewItem listViewItem = LayerList.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                    if (listViewItem != null)
                    {
                        listViewItem.Foreground = Brushes.Red;
                    }
                }
            }
        }
    }
}