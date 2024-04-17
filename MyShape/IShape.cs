using System.Windows;
using System.Windows.Media;

namespace Shapes
{
    public interface IShape : ICloneable
    {
        bool ShiftPressed { get; set; }
        void AddPoints(Point point1, Point point2);
        UIElement Convert(double thickness, SolidColorBrush color);
        string Name { get; }
        void UpdateShape(Point point1, Point point2);
    }

}