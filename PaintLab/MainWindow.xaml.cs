using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
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
using Emgu.CV;
using static System.Net.Mime.MediaTypeNames;
using Emgu.CV.Structure;
using System.IO;
using Emgu.CV.Reg;
using System.Windows.Media.Media3D;

namespace PaintLab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Point currentPoint = new Point();
        public MainWindow()
        {
            InitializeComponent();
            
        }
        private Filtry filtry = new Filtry();
        private System.Windows.Shapes.Ellipse ellipseMain;
        private Rectangle rectangleMain;
        private Line lineMain; //Do rysowania krzywej
        private List<BitmapImage> sourceBitmap = new List<BitmapImage>(); //Do uploadowania zdjec
        private int selectedImage = 0;
        int drawStyle = 0;
        private Rectangle eraserRectangle = null;
        Color selectColor = Color.FromRgb(100, 100, 100);

        private List<Double> points = new List<Double>();
        private System.Windows.Shapes.Ellipse DrawPoint(double x, double y, Color color, double size)
        {
            System.Windows.Shapes.Ellipse ellipse = new System.Windows.Shapes.Ellipse();
            ellipse.Width = size;
            ellipse.Height = size;


            Canvas.SetTop(ellipse, y - ellipse.Height / 2);
            Canvas.SetLeft(ellipse, x - ellipse.Width / 2);
            Brush brushColor = new SolidColorBrush(selectColor);
            ellipse.Fill = brushColor;

            Obszar_roboczy.Children.Add(ellipse);
            return ellipse;
        }
        private System.Windows.Shapes.Ellipse DrawElipse(double sizeX, double sizeY, double x, double y, Color color)
        {
            System.Windows.Shapes.Ellipse ellipse = new System.Windows.Shapes.Ellipse();
            ellipse.Width = sizeX;
            ellipse.Height = sizeY;

            Canvas.SetTop(ellipse, y - ellipse.Height / 2);
            Canvas.SetLeft(ellipse, x - ellipse.Width / 2);

            Brush brushColor = new SolidColorBrush(selectColor);
            ellipse.Stroke = brushColor;
            // Obszar_roboczy.Children.Add(ellipse);
            ellipseMain = ellipse;
            return ellipse;
        }
        private Line DrawLine(double size, double x1, double y1, double x2, double y2, Color color)
        {
            Line line = new Line();

            line.Stroke = SystemColors.WindowFrameBrush;
            line.X1 = x1;
            line.Y1 = y1;
            line.X2 = x2;
            line.Y2 = y2;
            Obszar_roboczy.Children.Add(line);
            return line;
        }

        private Rectangle DrawRectangle(double size, double x, double y)
        {
            Rectangle rectangle = new Rectangle();
            rectangle.Width = x;
            rectangle.Height = y;

            Canvas.SetLeft(rectangle, x);
            Canvas.SetTop(rectangle, y);
            Brush brushColor = new SolidColorBrush(selectColor);
            rectangle.Stroke = brushColor;
            rectangleMain = rectangle;
            return rectangle;
        }
        private List<Line> linesCollection = new List<Line>();
        private Line DrawStaticLine(double size, double x1, double y1, double x2, double y2, Color color)
        {
            Line line = new Line();
            //Dodanie przeźroczystych punktów. Dzięki nim można przesuwać linie
            //Ellipse point1Vanish = DrawPoint(x1, y1, Colors.Red, 5);
            //Ellipse point2Vanish = DrawPoint(x2, y2, Colors.Red, 5);
            Brush brushColor = new SolidColorBrush(selectColor);
            line.Stroke = brushColor;
            line.X1 = x1;
            line.Y1 = y1;
            line.X2 = x2;
            line.Y2 = y2;
            lineMain = line;
            Obszar_roboczy.Children.Add(line);
            return line;
        }

        //Dodatkowe funkcje
        private void CreateRomb(double mouseX, double mouseY, double size, double angle, Color color)
        {
            Line line = new Line(); //gora
            line.X1 = mouseX;
            line.Y1 = mouseY;
            line.X2 = mouseX + size;
            line.Y2 = mouseY;

            Line line2 = new Line(); //podstawa
            line2.X1 = line.X1 - angle;
            line2.Y1 = line.Y1 - size;
            line2.X2 = line.X2 - angle;
            line2.Y2 = line.Y2 - size;

            Line line3 = new Line(); //prawy bok
            line3.X1 = line2.X2;
            line3.Y1 = line2.Y2;
            line3.X2 = line.X2;
            line3.Y2 = line.Y2;

            Line line4 = new Line(); //lewy bok
            line4.X1 = line.X1;
            line4.Y1 = line.Y1;
            line4.X2 = line2.X1;
            line4.Y2 = line2.Y1;

            Brush brushColor = new SolidColorBrush(selectColor);
            line.Stroke = brushColor;
            line2.Stroke = brushColor;
            line3.Stroke = brushColor;
            line4.Stroke = brushColor;
            Obszar_roboczy.Children.Add(line);
            Obszar_roboczy.Children.Add(line2);
            Obszar_roboczy.Children.Add(line3);
            Obszar_roboczy.Children.Add(line4);
        }
        private void CreatePlus(double mouseX, double mouseY, double size, Color color)
        {
            List<Line> lines = new List<Line>();
            Line line1 = new Line();
            line1.X1 = mouseX;
            line1.Y1 = mouseY;
            line1.X2 = mouseX + size;
            line1.Y2 = mouseY;
            lines.Add(line1);

            Line line2 = new Line();
            line2.X1 = line1.X2;
            line2.Y1 = line1.Y2;
            line2.X2 = line1.X2;
            line2.Y2 = line1.Y2 - size;
            lines.Add(line2);

            Line line3 = new Line();
            line3.X1 = line2.X2;
            line3.Y1 = line2.Y2;
            line3.X2 = line2.X2 + size;
            line3.Y2 = line2.Y2;
            lines.Add(line3);

            Line line4 = new Line();
            line4.X1 = line3.X2;
            line4.Y1 = line3.Y2;
            line4.X2 = line3.X2;
            line4.Y2 = line3.Y2 - size;
            lines.Add(line4);

            Line line5 = new Line();
            line5.X1 = line4.X2;
            line5.Y1 = line4.Y2;
            line5.X2 = line4.X2 - size;
            line5.Y2 = line4.Y2;
            lines.Add(line5);

            Line line6 = new Line();
            line6.X1 = line5.X2;
            line6.Y1 = line5.Y2;
            line6.X2 = line5.X2;
            line6.Y2 = line5.Y2 - size;
            lines.Add(line6);

            Line line7 = new Line();
            line7.X1 = line6.X2;
            line7.Y1 = line6.Y2;
            line7.X2 = line6.X2 - size;
            line7.Y2 = line6.Y2;
            lines.Add(line7);

            Line line8 = new Line();
            line8.X1 = line7.X2;
            line8.Y1 = line7.Y2;
            line8.X2 = line7.X2;
            line8.Y2 = line7.Y2 + size;
            lines.Add(line8);

            Line line9 = new Line();
            line9.X1 = line8.X2;
            line9.Y1 = line8.Y2;
            line9.X2 = line8.X2 - size;
            line9.Y2 = line8.Y2;
            lines.Add(line9);

            Line line10 = new Line();
            line10.X1 = line9.X2;
            line10.Y1 = line9.Y2;
            line10.X2 = line9.X2;
            line10.Y2 = line9.Y2 + size;
            lines.Add(line10);

            Line line11 = new Line();
            line11.X1 = line10.X2;
            line11.Y1 = line10.Y2;
            line11.X2 = line10.X2 + size;
            line11.Y2 = line10.Y2;
            lines.Add(line11);

            Line line12 = new Line();
            line12.X1 = line11.X2;
            line12.Y1 = line11.Y2;
            line12.X2 = line11.X2;
            line12.Y2 = line11.Y2 + size;
            lines.Add(line12);
            
            Brush brushColor = new SolidColorBrush(selectColor);
            foreach (Line line in lines)
            {
                line.Stroke = brushColor;
                Obszar_roboczy.Children.Add(line);
            }
        }

        private void CreateImage(double mouseX, double mouseY)
        {
            System.Windows.Controls.Image pngImage = new System.Windows.Controls.Image();
            pngImage.Source = sourceBitmap[selectedImage];
            pngImage.Width = 100;
            pngImage.Height = 100;
            Canvas.SetLeft(pngImage, mouseX);
            Canvas.SetTop(pngImage, mouseY);
            Obszar_roboczy.Children.Add(pngImage);
        }
        private void Obszar_roboczy_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ButtonState == MouseButtonState.Pressed)
            {
                currentPoint = e.GetPosition(this);
            }
        }


        private void Obszar_roboczy_MouseMove(object sender, MouseEventArgs e)
        {
            if (drawStyle == 12 && e.LeftButton == MouseButtonState.Pressed)
            {
                if (e.Source is Rectangle eraserRectangle)
                {
                    double mouseX = e.GetPosition(this).X;
                    double mouseY = e.GetPosition(this).Y;
                    Canvas.SetLeft(eraserRectangle, mouseX - eraserRectangle.Width / 2);
                    Canvas.SetTop(eraserRectangle, mouseY - eraserRectangle.Height / 2);
                    Rect rect = new Rect(Canvas.GetLeft(eraserRectangle), Canvas.GetTop(eraserRectangle), eraserRectangle.Width, eraserRectangle.Height);
                    try
                    {
                        foreach (UIElement element in Obszar_roboczy.Children)
                        {
                            {
                                if (element is Line)
                                {
                                    var result = element as Line;
                                    if (element != eraserRectangle && (rect.IntersectsWith(new Rect(result.X1, result.Y1, 1, 1)) || rect.IntersectsWith(new Rect(result.X2, result.Y2, 1, 1)))) //: && IsPointInsideElement(new Point(mouseX, mouseY), element))
                                    {
                                        Obszar_roboczy.Children.Remove(element);
                                        break; // Usunięto pierwszy napotkany element, można przerwać pętlę
                                    }
                                }
                                else if (element is Polygon)
                                {
                                    var result = element as Polygon;
                                    if (element != eraserRectangle)
                                    {
                                        foreach (var pkt in result.Points)
                                        {
                                            if (rect.IntersectsWith(new Rect(pkt.X, pkt.Y, 1, 1)))
                                            {
                                                Obszar_roboczy.Children.Remove(element);
                                                break;
                                            }
                                        }
                                    }
                                }
                                else if (element is System.Windows.Shapes.Ellipse)
                                {
                                    var rectElement = element.PointToScreen(new Point(0, 0));
                                    double width = element.RenderSize.Width;
                                    double height = element.RenderSize.Height;
                                    if (element != eraserRectangle && rect.IntersectsWith(new Rect(Canvas.GetLeft(element), Canvas.GetTop(element), width, height))) //: && IsPointInsideElement(new Point(mouseX, mouseY), element))
                                    {
                                        Obszar_roboczy.Children.Remove(element);
                                        break; // Usunięto pierwszy napotkany element, można przerwać pętlę
                                    }
                                }
                                else
                                {
                                    double left = Canvas.GetLeft(element);
                                    double top = Canvas.GetTop(element);

                                    Vector vector = new Vector(left, top);
                                    if (element != eraserRectangle && rect.IntersectsWith(new Rect(left, top, element.RenderSize.Width, element.RenderSize.Height))) //: && IsPointInsideElement(new Point(mouseX, mouseY), element))
                                    {
                                        Obszar_roboczy.Children.Remove(element);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    } 
                }
            }
            else if (drawStyle == 12)
            {
                double mouseX = e.GetPosition(this).X;
                double mouseY = e.GetPosition(this).Y;
                Canvas.SetLeft(eraserRectangle, mouseX - eraserRectangle.Width / 2);
                Canvas.SetTop(eraserRectangle, mouseY - eraserRectangle.Height / 2);
            }
            else
            {
                Obszar_roboczy.Children.Remove(eraserRectangle);

            }

            if (e.LeftButton == MouseButtonState.Pressed && drawStyle == 1)
            {
                Line line = new Line();
                line.Stroke = new SolidColorBrush(selectColor);
                line.X1 = currentPoint.X;
                line.Y1 = currentPoint.Y;
                line.X2 = e.GetPosition(this).X;
                line.Y2 = e.GetPosition(this).Y;

                currentPoint = e.GetPosition(this);

                Obszar_roboczy.Children.Add(line);
            }
            if(e.LeftButton == MouseButtonState.Pressed && drawStyle == 6)
            {
                Obszar_roboczy.Children.Remove(rectangleMain);
                rectangleMain.Stroke = new SolidColorBrush(selectColor);
                rectangleMain.Width = Math.Abs(e.GetPosition(this).X - currentPoint.X);
                rectangleMain.Height = Math.Abs(e.GetPosition(this).Y - currentPoint.Y);
                Canvas.SetLeft(rectangleMain, e.GetPosition(this).X - rectangleMain.Width);
                Canvas.SetTop(rectangleMain, e.GetPosition(this).Y - rectangleMain.Height);
                Obszar_roboczy.Children.Add(rectangleMain);
            }
            if(e.LeftButton == MouseButtonState.Pressed && drawStyle == 7)
            {
                Obszar_roboczy.Children.Remove(ellipseMain);
                ellipseMain.Stroke = new SolidColorBrush(selectColor);
                ellipseMain.Width = Math.Abs(e.GetPosition(this).X - currentPoint.X);
                ellipseMain.Height = Math.Abs(e.GetPosition(this).Y - currentPoint.Y);
                Canvas.SetLeft(ellipseMain, e.GetPosition(this).X - ellipseMain.Width / 2);
                Canvas.SetTop(ellipseMain, e.GetPosition(this).Y - ellipseMain.Height / 2);
                Obszar_roboczy.Children.Add(ellipseMain);
            }
            
                
        }


        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            string name = "Kamil";
            string surname = "Grabowski";
            MessageBox.Show("Autor: " + name + " " + surname);
        }

        private double CalculateDistance(double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;

            return Math.Sqrt(dx * dx + dy * dy);
        }

        System.Windows.Media.PointCollection linePoints = new System.Windows.Media.PointCollection();
        double editLineX;
        double editLineY;
        bool durginEdit = false;
        private void Obszar_roboczy_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            double mouseX = e.GetPosition(this).X;
            double mouseY = e.GetPosition(this).Y;
            if (drawStyle == 2)
            {
                System.Windows.Shapes.Ellipse ellipse = DrawPoint(e.GetPosition(this).X, e.GetPosition(this).Y, Colors.Violet, 6);
            }
            if (drawStyle == 3)
            {
                linePoints.Add(new Point(mouseX, mouseY));
                if (linePoints.Count == 2)
                {
                    DrawStaticLine(16, linePoints[0].X, linePoints[0].Y, linePoints[1].X, linePoints[1].Y, Colors.Red);
                    linePoints.Clear();
                }
            }
            if(drawStyle == 4)
            {
                if (e.Source is Line lineRemove)
                {
                    if (CalculateDistance(lineRemove.X1, lineRemove.Y1, mouseX, mouseY) > CalculateDistance(lineRemove.X2, lineRemove.Y2, mouseX, mouseY))
                    {
                        editLineX = lineRemove.X1;
                        editLineY = lineRemove.Y1;
                    }
                    else
                    {
                        editLineX = lineRemove.X2;
                        editLineY = lineRemove.Y2;
                    }
                    durginEdit = true;
                    Obszar_roboczy.Children.Remove(lineRemove);
                }
                else if (durginEdit == true)
                {
                    DrawStaticLine(16, editLineX, editLineY, mouseX, mouseY, Colors.Violet);
                    durginEdit = false;
                }
            }
            if (drawStyle == 5)
            {
                Polygon poly = new Polygon();
                double polySize = 40;
                Point point1 = new Point(mouseX - polySize, mouseY + polySize * 2);
                Point point2 = new Point(mouseX + polySize, mouseY + polySize * 2);
                Point point3 = new Point(mouseX + 2 * polySize, mouseY);
                Point point4 = new Point(mouseX + polySize, mouseY - 2 * polySize);
                Point point5 = new Point(mouseX - polySize, mouseY - 2 * polySize);
                Point point6 = new Point(mouseX - 2 * polySize, mouseY);
                System.Windows.Media.PointCollection polyPoints = new System.Windows.Media.PointCollection();
                polyPoints.Add(point1);
                polyPoints.Add(point2);
                polyPoints.Add(point3);
                polyPoints.Add(point4);
                polyPoints.Add(point5);
                polyPoints.Add(point6);

                poly.Points = polyPoints;

                Brush brushColor = new SolidColorBrush(selectColor);
                poly.Stroke = brushColor;
                Obszar_roboczy.Children.Add(poly);
            }
            if (drawStyle == 6)
            {
                Rectangle rectangle = DrawRectangle(40, mouseX, mouseY);

            }
            if (drawStyle == 7)
            {
                System.Windows.Shapes.Ellipse elips = DrawElipse(0, 0, mouseX, mouseY, Colors.Violet);

            }
            if(drawStyle == 8)
            {
                if (lineMain == null)
                {
                    Line line = DrawStaticLine(6, mouseX, mouseY, mouseX, mouseY, Colors.Red);
                }
                else
                {
                    Line line = DrawStaticLine(6, mouseX, mouseY, lineMain.X1, lineMain.Y1, Colors.Red);
                }
            }
            else
            {
                lineMain = null;
            }
            if(drawStyle == 9)
            {
                CreateRomb(mouseX,mouseY, 80, -30, Colors.Violet);
            }
            if(drawStyle == 10)
            {
                CreatePlus(mouseX, mouseY, 25, Colors.BlueViolet);
            }
            if(drawStyle == 11)
            {
                CreateImage(mouseX, mouseY);
            }
        }


        private void DrawButton_Click(object sender, RoutedEventArgs e) //Rysowanie dowolne
        {
            drawStyle = 1;
        }

        private void DrawPoint_Click(object sender, RoutedEventArgs e) //Rysowanie punktow
        {
            drawStyle = 2;
        }

        private void DrawSegment_Click(object sender, RoutedEventArgs e)
        {
            drawStyle = 3;
        }

        private void EditSegment_Click(object sender, RoutedEventArgs e)
        {
            drawStyle = 4;   
        }

        //rysowanie wielokata
        private void DrawPolygon_Click(object sender, RoutedEventArgs e)
        {
            drawStyle = 5;
        }

        //Prostokat
        private void Prostokat_Click(object sender, RoutedEventArgs e)
        {
            drawStyle = 6;
        }

        //Elipsa, kolo
        private void Elipse_Click(object sender, RoutedEventArgs e)
        {
            drawStyle = 7;
        }


        //Łamana
        private void DrawLineLamana_Click(object sender, RoutedEventArgs e)
        {
            drawStyle = 8;
        }

        //Romb
        private void DrawRomb_Click(object sender, RoutedEventArgs e)
        {
            drawStyle = 9;
        }

        private void DrawPlus_Click(object sender, RoutedEventArgs e)
        {
            drawStyle = 10;
        }

        //Wstawianie obrazu 
        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            drawStyle = 11;
            var result = e.Source as MenuItem;
            selectedImage = Int32.Parse(result.DataContext.ToString()) - 3; //-3, bo miały być już 2 załadowane obrazy
        }

        private void ColorPicker_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowColorPicker window = new WindowColorPicker();
            var result = window.ShowDialog();
            if (result == true)
            {
                // Pobieranie zmiennej z drugiego okna po jego zamknięciu
                selectColor = window.color.Color;
                ColorPicker.Fill = window.color;
            }
        }

        private void Upload_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.FilterIndex = 1;
            if (openDialog.ShowDialog() == true)
            {
                BitmapImage bitmap = new BitmapImage(new Uri(openDialog.FileName));
                MenuItem newMenuItem = new MenuItem();
                newMenuItem.Header = openDialog.SafeFileName;
                
                newMenuItem.Click += ImageButton_Click;
                var sizeArray = sourceBitmap.Count + 3;
                newMenuItem.DataContext = sizeArray.ToString();
                AllImages.Items.Add(newMenuItem);

                sourceBitmap.Add(bitmap);
            }
            
        }

        private BitmapSource ConvertCanvasToBitmap(Canvas canvas)
        {
            // Utwórz obiekt RenderTargetBitmap o rozmiarach Canvas
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                (int)canvas.ActualWidth, (int)canvas.ActualHeight, 96, 96, PixelFormats.Pbgra32);

            // Renderuj zawartość Canvas na RenderTargetBitmap
            canvas.Measure(new Size(canvas.ActualWidth, canvas.ActualHeight));
            canvas.Arrange(new Rect(new Size(canvas.ActualWidth, canvas.ActualHeight)));
            renderBitmap.Render(canvas);

            // Konwertuj RenderTargetBitmap na BitmapSource
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                ms.Position = 0;

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();

                return bitmap;
            }
        }
        private BitmapSource ConvertCanvasToBitmapSource()
        {
            BitmapImage canvasImage = new BitmapImage();

            // Save current canvas transform
            Transform transform = Obszar_roboczy.LayoutTransform;
            // reset current transform (in case it is scaled or rotated)
            //Obszar_roboczy.LayoutTransform = null;

            // Get the size of canvas
            Size size = new Size(Obszar_roboczy.Width, Obszar_roboczy.Height);
            // Measure and arrange the surface
            // VERY IMPORTANT
            Obszar_roboczy.Measure(Obszar_roboczy.RenderSize);
            Obszar_roboczy.Arrange(new Rect(Obszar_roboczy.RenderSize));

            // Create a render bitmap and push the surface to it
            RenderTargetBitmap renderBitmap =
              new RenderTargetBitmap(
                (int)Obszar_roboczy.RenderSize.Width,
                (int)Obszar_roboczy.RenderSize.Height,
                96d,
                96d,
                PixelFormats.Pbgra32);
            renderBitmap.Render(Obszar_roboczy);
            return renderBitmap;
        }
        private void sobel_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource renderBitmap = ConvertCanvasToBitmapSource();
            System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            Canvas.SetLeft(image, 0);
            Canvas.SetTop(image, 0);
            double[,] sobelX = {
            { -1, 0, 1 },
            { -2, 0, 2 },
            { -1, 0, 1 }
        };

                double[,] sobelY = {
            { -1, -2, -1 },
            { 0, 0, 0 },
            { 1, 2, 1 }
        };

                BitmapSource filteredImage = filtry.ApplySobelFilter(renderBitmap, sobelX, sobelY);
                image.Source = filteredImage;
            Obszar_roboczy.Children.Add(image);
        }

        private void gauss_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource renderBitmap = ConvertCanvasToBitmapSource();
            System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            Canvas.SetLeft(image, 0);
            Canvas.SetTop(image, 0);
            // Przykładowy filtr Gaussa
            double[,] filterMatrix = {
            { 1, 2, 1 },
            { 2, 4, 2 },
            { 1, 2, 1 }
        };
            

            BitmapSource filteredImage = filtry.ApplyFilterGauss(renderBitmap, filterMatrix);
            image.Source = filteredImage;
            Obszar_roboczy.Children.Add(image);
        }
        private void sepia_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource renderBitmap = ConvertCanvasToBitmapSource();
            System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            Canvas.SetLeft(image, 0);
            Canvas.SetTop(image, 0);
            BitmapSource filteredImage = filtry.ApplySepiaFilter(renderBitmap);
            image.Source = filteredImage;
            Obszar_roboczy.Children.Add(image);
        }
        private void SaveAsJpg_Click(object sender, RoutedEventArgs e)
        {
            Uri path = null;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Image File (*.jpg|*.jpg";
            saveFileDialog.FilterIndex = 1;
            if (saveFileDialog.ShowDialog() == true)
            {
                path = new Uri(saveFileDialog.FileName);
            }

             if (path == null) return;

            Transform transform = Obszar_roboczy.LayoutTransform;
            Obszar_roboczy.LayoutTransform = null;

            Size size = new Size(Obszar_roboczy.ActualWidth, Obszar_roboczy.ActualHeight);

            Obszar_roboczy.Measure(size);
            Obszar_roboczy.Arrange(new Rect(size));

            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
                (int)size.Width,
                (int)size.Height,
                96d,
                96d,
                PixelFormats.Pbgra32
                );
            renderTargetBitmap.Render(Obszar_roboczy);

            using (FileStream outStream = new FileStream(path.LocalPath, FileMode.Create))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                encoder.Save(outStream);
            }
            Obszar_roboczy.LayoutTransform = transform;
        }

        private void SaveAsPng_Click(object sender, RoutedEventArgs e)
        {
            Uri path = null;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Image File (*.png|*.png";
            saveFileDialog.FilterIndex = 1;
            if (saveFileDialog.ShowDialog() == true)
            {
                path = new Uri(saveFileDialog.FileName);
            }

            if (path == null) return;

            Transform transform = Obszar_roboczy.LayoutTransform;
            Obszar_roboczy.LayoutTransform = null;

            Size size = new Size(Obszar_roboczy.ActualWidth, Obszar_roboczy.ActualHeight);

            Obszar_roboczy.Measure(size);
            Obszar_roboczy.Arrange(new Rect(size));

            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
                (int)size.Width,
                (int)size.Height,
                96d,
                96d,
                PixelFormats.Pbgra32
                );
            renderTargetBitmap.Render(Obszar_roboczy);

            using (FileStream outStream = new FileStream(path.LocalPath, FileMode.Create))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                encoder.Save(outStream);
            }
            Obszar_roboczy.LayoutTransform = transform;
        }

        
        private void Eraser_Click(object sender, RoutedEventArgs e)
        {
            drawStyle = 12;
            Rectangle tempEraser = new Rectangle();
            tempEraser.Width = 100;
            tempEraser.Height = 100;
            Brush brushColor = new SolidColorBrush(Colors.White);
            tempEraser.Stroke = brushColor;
            tempEraser.Fill = brushColor;
            eraserRectangle = tempEraser;
            Obszar_roboczy.Children.Add(eraserRectangle);

        }

        
    }
}