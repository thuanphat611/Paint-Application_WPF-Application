
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
        DoubleCollection style;
        double rotateDeg;

        public string Name => "Pentagon";
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

            if (this.style != null)
            {
                shape.StrokeDashArray = this.style;
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
            double centerX = shape.Points.Average(p => p.X);
            double centerY = shape.Points.Average(p => p.Y);

            RotateTransform rotateTransform = new RotateTransform(this.rotateDeg, centerX, centerY);
            shape.RenderTransform = rotateTransform;
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
    }
}
