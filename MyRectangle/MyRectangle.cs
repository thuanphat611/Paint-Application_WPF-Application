
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using Shapes;
using System.Windows.Media.Media3D;
using System.Runtime.InteropServices.JavaScript;

namespace MyRectangle
{
    public class MyRectangle : IShape
    {
        private Point _topLeft;
        private Point _rightBottom;
        Rectangle shape;
        double thickness;
        SolidColorBrush brush;
        int style;
        double rotateDeg;
        Border textWrap;
        TextBlock textBlock;
        SolidColorBrush background;
        SolidColorBrush foreground;

        public string Name => "Rectangle";
        public bool ShiftPressed { get; set; } = false;

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

            if (ShiftPressed)
            {
                double size = Math.Max(Math.Abs(_rightBottom.X - _topLeft.X), Math.Abs(_rightBottom.Y - _topLeft.Y));
                shape = new Rectangle()
                {
                    Width = size,
                    Height = size,
                    StrokeThickness = this.thickness,
                    Stroke = brush
                };
            }
            else
            {
                shape = new Rectangle() { 
                    Width = Math.Abs(_rightBottom.X - _topLeft.X),
                    Height = Math.Abs(_rightBottom.Y - _topLeft.Y),
                    StrokeThickness = this.thickness,
                    Stroke = brush
                };
            }

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

            if (ShiftPressed)
            {
                double size = Math.Max(Math.Abs(_rightBottom.X - _topLeft.X), Math.Abs(_rightBottom.Y - _topLeft.Y));
                shape.Width = size;
                shape.Height = size;
            }
            else
            {
                shape.Width = Math.Abs(_rightBottom.X - _topLeft.X);
                shape.Height = Math.Abs(_rightBottom.Y - _topLeft.Y);
            }

            Canvas.SetLeft(shape, _topLeft.X);
            Canvas.SetTop(shape, _topLeft.Y);

            if (textWrap != null)
            {
                if (ShiftPressed)
                {
                    double size = Math.Max(Math.Abs(_rightBottom.X - _topLeft.X), Math.Abs(_rightBottom.Y - _topLeft.Y));
                    textWrap.Width = size - thickness * 2 > 0 ? size - thickness * 2 : 50;
                    textWrap.Height = size - thickness * 2 > 0 ? size - thickness * 2 : 50;
                }
                else
                {
                    textWrap.Width = Math.Abs(_rightBottom.X - _topLeft.X) - thickness * 2 > 0 ? Math.Abs(_rightBottom.X - _topLeft.X) - thickness * 2 : 50;
                    textWrap.Height = Math.Abs(_rightBottom.Y - _topLeft.Y) - thickness * 2 > 0 ? Math.Abs(_rightBottom.Y - _topLeft.Y) - thickness * 2 : 50;
                }
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
            if (shape == null)
                return shape;
            return null;
        }

        public void AddRotation(double deg)
        {
            this.rotateDeg = deg;
            RotateTransform rotateTransform = new RotateTransform(this.rotateDeg, shape.Width / 2, shape.Height / 2);
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

            if (textWrap == null)
            {
                textWrap = new Border();
                textWrap.BorderThickness = new Thickness(0);
                textWrap.Background = Brushes.Red;

                if (ShiftPressed)
                {
                    double wrapSize = Math.Max(Math.Abs(_rightBottom.X - _topLeft.X), Math.Abs(_rightBottom.Y - _topLeft.Y));
                    textWrap.Width = wrapSize - thickness * 2 > 0 ? wrapSize - thickness * 2 : 50;
                    textWrap.Height = wrapSize - thickness * 2 > 0 ? wrapSize - thickness * 2 : 50;
                }
                else
                {
                    textWrap.Width = Math.Abs(_rightBottom.X - _topLeft.X) - thickness * 2 > 0 ? Math.Abs(_rightBottom.X - _topLeft.X) - thickness * 2 : 50;
                    textWrap.Height = Math.Abs(_rightBottom.Y - _topLeft.Y) - thickness * 2 > 0 ? Math.Abs(_rightBottom.Y - _topLeft.Y) - thickness * 2 : 50;
                }

                textBlock = new TextBlock();
                textBlock.TextWrapping = TextWrapping.Wrap;
                textBlock.TextAlignment = TextAlignment.Center;
                textBlock.VerticalAlignment = VerticalAlignment.Center;
                textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                textWrap.Child = textBlock;

                Canvas.SetLeft(textWrap, _topLeft.X + this.thickness);
                Canvas.SetTop(textWrap, _topLeft.Y + this.thickness);
            }

            textBlock.Text = text;
            textBlock.FontFamily = new FontFamily(font);
            textBlock.Foreground = foreground;
            textBlock.Background = background;
            textBlock.FontSize = size;

            if (rotateDeg != null)
            {
                RotateTransform textRotateTransform = new RotateTransform(this.rotateDeg, textWrap.ActualWidth / 2, textWrap.ActualHeight / 2);
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

        public Object[] GetProperty()
        {
            if (textBlock != null)
                return [Name, ShiftPressed, _topLeft, _rightBottom, style, thickness, brush, rotateDeg, true, textBlock.FontFamily.Source, background, foreground, textBlock.FontSize, textBlock.Text];
            else
                return [Name, ShiftPressed, _topLeft, _rightBottom, style, thickness, brush, rotateDeg, false];
        }
    }
}