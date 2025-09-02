using System;
using System.Collections.Generic;
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
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using MyMVVM.Map.ViewModel;

namespace MyMVVM.Map.View
{
    /// <summary>
    /// MapView.xaml 的交互逻辑
    /// </summary>
    public partial class MapView : UserControl
    {
        public MapView()
        {
            InitializeComponent();
            _selectedTool = DrawingTool.Brush;
            this.DataContext = new MapViewModel();
        }
        public enum DrawingTool
        {
            Brush,
            Eraser,
            Rectangle,
            Ellipse,
            Line
        }
        private DrawingTool _selectedTool;
        private Point _startPoint;
        private Shape _currentShape;
        private void BrushButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedTool = DrawingTool.Brush;
        }

        private void EraserButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedTool = DrawingTool.Eraser;
        }

        private void RectangleButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedTool = DrawingTool.Rectangle;
        }

        private void EllipseButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedTool = DrawingTool.Ellipse;
        }

        private void LineButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedTool = DrawingTool.Line;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            DrawingCanvas.Children.Clear();
            DrawingCanvas.Background = Brushes.Transparent;
        }

        private void DrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(DrawingCanvas);

            switch (_selectedTool)
            {
                case DrawingTool.Brush:
                    _currentShape = new Ellipse
                    {
                        Width = 10,
                        Height = 10,
                        Fill = Brushes.Black
                    };
                    Canvas.SetLeft(_currentShape, _startPoint.X - 5);
                    Canvas.SetTop(_currentShape, _startPoint.Y - 5);
                    DrawingCanvas.Children.Add(_currentShape);
                    break;

                case DrawingTool.Rectangle:
                    _currentShape = new Rectangle
                    {
                        Stroke = Brushes.Black,
                        StrokeThickness = 2
                    };
                    Canvas.SetLeft(_currentShape, _startPoint.X);
                    Canvas.SetTop(_currentShape, _startPoint.Y);
                    DrawingCanvas.Children.Add(_currentShape);
                    break;

                case DrawingTool.Ellipse:
                    _currentShape = new Ellipse
                    {
                        Stroke = Brushes.Black,
                        StrokeThickness = 2
                    };
                    Canvas.SetLeft(_currentShape, _startPoint.X);
                    Canvas.SetTop(_currentShape, _startPoint.Y);
                    DrawingCanvas.Children.Add(_currentShape);
                    break;

                case DrawingTool.Eraser:
                    var hitShapes = DrawingCanvas.Children.OfType<UIElement>().Where(el => el is Shape && el.IsMouseOver).ToList();
                    foreach (var shape in hitShapes)
                    {
                        DrawingCanvas.Children.Remove(shape);
                    }
                    break;

                case DrawingTool.Line:
                    _currentShape = new Line
                    {
                        Stroke = Brushes.Black,
                        StrokeThickness = 2,
                        X1 = _startPoint.X,
                        Y1 = _startPoint.Y,
                    };
                    DrawingCanvas.Children.Add(_currentShape);
                    break;
            }
        }

        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_currentShape != null && e.LeftButton == MouseButtonState.Pressed)
            {
                Point endPoint = e.GetPosition(DrawingCanvas);

                switch (_selectedTool)
                {
                    case DrawingTool.Brush:
                        Line line = new Line
                        {
                            Stroke = Brushes.Black,
                            StrokeThickness = 2,
                            X1 = _startPoint.X,
                            Y1 = _startPoint.Y,
                            X2 = endPoint.X,
                            Y2 = endPoint.Y
                        };
                        DrawingCanvas.Children.Add(line);
                        _startPoint = endPoint;
                        break;

                    case DrawingTool.Rectangle:
                        Rectangle rect = _currentShape as Rectangle;
                        if (rect != null)
                        {
                            double minX = Math.Min(endPoint.X, _startPoint.X);
                            double minY = Math.Min(endPoint.Y, _startPoint.Y);
                            double maxX = Math.Max(endPoint.X, _startPoint.X);
                            double maxY = Math.Max(endPoint.Y, _startPoint.Y);
                            rect.Width = maxX - minX;
                            rect.Height = maxY - minY;
                            Canvas.SetLeft(rect, minX);
                            Canvas.SetTop(rect, minY);
                        }
                        break;

                    case DrawingTool.Ellipse:
                        Ellipse ellipse = _currentShape as Ellipse;
                        if (ellipse != null)
                        {
                            double minX = Math.Min(endPoint.X, _startPoint.X);
                            double minY = Math.Min(endPoint.Y, _startPoint.Y);
                            double maxX = Math.Max(endPoint.X, _startPoint.X);
                            double maxY = Math.Max(endPoint.Y, _startPoint.Y);
                            ellipse.Width = maxX - minX;
                            ellipse.Height = maxY - minY;
                            Canvas.SetLeft(ellipse, minX);
                            Canvas.SetTop(ellipse, minY);
                        }
                        break;

                    case DrawingTool.Line:
                        Line lineShape = _currentShape as Line;
                        if (lineShape != null)
                        {
                            lineShape.X2 = endPoint.X;
                            lineShape.Y2 = endPoint.Y;
                        }
                        break;
                }
            }
        }

        private void DrawingCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _currentShape = null;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PNG Image|*.png",
                Title = "Save the Drawing"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                RenderTargetBitmap rtb = new RenderTargetBitmap((int)DrawingCanvas.RenderSize.Width,
                    (int)DrawingCanvas.RenderSize.Height, 96d, 96d, PixelFormats.Default);
                rtb.Render(DrawingCanvas);
                PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(rtb));
                using (var fs = File.OpenWrite(saveFileDialog.FileName))
                {
                    pngEncoder.Save(fs);
                }
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "PNG Image|*.png",
                Title = "Load a Drawing"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                ImageBrush brush = new ImageBrush
                {
                    ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri(openFileDialog.FileName, UriKind.Relative)),
                    Stretch = Stretch.Uniform
                };
                DrawingCanvas.Background = brush;
            }
        }
    }
}

