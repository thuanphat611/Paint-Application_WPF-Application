using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Shapes
{
    public interface IShape : ICloneable
    {
        bool ShiftPressed { get; set; }
        void AddPoints(Point point1, Point point2);
        UIElement Convert(DoubleCollection style, double thickness, SolidColorBrush color);
        UIElement GetShape();
        string Name { get; }
        void UpdateShape(Point point1, Point point2);
        void AddRotation(double deg);
        double GetRotationDeg();
    }

}