using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PaintLab
{
    /// <summary>
    /// Logika interakcji dla klasy WindowColorPicker.xaml
    /// </summary>
    public partial class WindowColorPicker : Window
    {
        public WindowColorPicker()
        {
            InitializeComponent();
        }

        private float h = 0;
        private float s = 0;
        private float v = 0;
        private float Rprim = 0;
        private float Gprim = 0;
        private float Bprim = 0;

        public SolidColorBrush color = new SolidColorBrush();
        private byte colorR = 0;
        private byte colorB = 0;
        private byte colorG = 0;
        private void Red_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            try
            {
                var number = Int32.Parse(textBox.Text);
                if (number > 255 )
                {
                    textBox.Text = "255";
                }
                else if(number < 0)
                {
                    textBox.Text = "0";
                }
                
            }
            catch(Exception ex)
            {
                MessageBox.Show("Podałeś znak, który nie jest liczbą");
                textBox.Text = "0";
            }
            Rprim = Int32.Parse(textBox.Text) / 255.0f;
            colorR = Byte.Parse(textBox.Text);
            color = new SolidColorBrush(Color.FromRgb(colorR, colorG, colorB));
            ColorRectangle.Fill = color;
            FromRGBToHSV();
        }

        private void Green_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            try
            {
                var number = Int32.Parse(textBox.Text);
                if (number > 255)
                {
                    textBox.Text = "255";
                }
                else if (number < 0)
                {
                    textBox.Text = "0";
                }
            }
            catch (Exception ex)
            {
                if (textBox.Text != "")
                {
                    MessageBox.Show("Podałeś znak, który nie jest liczbą");
                }
                textBox.Text = "0";
            }
            Gprim = Int32.Parse(textBox.Text) / 255.0f;
            colorG = Byte.Parse(textBox.Text);
            color = new SolidColorBrush(Color.FromRgb(colorR, colorG, colorB));
            ColorRectangle.Fill = color;
            FromRGBToHSV();
        }

        private void Blue_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            try
            {
                var number = Int32.Parse(textBox.Text);
                if (number > 255)
                {
                    textBox.Text = "255";
                }
                else if (number < 0)
                {
                    textBox.Text = "0";
                }         
            }
            catch (Exception ex)
            {
                MessageBox.Show("Podałeś znak, który nie jest liczbą");
                textBox.Text = "0";
            }
            Bprim = Int32.Parse(textBox.Text) / 255.0f;
            colorB = Byte.Parse(textBox.Text);
            color = new SolidColorBrush(Color.FromRgb(colorR, colorG, colorB));
            ColorRectangle.Fill = color;
            FromRGBToHSV();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
        private string FromRGBToHSV()
        {
            float[] prims = new float[] { Rprim, Gprim, Bprim };
            float cMax = prims.Max();
            float cMin = prims.Min();
            float deltaPrim = cMax - cMin;

            v = cMax;

            if (cMax != 0)
            {
                s = deltaPrim/cMax;
            }
            else
            {
                s = 0;
            }

            if (cMax == Rprim)
            {
               h = 60 * (((Gprim - Bprim)/deltaPrim) % 6);
            }
            else if(cMax == Gprim)
            {
                h = 60 * (((Bprim - Rprim) / deltaPrim) + 2);
            }
            else if(cMax == Bprim)
            {
                h = 60 * (((Rprim - Gprim) / deltaPrim) + 4);
            }
            else
            {
                h = 0;
            }
            
            h = double.IsNaN(h) ? 0 : (float)Math.Round(h);
            s = (float)Math.Round(s* 100);
            v = (float)Math.Round(v * 100);
            string result = h.ToString() + "°, " + s.ToString() + "%, " + v.ToString() + "%";
            HSVtext.Content = result;
            return result;
        }
            
        }
}

