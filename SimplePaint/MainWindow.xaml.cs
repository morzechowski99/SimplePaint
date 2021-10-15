using SimplePaint.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimplePaint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CommandEnum selecteedCommand = CommandEnum.NoAction;
        private Shape newShape, selectedShape;
        private SolidColorBrush brush = new SolidColorBrush() { Color = Colors.Blue };
        private Point startPoint;
        private double startMovingTop, startMovingLeft;
        private double startWidth;
        private double startHeight;
        private Point startLineMoving1;
        private Point startLineMoving2;
        private LineEnds lineEndSelected = LineEnds.NoSelected;
        private ReactangleAndEllipseEnds rectangleOrEllipseSelectedEnd = ReactangleAndEllipseEnds.NoSelected;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SetAction(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            selecteedCommand = btn.Name switch
            {
                var s when s == "RectangleBtn" => CommandEnum.DrawReactangle,
                var s when s == "CircleBtn" => CommandEnum.DrawCircle,
                var s when s == "LineBtn" => CommandEnum.DrawLine,
                var s when s == "MoveBtn" => CommandEnum.Move,
                var s when s == "ReshapeByKeyboardBtn" => CommandEnum.ReshapeByKeyboard,
                var s when s == "ReshapeByMouseBtn" => CommandEnum.ReshapeByMouse,
                _ => CommandEnum.NoAction
            };

            ChangeMenu(selecteedCommand);

        }

        private void ChangeMenu(CommandEnum selecteedCommand)
        {
            ActionType.Text = Dictionaries.Dictionaries.ActionTypesDictionary[selecteedCommand];
            switch (selecteedCommand)
            {
                case CommandEnum.NoAction:
                case CommandEnum.Move:
                case CommandEnum.ReshapeByMouse:
                case CommandEnum.ReshapeByKeyboard:
                    X.IsEnabled = false;
                    Y.IsEnabled = false;
                    X2.IsEnabled = false;
                    Y2.IsEnabled = false;
                    Diameter.IsEnabled = false;
                    break;
                case CommandEnum.DrawCircle:
                    X.IsEnabled = true;
                    Y.IsEnabled = true;
                    X2.IsEnabled = false;
                    Y2.IsEnabled = false;
                    Diameter.IsEnabled = true;
                    break;
                case CommandEnum.DrawLine:
                    X.IsEnabled = true;
                    Y.IsEnabled = true;
                    X2.IsEnabled = true;
                    Y2.IsEnabled = true;
                    Diameter.IsEnabled = false;
                    break;
                case CommandEnum.DrawReactangle:
                    X.IsEnabled = true;
                    Y.IsEnabled = true;
                    X2.IsEnabled = true;
                    Y2.IsEnabled = true;
                    Diameter.IsEnabled = false;
                    break;


            };
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.Captured == canvas && newShape != null)
            {
                Point pt = e.GetPosition(canvas);
                var TopBottom = startPoint.Y;
                var RightLeft = startPoint.X;
                switch (newShape)
                {
                    case Line l:
                        l.X2 = pt.X;
                        l.Y2 = pt.Y;
                        break;
                    default:
                        if (pt.Y - TopBottom >= 0)
                        {
                            newShape.SetValue(Canvas.TopProperty, TopBottom);
                            newShape.Height = pt.Y - TopBottom;
                        }
                        else
                        {
                            newShape.SetValue(Canvas.TopProperty, pt.Y);
                            newShape.Height = TopBottom - pt.Y;
                        }
                        if (pt.X - RightLeft >= 0)
                        {
                            newShape.SetValue(Canvas.LeftProperty, RightLeft);
                            newShape.Width = pt.X - RightLeft;
                        }
                        else
                        {
                            newShape.SetValue(Canvas.LeftProperty, pt.X);
                            newShape.Width = RightLeft - pt.X;
                        }
                        break;

                };

            }
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((int)selecteedCommand > (int)CommandEnum.DrawLine)
                return;
            Mouse.Capture(canvas);
            Point pt = e.GetPosition(canvas);
            newShape = selecteedCommand switch
            {
                var c when c == CommandEnum.DrawCircle => new Ellipse() { Stretch = Stretch.Uniform, Height = 0, Width = 0 },
                var c when c == CommandEnum.DrawLine => new Line() { X1 = pt.X, Y1 = pt.Y, X2 = pt.X, Y2 = pt.Y },
                var c when c == CommandEnum.DrawReactangle => new Rectangle() { Height = 0, Width = 0 },
                _ => null
            };
            if (newShape == null)
                return;
            newShape.Stroke = brush;
            newShape.StrokeThickness = 7;
            newShape.MouseLeftButtonUp += new MouseButtonEventHandler(Shape_MouseLeftButtonUp);
            newShape.MouseLeftButtonDown += new MouseButtonEventHandler(Shape_MouseLeftButtonDown);
            newShape.MouseMove += new MouseEventHandler(Shape_MouseMove);
            canvas.Children.Add(newShape);
            startPoint = pt;
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            newShape = null;
            if (selecteedCommand != CommandEnum.ReshapeByKeyboard)
                selectedShape = null;
        }

        private void Draw(object sender, RoutedEventArgs e)
        {
            Shape shape = GetShape();
            if (shape == null)
                return;

            shape.Stroke = brush;
            shape.StrokeThickness = 7;
            shape.MouseLeftButtonUp += new MouseButtonEventHandler(Shape_MouseLeftButtonUp);
            shape.MouseLeftButtonDown += new MouseButtonEventHandler(Shape_MouseLeftButtonDown);
            shape.MouseMove += new MouseEventHandler(Shape_MouseMove);
            canvas.Children.Add(shape);
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
        }

        private Shape GetShape()
        {
            double d;
            double x;
            double y;
            double x2;
            double y2;
            switch (selecteedCommand)
            {
                case CommandEnum.NoAction:
                    return null;
                case CommandEnum.DrawCircle:
                    if (!double.TryParse(Diameter.Text, out d) || !double.TryParse(X.Text, out x) || !double.TryParse(Y.Text, out y))
                    {
                        MessageBox.Show("Podaj poprawne wartośći", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                    }
                    Ellipse el = new Ellipse();
                    el.Height = d;
                    el.Width = d;
                    el.SetValue(Canvas.LeftProperty, x - d / 2);
                    el.SetValue(Canvas.TopProperty, y - d / 2);
                    return el;
                case CommandEnum.DrawLine:
                    if (!double.TryParse(X2.Text, out x2) || !double.TryParse(X.Text, out x) || !double.TryParse(Y.Text, out y) || !double.TryParse(Y2.Text, out y2))
                    {
                        MessageBox.Show("Podaj poprawne wartośći", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                    }
                    Line line = new Line() { X1 = x, Y1 = y, X2 = x2, Y2 = y2 };
                    return line;
                case CommandEnum.DrawReactangle:
                    if (!double.TryParse(X2.Text, out x2) || !double.TryParse(X.Text, out x) || !double.TryParse(Y.Text, out y) || !double.TryParse(Y2.Text, out y2))
                    {
                        MessageBox.Show("Podaj poprawne wartośći", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                    }
                    Rectangle rectangle = new Rectangle();
                    rectangle.SetValue(Canvas.TopProperty, y > y2 ? y2 : y);
                    rectangle.SetValue(Canvas.LeftProperty, x > x2 ? x2 : x);
                    rectangle.Width = Math.Abs(x - x2);
                    rectangle.Height = Math.Abs(y - y2);
                    return rectangle;

            }
            return null;
        }

        private void Shape_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            if (selecteedCommand != CommandEnum.ReshapeByKeyboard)
                selectedShape = null;
            else
                EnableInputs();
        }

        private void EnableInputs()
        {
            ScaleBtn.IsEnabled = true;
            double x1, x2, y1, y2;
            switch (selectedShape)
            {

                case Ellipse el:
                    X.IsEnabled = true;
                    X.Text = ((el.GetValue(Canvas.LeftProperty) as double?).Value + el.Width / 2).ToString();
                    Y.IsEnabled = true;
                    Y.Text = ((el.GetValue(Canvas.TopProperty) as double?).Value + el.Height / 2).ToString();
                    X2.IsEnabled = false;
                    Y2.IsEnabled = false;
                    Diameter.IsEnabled = true;
                    Diameter.Text = el.Width.ToString();
                    break;
                case Line l:
                    x1 = (l.GetValue(Line.X1Property) as double?).Value;
                    x2 = (l.GetValue(Line.X2Property) as double?).Value;
                    y1 = (l.GetValue(Line.Y1Property) as double?).Value;
                    y2 = (l.GetValue(Line.Y2Property) as double?).Value;
                    X.IsEnabled = true;
                    X.Text = x1.ToString();
                    Y.IsEnabled = true;
                    Y.Text = y1.ToString();
                    X2.IsEnabled = true;
                    X2.Text = x2.ToString();
                    Y2.IsEnabled = true;
                    Y2.Text = y2.ToString();
                    Diameter.IsEnabled = false;
                    break;
                case Rectangle r:
                    x1 = (r.GetValue(Canvas.LeftProperty) as double?).Value;
                    y1 = (r.GetValue(Canvas.TopProperty) as double?).Value;
                    X.IsEnabled = true;
                    X.Text = x1.ToString();
                    Y.IsEnabled = true;
                    Y.Text = y1.ToString();
                    X2.IsEnabled = true;
                    X2.Text = (x1 + r.Width).ToString();
                    Y2.IsEnabled = true;
                    Y2.Text = (y1 + r.Height).ToString();
                    Diameter.IsEnabled = false;
                    break;
            }
        }

        private void Scale(object sender, RoutedEventArgs e)
        {
            if (selectedShape == null)
                return;

            if (!Validate(out double x1, out double x2, out double y1, out double y2, out double d))
                return;

            Reshape(x1, x2, y1, y2, d);

            ((Button)sender).IsEnabled = false;
            selecteedCommand = CommandEnum.NoAction;
            ChangeMenu(selecteedCommand);

        }

        private void Reshape(double x1, double x2, double y1, double y2, double d)
        {
            switch (selectedShape)
            {
                case Ellipse el:
                    el.Height = d;
                    el.Width = d;
                    el.SetValue(Canvas.LeftProperty, x1 - d / 2);
                    el.SetValue(Canvas.TopProperty, y1 - d / 2);
                    return;
                case Line l:
                    l.SetValue(Line.X1Property, x1);
                    l.SetValue(Line.X2Property, x2);
                    l.SetValue(Line.Y1Property, y1);
                    l.SetValue(Line.Y2Property, y2);
                    return;
                case Rectangle rectangle:
                    rectangle.SetValue(Canvas.TopProperty, y1 > y2 ? y2 : y1);
                    rectangle.SetValue(Canvas.LeftProperty, x1 > x2 ? x2 : x1);
                    rectangle.Width = Math.Abs(x1 - x2);
                    rectangle.Height = Math.Abs(y1 - y2);
                    return;

            }
        }

        private bool Validate(out double x1, out double x2, out double y1, out double y2, out double d)
        {
            x1 = x2 = y1 = y2 = d = 0.0;
            switch (selectedShape)
            {
                case Line l:
                case Rectangle r:
                    if (!double.TryParse(X2.Text, out x2) ||
                        !double.TryParse(X.Text, out x1) ||
                        !double.TryParse(Y.Text, out y1) ||
                        !double.TryParse(Y2.Text, out y2))
                    {
                        MessageBox.Show("Podaj poprawne wartośći", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    break;
                case Ellipse el:
                    if (!double.TryParse(Y.Text, out y1) ||
                        !double.TryParse(X.Text, out x1) ||
                        !double.TryParse(Diameter.Text, out d))
                    {
                        MessageBox.Show("Podaj poprawne wartośći", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    break;
            }
            return true;
        }

        private void Shape_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (selecteedCommand < CommandEnum.Move)
                return;
            var elem = (Shape)sender;
            elem.CaptureMouse();
            selectedShape = sender as Shape;
            Point pt = e.GetPosition(canvas);
            startPoint = pt;
            if (selecteedCommand == CommandEnum.Move)
            {
                SetStartMovingParameters();
            }
            else if (selecteedCommand == CommandEnum.ReshapeByMouse)
            {
                DetectNearestShapeEnd(pt);
            }

        }

        private void DetectNearestShapeEnd(Point pt)
        {
            switch (selectedShape)
            {
                case Line l:
                    var d1 = Math.Sqrt(Math.Pow(pt.X - l.X1, 2) + Math.Pow(pt.Y - l.Y1, 2));
                    var d2 = Math.Sqrt(Math.Pow(pt.X - l.X2, 2) + Math.Pow(pt.Y - l.Y2, 2));
                    if (d1 < d2)
                        lineEndSelected = LineEnds.First;
                    else
                        lineEndSelected = LineEnds.Second;
                    break;
                case Rectangle r:
                case Ellipse el:
                    var top = (selectedShape.GetValue(Canvas.TopProperty) as double?).Value;
                    var left = (selectedShape.GetValue(Canvas.LeftProperty) as double?).Value;
                    var d_top = Math.Abs(pt.Y - top);
                    var d_bottom = Math.Abs(pt.Y - (top + selectedShape.Height));;
                    var d_left = Math.Abs(pt.X - left);
                    var d_right = Math.Abs(pt.X - (left + selectedShape.Width));
                    var min = new double[] { d_bottom, d_left, d_right, d_top }.OrderBy(x => x).First();
                    if (min == d_top)
                        rectangleOrEllipseSelectedEnd = ReactangleAndEllipseEnds.Top;
                    else if (min == d_bottom)
                        rectangleOrEllipseSelectedEnd = ReactangleAndEllipseEnds.Bottom;
                    else if (min == d_left)
                        rectangleOrEllipseSelectedEnd = ReactangleAndEllipseEnds.Left;
                    else if (min == d_right)
                        rectangleOrEllipseSelectedEnd = ReactangleAndEllipseEnds.Rigth;
                    SetStartMovingParameters();
                    break;
            }
        }

        private void SetStartMovingParameters()
        {
            switch (selectedShape)
            {
                case Line n:
                    var x1 = (n.GetValue(Line.X1Property) as double?).Value;
                    var x2 = (n.GetValue(Line.X2Property) as double?).Value;
                    var y1 = (n.GetValue(Line.Y1Property) as double?).Value;
                    var y2 = (n.GetValue(Line.Y2Property) as double?).Value;
                    startLineMoving1 = new Point(x1, y1);
                    startLineMoving2 = new Point(x2, y2);
                    break;
                default:
                    startMovingTop = (selectedShape.GetValue(Canvas.TopProperty) as double?).Value;
                    startMovingLeft = (selectedShape.GetValue(Canvas.LeftProperty) as double?).Value;
                    startWidth = selectedShape.Width;
                    startHeight = selectedShape.Height;
                    break;
            }
        }

        private void Shape_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectedShape == null || Mouse.Captured != sender)
                return;


            Point pt = e.GetPosition(canvas);
            if (selecteedCommand == CommandEnum.Move)
            {
                double deltaX = pt.X - startPoint.X;
                double deltaY = pt.Y - startPoint.Y;
                switch (selectedShape)
                {
                    case Line l:
                        l.SetValue(Line.X1Property, startLineMoving1.X + deltaX);
                        l.SetValue(Line.X2Property, startLineMoving2.X + deltaX);
                        l.SetValue(Line.Y1Property, startLineMoving1.Y + deltaY);
                        l.SetValue(Line.Y2Property, startLineMoving2.Y + deltaY);
                        break;
                    default:
                        selectedShape.SetValue(Canvas.LeftProperty, startMovingLeft + deltaX);
                        selectedShape.SetValue(Canvas.TopProperty, startMovingTop + deltaY);
                        break;
                }
            }
            else if (selecteedCommand == CommandEnum.ReshapeByMouse)
                switch (selectedShape)
                {
                    case Line l:
                        if (lineEndSelected == LineEnds.First)
                        {
                            l.SetValue(Line.X1Property, pt.X);
                            l.SetValue(Line.Y1Property, pt.Y);
                        }
                        else
                        {
                            l.SetValue(Line.X2Property, pt.X);
                            l.SetValue(Line.Y2Property, pt.Y);
                        }  
                        break;
                    case Rectangle r:
                        switch (rectangleOrEllipseSelectedEnd)
                        {
                            case ReactangleAndEllipseEnds.Left:
                                {
                                    selectedShape.SetValue(Canvas.LeftProperty, pt.X);
                                    var width = startWidth + (startMovingLeft - pt.X);
                                    selectedShape.Width = width > 0 ? width : 0;
                                    break;
                                }
                            case ReactangleAndEllipseEnds.Rigth:
                                {
                                    var width = startWidth + (pt.X - (startMovingLeft + startWidth));
                                    selectedShape.Width = width > 0 ? width : 0;
                                    break;
                                }
                            case ReactangleAndEllipseEnds.Top:
                                {
                                    selectedShape.SetValue(Canvas.TopProperty, pt.Y);
                                    var height = startHeight + (startMovingTop - pt.Y);
                                    selectedShape.Height = height > 0 ? height : 0;
                                    break;
                                }
                            case ReactangleAndEllipseEnds.Bottom:
                                {
                                    var height = startHeight + (pt.Y - (startMovingTop + startHeight));
                                    selectedShape.Height = height > 0 ? height : 0;
                                    break;
                                }
                        }
                        break;
                    case Ellipse el:
                        switch (rectangleOrEllipseSelectedEnd)
                        {
                            case ReactangleAndEllipseEnds.Left:
                                {
                                    selectedShape.SetValue(Canvas.LeftProperty, pt.X);
                                    var width = startWidth + (startMovingLeft - pt.X);
                                    selectedShape.Width = width > 0 ? width : 0;
                                    selectedShape.Height = selectedShape.Width;
                                    break;
                                }
                            case ReactangleAndEllipseEnds.Rigth:
                                {
                                    var width = startWidth + (pt.X - (startMovingLeft + startWidth));
                                    selectedShape.Width = width > 0 ? width : 0;
                                    selectedShape.Height = selectedShape.Width;
                                    break;
                                }
                            case ReactangleAndEllipseEnds.Top:
                                {
                                    selectedShape.SetValue(Canvas.TopProperty, pt.Y);
                                    var height = startHeight + (startMovingTop - pt.Y);
                                    selectedShape.Height = height > 0 ? height : 0;
                                    selectedShape.Width = selectedShape.Height;
                                    break;
                                }
                            case ReactangleAndEllipseEnds.Bottom:
                                {
                                    var height = startHeight + (pt.Y - (startMovingTop + startHeight));
                                    selectedShape.Height = height > 0 ? height : 0;
                                    selectedShape.Width = selectedShape.Height;
                                    break;
                                }
                        }
                        break;
                }

        }
    }
}
