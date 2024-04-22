
using Shapes;
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
        DoubleCollection style;
        double rotateDeg;

        public string Name => "Star";
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

            PointCollection points = new PointCollection();
            double outerRadius = Math.Max(Math.Abs(_rightBottom.X - _topLeft.X), Math.Abs(_rightBottom.Y - _topLeft.Y)) / 2;
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
