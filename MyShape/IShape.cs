using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Shapes
{
    public interface IShape : ICloneable
    {
        bool ShiftPressed { get; set; }
        string Layer { get; set; }
        string Name { get; }
        void AddPoints(Point point1, Point point2);
        UIElement Convert(int style, double thickness, SolidColorBrush color);
        UIElement GetShape();
        void UpdateShape(Point point1, Point point2);
        void AddRotation(double deg);
        double GetRotationDeg();
        Point[] GetPoints();
        void SetText(string font, SolidColorBrush background, SolidColorBrush foreground, double size, string text);
        Border GetText();
        Object[] GetProperty();
    }

}