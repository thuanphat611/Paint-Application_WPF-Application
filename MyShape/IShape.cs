using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Shapes
{
    public interface IShape : ICloneable
    {
        bool ShiftPressed { get; set; }
        string Name { get; }
        void AddPoints(Point point1, Point point2);
        UIElement Convert(DoubleCollection style, double thickness, SolidColorBrush color);
        UIElement GetShape();
        void UpdateShape(Point point1, Point point2);
        void AddRotation(double deg);
        double GetRotationDeg();
        Point[] GetPoints();
    }

}