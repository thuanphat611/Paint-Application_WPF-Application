
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using Shapes;

namespace MyEllipse
{
    public class MyEllipse : IShape
    {
        private Point _topLeft;
        private Point _rightBottom;
        Ellipse shape;
        double thickness;
        SolidColorBrush brush;
        DoubleCollection style;
        double rotateDeg;

        public string Name => "Ellipse";
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
                shape = new Ellipse()
                {
                    Width = size,
                    Height = size,
                    StrokeThickness = this.thickness,
                    Stroke = brush
                };
            }
            else
            {
                shape = new Ellipse()
                {
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
        }

        public double GetRotationDeg()
        {
            if (rotateDeg != null)
                return rotateDeg;
            else return 0;
        }
    }
}
