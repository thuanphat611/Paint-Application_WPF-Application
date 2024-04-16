
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

        public string Name => "Rectangle";
        
        public void AddPoints(Point point1, Point point2)
        {
            _topLeft = new Point(point1.X < point2.X ? point1.X : point2.X, point1.Y < point2.Y ? point1.Y : point2.Y);
            _rightBottom = new Point(point1.X > point2.X ? point1.X : point2.X, point1.Y > point2.Y ? point1.Y : point2.Y);
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public UIElement Convert(double thickness, SolidColorBrush color)
        {
            brush = color;
            this.thickness = thickness;
            shape = new Rectangle() { 
                Width = Math.Abs(_rightBottom.X - _topLeft.X),
                Height = Math.Abs(_rightBottom.Y - _topLeft.Y),
                StrokeThickness = this.thickness,
                Stroke = brush
            };

            Canvas.SetLeft(shape, _topLeft.X);
            Canvas.SetTop(shape, _topLeft.Y);
            return shape;
        }

        public void UpdateShape(Point point1, Point point2)
        {
            AddPoints(point1, point2);
            shape.Width = Math.Abs(_rightBottom.X - _topLeft.X);
            shape.Height = Math.Abs(_rightBottom.Y - _topLeft.Y);
            Canvas.SetLeft(shape, _topLeft.X);
            Canvas.SetTop(shape, _topLeft.Y);
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
