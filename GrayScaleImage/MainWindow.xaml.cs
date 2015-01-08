using Microsoft.Win32;
using System;
using System.Threading;
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

namespace LassoTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ImGraph curg; //Current image graph

        //Image
        private BitmapImage originalImage;
        private byte[] originalImageBytes;
        int H, W;

        List<Line> way = new List<Line>(); // List of points on the way of mouse
        private Point prev, start; //Points for drawing way of mouse

        sbyte cond = 0; // Variable for tracking progress of marque
        
        Thread DjCount; // Thread for djikstra 
        System.Windows.Threading.DispatcherTimer Grloading; //Timer for loading progress

        public MainWindow()
        {
            InitializeComponent();
        }

        

        // Sets drawPanel to default condition
        private void SetByDef()
        {
            cond = 0;
            way.Clear();
            drawPanel.Children.Clear();
            drawPanel.Cursor = Cursors.Hand;
        }
       
        // open file from the finder
        private void openFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg)|*.jpg; *.jpeg";
            if (openFileDialog.ShowDialog() == true)
            {
                showImage(openFileDialog.FileName);
                sobelButton.IsEnabled = true;
                magicButton.IsEnabled = true;

                SetByDef();

                Procl.Content = "Choose type of action"; // Start text value
                Pbar.Value = 0; //Loading bar sets to zero
                cond = 0; // 0 step of marquee
            }
        }

        // show image on the window
        private void showImage(string filename)
        {
            originalImage = ImageConvertor.FilenameToImage(filename);
            originalImageBytes = ImageConvertor.ImageToByteArray(filename);
            originalPanel.Source = originalImage;
        }

        // click on processing buttons
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Setting height and width of original image
            W = originalImage.PixelWidth;
            H = originalImage.PixelHeight;

            string buttonName = (sender as Button).Content.ToString();
            ImageProcessing process = new ImageProcessing();
            byte[] processedImageBytes;
            switch (buttonName[0])
            {
                // if the button is "Sobel"
                case 'S':
                    processedImageBytes = process.setSobel(originalImageBytes, H, W);
                    proccessedPanel.Source = ImageConvertor.ByteArrayToImage(processedImageBytes, originalImage.PixelWidth, originalImage.PixelHeight, 1);

                    Procl.Content = "Choose type of action"; // Changing main text value

                    //Hide drawPanel
                    drawPanel.Visibility = Visibility.Hidden;
                    drawPanel.Height = 0;
                    break;

                // if the button is "Magic"
                case 'M':
                    processedImageBytes = process.setMagic(originalImageBytes, H, W, out curg);
                    proccessedPanel.Source = ImageConvertor.ByteArrayToImage(processedImageBytes, originalImage.PixelWidth, originalImage.PixelHeight, 4);

                    ShowDrPan();

                    Procl.Content = "Click on the start point of the marquee";
                    resetButton.IsEnabled = true;
                    break;
                // if the button is "Gray-scale"
                case 'R':
                    processedImageBytes = process.setMagic(originalImageBytes, H, W, out curg);
                    proccessedPanel.Source = ImageConvertor.ByteArrayToImage(processedImageBytes, originalImage.PixelWidth, originalImage.PixelHeight, 4);

                    ShowDrPan();

                    SetByDef();

                    Procl.Content = "Click on the start point of the marquee";
                    break;
                // if smth stupid happend
                default:
                    break;
            }
        }

        //Show drawPanel
        private void ShowDrPan()
        {
            drawPanel.Visibility = Visibility.Visible;
            drawPanel.Height = originalPanel.ActualHeight;
        }

        //Mousedown handler
        private void MD(object sender, MouseButtonEventArgs e)
        {
            //Handler for first press of mouse
            if (cond == 0)
            {
                start = Mouse.GetPosition(drawPanel); //Getting start marquee position 
                prev = start; //Remember last point
                start = CorP(start); //Converts coords to image coords

                //Packing args to thread
                List<int> args = new List<int>();
                args.Add((int)start.X);
                args.Add((int)start.Y);
                args.Add(H);
                args.Add(W);

                //Setting djikstra thread 
                DjCount = new Thread(curg.Dj);
                DjCount.Start((object)args);

                //Starting timer for updating loading bar
                Grloading = new System.Windows.Threading.DispatcherTimer();
                Grloading.Tick += new EventHandler(Tick);
                Grloading.Interval = new TimeSpan(0, 0, 1);
                Grloading.Start();

                Pbar.Visibility = Visibility.Visible;
                Procl.Content = "Exellent, image preparation will take few moments";
                cond = 2; //Dj started
            }
            else
                //Handler for second press of mouse
                if (cond == 4)
                    ++cond; //now we can paint
        }
        
        //Mouseup handler
        private void MU(object sender, MouseButtonEventArgs e)
        {
            //First mouseup
            if (cond == 2)
            {
                drawPanel.Cursor = Cursors.Wait;
                cond = 3;

                //Setting all buttons unactive while Dj is going
                openFileButton.IsEnabled = false;
                sobelButton.IsEnabled = false;
                resetButton.IsEnabled = false;
            }
            else
                //Second mouseup
                if (cond == 5)
                {
                    ++cond;

                    Procl.Content = "Done!";
                    //Setting all buttons active when marquee completed
                    openFileButton.IsEnabled = true;
                    sobelButton.IsEnabled = true;
                    resetButton.IsEnabled = true;
                }
        }

        //Timer Tick
        private void Tick(object sender, EventArgs e)
        {
            Pbar.Value = ImGraph.load; //Attach loading bar to load of Dj
            if (!DjCount.IsAlive)
            {
                ++cond;
                Grloading.Stop(); //Stops timer when Dj finished
                Procl.Content = "Done. Now you can draw!";
                drawPanel.Cursor = Cursors.Pen;
                Pbar.Visibility = Visibility.Hidden;
            }
        }

        //Mouse move handler
        private void MM(object sender, MouseEventArgs e)
        {
            //If Dj is finish we'll track mouse
            if (cond == 5)
            {
                drawPanel.Children.Clear(); //Clean everything on canvas
                Point curpoint = Mouse.GetPosition(drawPanel); //Get current mouse pos

                //Setting current line
                SolidColorBrush gr = new SolidColorBrush();
                gr.Color = Colors.Green;
                Line lnn = new Line
                {
                    Stroke = gr,
                    X1 = prev.X,
                    X2 = curpoint.X,
                    Y1 = prev.Y,
                    Y2 = curpoint.Y
                };
                prev = curpoint;
                way.Add(lnn); //Adding it to mouse way

                //Drawing mouse way
                for (int i = 0; i < way.Count; ++i)
                    drawPanel.Children.Add(way[i]);
                
                //Getting path
                List<int> dots = new List<int>();
                curpoint = CorP(curpoint);
                dots = curg.GetPath((int)(curpoint.Y * W + curpoint.X));

                //Drawing path                
                for (int i = 0; i < dots.Count - 1; ++i)
                {

                    SolidColorBrush re = new SolidColorBrush();
                    re.Color = Colors.Red;
                    Line ln = new Line
                    {
                        Stroke = re,
                        X1 = OpCorX(dots[i] % W),
                        X2 = OpCorX(dots[i + 1] % W),
                        Y1 = OpCorY(dots[i] / W),
                        Y2 = OpCorY(dots[i + 1] / W),
                    };
                    drawPanel.Children.Add(ln);
                }
                
            }
        }
        
        //Resizes drawPanel
        private void RES(object sender, SizeChangedEventArgs e)
        {
            drawPanel.Height = originalPanel.ActualHeight;
        }
        
        // Methods for recalculation coordinates
        private int OpCorY(double y)
        {
            return (int)((y / proccessedPanel.Source.Height) * proccessedPanel.ActualHeight);
        }
        private int OpCorX(double x)
        {
            return (int)((x / proccessedPanel.Source.Width) * proccessedPanel.ActualWidth);
        }
        private int CorX(double x)
        {
            return (int)((x / proccessedPanel.ActualWidth) * proccessedPanel.Source.Width);
        }
        private int CorY(double y)
        {
            return (int)((y / proccessedPanel.ActualHeight) * proccessedPanel.Source.Height);
        }

        // Converts coordinates from original image to displaying image
        private Point CorP(Point p)
        {
            p.X = CorX(p.X);
            p.Y = CorY(p.Y);
            return p;
        }
    }
}
