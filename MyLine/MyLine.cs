
using Shapes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MyLine
{
    public class MyLine : IShape
    {
        private Point _start;
        private Point _end;
        Line shape;
        double thickness;
        SolidColorBrush brush;
        int style;
        double rotateDeg;
        Border textWrap;
        TextBlock textBlock;
        SolidColorBrush background;
        SolidColorBrush foreground;

        public string Name => "Line";
        public bool ShiftPressed { get; set; } = false;
        public string Layer { get; set; } = "";


        public void AddPoints(Point point1, Point point2)
        {
            _start = point1;
            _end = point2;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public UIElement Convert(int style, double thickness, SolidColorBrush color)
        {
            this.thickness = thickness;
            this.style = style;
            brush = color;

            shape = new Line()
            {
                X1 = _start.X,
                Y1 = _start.Y,
                X2 = _end.X,
                Y2 = _end.Y,
                StrokeThickness = this.thickness,
                Stroke = brush
            };

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
            shape.X1 = _start.X;
            shape.Y1 = _start.Y;
            shape.X2 = _end.X;
            shape.Y2 = _end.Y;

            if (textWrap != null)
            {
                textWrap.Width = Math.Abs(_end.X - _start.X) - thickness * 2 > 0 ? Math.Abs(_end.X - _start.X) - thickness * 2 : 50;
                textWrap.Height = Math.Abs(_end.Y - _start.Y) - thickness * 2 > 0 ? Math.Abs(_end.Y - _start.Y) - thickness * 2 : 50;
                Canvas.SetLeft(textWrap, _start.X < _end.X ? _start.X + this.thickness : _end.X + this.thickness);
                Canvas.SetTop(textWrap, _start.Y < _end.Y ? _start.Y + this.thickness : _end.Y + this.thickness);
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
            UpdateShape(this._start, this._end);
            return shape;
        }

        private double CalculateAngle()
        {
            
            double deltaX = _start.X - _end.X;
            double deltaY = _start.Y - _end.Y;

            // Tính toán góc gi?a vector này và vector ch? h??ng lên trên
            double angle = Math.Atan2(deltaY, deltaX) * (180 / Math.PI);

            // Chuy?n ??i góc t? radian sang ?? và ?i?u ch?nh ?? n?m trong kho?ng t? 0 ??n 360 ??
            if (angle < 0)
            {
                angle += 360;
            }

            return angle;
        }

        public void AddRotation(double deg)
        {
            this.rotateDeg = deg;
            Point center = new Point((_start.X + _end.X)/2, (_start.Y + _end.Y) / 2);
            RotateTransform rotateTransform = new RotateTransform(this.rotateDeg, center.X, center.Y);
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
            return [_start, _end];
        }

        public void SetText(string font, SolidColorBrush background, SolidColorBrush foreground, double size, string text)
        {
            this.background = background;
            this.foreground = foreground;

            textWrap = new Border();
            textWrap.BorderThickness = new Thickness(0);
            textWrap.Width = Math.Abs(_end.X - _start.X) - thickness * 2 > 0 ? Math.Abs(_end.X - _start.X) - thickness * 2 : 50;
            textWrap.Height = Math.Abs(_end.Y - _start.Y) - thickness * 2 > 0 ? Math.Abs(_end.Y - _start.Y) - thickness * 2 : 50;

            textBlock = new TextBlock();
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.TextAlignment = TextAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textWrap.Child = textBlock;


            Canvas.SetLeft(textWrap, _start.X < _end.X ? _start.X + this.thickness : _end.X + this.thickness);
            Canvas.SetTop(textWrap, _start.Y < _end.Y ? _start.Y + this.thickness : _end.Y + this.thickness);

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
                return [Name, ShiftPressed, _start, _end, style, thickness, brush, rotateDeg, Layer, true, textBlock.FontFamily.Source, background, foreground, textBlock.FontSize, textBlock.Text];
            else
                return [Name, ShiftPressed, _start, _end, style, thickness, brush, rotateDeg, Layer, false];
        }
    }
}
