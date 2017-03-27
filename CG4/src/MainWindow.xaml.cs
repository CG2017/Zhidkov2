using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;

namespace ImageInfoViewer
{
    class ResultParameter
    {
        public List<BindingImage> Images;
        public string ElapsedTime;
        public ResultParameter()
        {
            Images = new List<BindingImage>();
            ElapsedTime = null;
        }
    }

    public partial class MainWindow : Window
    {
        private BackgroundWorker backgroundWorker;
        private BindingList<BindingImage> ImageList;
        private Microsoft.Win32.OpenFileDialog ofd;
        public int ImagesCount = 0;
        private List<string> paths;

        public MainWindow()
        {
            InitializeComponent();
            backgroundWorker = ((BackgroundWorker)FindResource("backgroundWorker"));
            ImageList = new BindingList<BindingImage>();
            ofd = new Microsoft.Win32.OpenFileDialog();
            ImageGrid.ItemsSource = ImageList;
            paths = new List<string>();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ofd.Filter = "All Supported Image Files|*.jpg;*.jpeg;*.jfif;*.tif;*.tiff;*.dib;*.rle;*.bmp;*.png;*.ico;*.gif;*.exif";
            ofd.ShowReadOnly = true;
            ofd.Multiselect = true;
            ofd.Title = "Choose image";
            if (ofd.ShowDialog() == true)
            {
                backgroundWorker.RunWorkerAsync(ofd);
            }
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog backOfd = (Microsoft.Win32.OpenFileDialog)e.Argument;
            ResultParameter rp = new ResultParameter();
            int count = 0;
            double step = 100.0 / backOfd.FileNames.Length;
            FileStream SourceStream = null;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            foreach (string FileName in backOfd.FileNames)
            {
                try
                {
                    paths.Add(FileName);
                    count++;
                    backgroundWorker.ReportProgress((int)(count * step), ofd.SafeFileNames[count - 1]);
                    SourceStream = File.Open(FileName, FileMode.Open);

                    System.Drawing.Image img = System.Drawing.Image.FromStream(SourceStream, false, false);
                    rp.Images.Add(new BindingImage(++ImagesCount, backOfd.FileNames[count - 1], img));
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    if (SourceStream != null)
                        SourceStream.Close();
                }
            }

            stopWatch.Stop();

            TimeSpan ts = stopWatch.Elapsed;
            rp.ElapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            e.Result = rp;
        }

        private void WorkDone(object sender, RunWorkerCompletedEventArgs e)
        {
            ResultParameter rp = (ResultParameter)e.Result;
            foreach (BindingImage img in rp.Images)
            {
                ImageList.Add(img);
            }
            int TotalImages = ofd.FileNames.Length;
            int Errors = TotalImages - rp.Images.Count;
            StateBar.Value = 0;
            ProcessLabel.Text = "Complete. " + TotalImages + " files loaded in: " + rp.ElapsedTime;
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProcessLabel.Text = "In process " + (string)e.UserState;
            if (e.ProgressPercentage > 100)
            {
                StateBar.Value = 100;
            }
            else
            {
                StateBar.Value = e.ProgressPercentage;
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ImageList.Clear();
            ImagesCount = 0;
            ProcessLabel.Text = "Complete";
            paths.Clear();
            if (image1.Source != null)
                image1.Source = null;
        }

        private void clickRow(object sender, MouseButtonEventArgs e)
        {
            if (ImageGrid.SelectedItem != null)
            {
                int index = ImageGrid.SelectedIndex;
                string format = ImageList[index].Format;

                if (format == "Bmp" || format == "Jpg" || format == "Jpeg" || format == "Tiff" || format == "Png" || format == "Ico" || format == "Gif")
                {
                    image1.BeginInit();
                    BitmapImage bmi = new BitmapImage(new Uri(ImageList[index].Name));
                    bmi.CacheOption = BitmapCacheOption.OnLoad;
                    image1.Source = bmi;
                    image1.EndInit();
                }
            }
        }
    }
}
