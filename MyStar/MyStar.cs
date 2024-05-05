
using Shapes;
using System.Reflection.Emit;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MyStar
{
    public class MyStar : IShape
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

        public string Name => "Star";
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

            Polygon star = new Polygon();
            shape = star;
            star.Stroke = this.brush;
            star.StrokeThickness = this.thickness;

            PointCollection points = new PointCollection();
            double outerRadius = Math.Max(Math.Abs(_rightBottom.X - _topLeft.X), Math.Abs(_rightBottom.Y - _topLeft.Y));
            double innerRadius = outerRadius / 2;

            double angleOffset = -Math.PI / 2;
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

            star.Points = points;

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

            Canvas.SetLeft(shape, _topLeft.X);
            Canvas.SetTop(shape, _topLeft.Y);
            return shape;
        }

        public void UpdateShape(Point point1, Point point2)
        {
            AddPoints(point1, point2);

            PointCollection points = new PointCollection();
            double outerRadius = Math.Max(Math.Abs(_rightBottom.X - _topLeft.X), Math.Abs(_rightBottom.Y - _topLeft.Y)) / 2;
            double outerSize = Math.Max(Math.Abs(_rightBottom.X - _topLeft.X), Math.Abs(_rightBottom.Y - _topLeft.Y));
            double innerRadius = outerRadius / 2;

            double angleOffset = -Math.PI / 2;
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

            shape.Points = points;

            Canvas.SetLeft(shape, _topLeft.X);
            Canvas.SetTop(shape, _topLeft.Y);

            if (textWrap != null)
            {
                textWrap.Width = outerSize - thickness * 2 > 0 ? outerSize - thickness * 2 : 50;
                textWrap.Height = outerSize - thickness * 2 > 0 ? outerSize - thickness * 2 : 50;
                Canvas.SetLeft(textWrap, _topLeft.X + this.thickness);
                Canvas.SetTop(textWrap, _topLeft.Y + this.thickness);
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public UIElement GetShape()
        {
            Convert(this.style, this.thickness, this.brush);
            UpdateShape(this._topLeft, this._rightBottom);

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

            textWrap = new Border();
            textWrap.BorderThickness = new Thickness(0);
            double outerRadius = Math.Max(Math.Abs(_rightBottom.X - _topLeft.X), Math.Abs(_rightBottom.Y - _topLeft.Y));
            textWrap.Width = outerRadius - thickness * 2 > 0 ? outerRadius - thickness * 2 : 50;
            textWrap.Height = outerRadius - thickness * 2 > 0 ? outerRadius - thickness * 2 : 50;

            textBlock = new TextBlock();
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.TextAlignment = TextAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textWrap.Child = textBlock;

            Canvas.SetLeft(textWrap, _topLeft.X + this.thickness);
            Canvas.SetTop(textWrap, _topLeft.Y + this.thickness);

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
