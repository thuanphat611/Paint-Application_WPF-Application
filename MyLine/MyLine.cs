
using Shapes;
using System.Windows;
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
        DoubleCollection style;

        public string Name => "Line";
        public bool ShiftPressed { get; set; } = false;

        public void AddPoints(Point point1, Point point2)
        {
            _start = point1;
            _end = point2;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public Shape Convert(DoubleCollection style, double thickness, SolidColorBrush color)
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

            if (this.style != null)
            {
                shape.StrokeDashArray = this.style;
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
        }

        public override string ToString()
        {
            return Name;
        }

        public Shape ReCreateShape()
        {
            if (shape == null)
                return shape;
            return null;
        }
    }
}
