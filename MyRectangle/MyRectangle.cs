
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using Shapes;
using System.Windows.Media.Media3D;

namespace MyRectangle
{
    public class MyRectangle : IShape
    {
        private Point _topLeft;
        private Point _rightBottom;
        Rectangle shape;
        double thickness;
        SolidColorBrush brush;
        DoubleCollection style;
        double rotateDeg;
        Border textWrap;
        TextBlock textBlock;

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

        public UIElement Convert(DoubleCollection style, double thickness, SolidColorBrush color)
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

            if (this.style != null)
            {
                shape.StrokeDashArray = this.style;
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
            if (textWrap == null)
            {
                textWrap = new Border();
                textWrap.BorderThickness = new Thickness(0);

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
        }

        public Border GetText()
        {
            if (textWrap != null)
                return textWrap;
            else
                return null;
        }
    }
}