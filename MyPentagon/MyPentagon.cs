
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using Shapes;

namespace MyTriangle
{
    public class MyPentagon : IShape
    {
        private Point _topLeft;
        private Point _rightBottom;
        Polygon shape;
        double thickness;
        SolidColorBrush brush;
        int style;
        double rotateDeg;
        Border textWrap;
        TextBlock textBlock;
        SolidColorBrush background;
        SolidColorBrush foreground;

        public string Name => "Pentagon";
        public bool ShiftPressed { get; set; } = false;
        public string Layer { get; set; } = "";


        public void AddPoints(Point point1, Point point2)
        {
            _topLeft = new Point(point1.X < point2.X ? point1.X : point2.X, point1.Y < point2.Y ? point1.Y : point2.Y);
            _rightBottom = new Point(point1.X > point2.X ? point1.X : point2.X, point1.Y > point2.Y ? point1.Y : point2.Y);
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public UIElement Convert(int style, double thickness, SolidColorBrush color)
        {
            brush = color;
            this.thickness = thickness;
            this.style = style;

            Point center = new Point((_topLeft.X + _rightBottom.X) / 2, (_topLeft.Y + _rightBottom.Y) / 2);
            Vector vector = Point.Subtract(_topLeft, _rightBottom);
            double radius = vector.Length / 2;

            Polygon polygon = new Polygon();
            polygon.Stroke = this.brush;
            polygon.StrokeThickness = this.thickness;

            for (int i = 0; i < 5; i++)
            {
                double angle = i * 2 * Math.PI / 5; 
                double x = center.X + radius * Math.Cos(angle); 
                double y = center.Y + radius * Math.Sin(angle); 
                polygon.Points.Add(new Point(x, y)); 
            }
            shape = polygon;

            DoubleCollection _style = null;

            if (style == 0)
            {
                _style = null;
            }
            else if (style == 1)
            {
                _style = new DoubleCollection() { 5, 2 };
            }
            else if (style == 2)
            {
                _style = new DoubleCollection() { 1, 1 };
            }
            else if (style == 3)
            {
                _style = new DoubleCollection() { 5, 2, 1, 2 };
            }

            else if (style == 4)
            {
                _style = new DoubleCollection() { 5, 2, 1, 2, 1, 2 };
            }

            if (_style != null)
            {
                shape.StrokeDashArray = _style;
            }

            return shape;
        }

        public void UpdateShape(Point point1, Point point2)
        {
            AddPoints(point1, point2);

            Point center = new Point((_topLeft.X + _rightBottom.X) / 2, (_topLeft.Y + _rightBottom.Y) / 2);
            Vector vector = Point.Subtract(_topLeft, _rightBottom);
            double radius = vector.Length / 2;

            shape.Points.Clear();
            for (int i = 0; i < 5; i++)
            {
                double angle = i * 2 * Math.PI / 5;
                double x = center.X + radius * Math.Cos(angle);
                double y = center.Y + radius * Math.Sin(angle);
                shape.Points.Add(new Point(x, y));
            }

            if (textWrap != null)
            {
                textWrap.Width = vector.Length - thickness * 2 > 0 ? vector.Length - thickness * 2 : 50;
                textWrap.Height = vector.Length - thickness * 2 > 0 ? vector.Length - thickness * 2 : 50;
                double top = center.X - (textWrap.Height / 2) + this.thickness;
                double left = center.Y - (textWrap.Width / 2) + this.thickness;
                Canvas.SetLeft(textWrap, top);
                Canvas.SetTop(textWrap, left);
            }
        }
        public override string ToString()
        {
            return Name;
        }

        public UIElement GetShape()
        {
            Convert(this.style, this.thickness, this.brush);
            if (this.rotateDeg != null)
                AddRotation(this.rotateDeg);
            UpdateShape(this._topLeft, this._rightBottom);
            return shape;
        }

        public void AddRotation(double deg)
        {
            this.rotateDeg = deg;
            double centerX = shape.Points.Average(p => p.X);
            double centerY = shape.Points.Average(p => p.Y);

            RotateTransform rotateTransform = new RotateTransform(this.rotateDeg, centerX, centerY);
            shape.RenderTransform = rotateTransform;

            if (textWrap != null)
            {
                RotateTransform textRotateTransform = new RotateTransform(this.rotateDeg, textWrap.ActualWidth / 2, textWrap.ActualHeight / 2);
                textWrap.RenderTransform = textRotateTransform;
            }
        }

        public double GetRotationDeg()
        {
            if (rotateDeg != null)
                return rotateDeg;
            else return 0;
        }

        public Point[] GetPoints()
        {
            return [_topLeft, _rightBottom];
        }

        public void SetText(string font, SolidColorBrush background, SolidColorBrush foreground, double size, string text)
        {
            this.background = background;
            this.foreground = foreground;

            Vector vector = Point.Subtract(_topLeft, _rightBottom);
            textWrap = new Border();
            textWrap.BorderThickness = new Thickness(0);
            textWrap.Width = vector.Length - thickness * 2 > 0 ? vector.Length - thickness * 2 : 50;
            textWrap.Height = vector.Length - thickness * 2 > 0 ? vector.Length - thickness * 2 : 50;

            textBlock = new TextBlock();
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.TextAlignment = TextAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textWrap.Child = textBlock;

            Point center = new Point((_topLeft.X + _rightBottom.X) / 2, (_topLeft.Y + _rightBottom.Y) / 2);
            double top = center.X - (textWrap.Height / 2) + this.thickness;
            double left = center.Y - (textWrap.Width / 2) + this.thickness;
            Canvas.SetLeft(textWrap, top);
            Canvas.SetTop(textWrap, left);

            textBlock.Text = text;
            textBlock.FontFamily = new FontFamily(font);
            textBlock.Foreground = foreground;
            textBlock.Background = background;
            textBlock.FontSize = size;

            if (rotateDeg != null)
            {
                RotateTransform textRotateTransform = new RotateTransform(this.rotateDeg, textWrap.Width / 2, textWrap.Height / 2);
                textWrap.RenderTransform = textRotateTransform;
            }
        }

        public Border GetText()
        {
            if (textWrap != null)
                return textWrap;
            else
                return null;
        }

        public Border RecreateText()
        {
            string text = textBlock.Text;
            string font = textBlock.FontFamily.Source;
            double size = textBlock.FontSize;

            SetText(font, this.background, this.foreground, size, text);
            return textWrap;
        }

        public void EditText(string font, SolidColorBrush background, SolidColorBrush foreground, double size, string text)
        {
            this.background = background;
            this.foreground = foreground;

            textBlock.Text = text;
            textBlock.FontFamily = new FontFamily(font);
            textBlock.Foreground = foreground;
            textBlock.Background = background;
            textBlock.FontSize = size;
        }

        public Object[] GetProperty()
        {
            if (textBlock != null)
                return [Name, ShiftPressed, _topLeft, _rightBottom, style, thickness, brush, rotateDeg, Layer, true, textBlock.FontFamily.Source, background, foreground, textBlock.FontSize, textBlock.Text];
            else
                return [Name, ShiftPressed, _topLeft, _rightBottom, style, thickness, brush, rotateDeg, Layer, false];
        }
    }
}
