using Microsoft.Win32;
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

namespace WindowsColors
{

    //Interesting keys found in Registry:
//HKEY_CURRENT_USER\Console
//HKEY_CURRENT_USER\Control Panel\Cursors
//HKEY_CURRENT_USER\Control Panel\Colors
//HKEY_CURRENT_USER\Control Panel\Mouse
//HKEY_CURRENT_USER\Control Panel\Desktop\Colors
//HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics
//HKEY_CURRENT_USER\Control Panel\Appearance
//HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects
//HKEY_CURRENT_USER\Software\Microsoft\Windows\DWM
//HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ThemeManager
//HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes
//HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Lock Screen


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            addButtonsFromregistry(stackPanel1, "Control Panel\\Colors");
            addButtonsFromregistry(stackPanel2, "Control Panel\\Desktop\\Colors");
        }

        /// <summary>
        /// Add a empty and invisible button
        /// </summary>
        /// <param name="sp"></param>
        private void addDummyButton(StackPanel sp)
        {
            Button newBtnFil = new Button();
            newBtnFil.Content = "";
            newBtnFil.Visibility = Visibility.Hidden;
            sp.Children.Add(newBtnFil);
        }

        /// <summary>
        /// Read Colors-Values from registry and add it to the stack panels
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="regKey"></param>
        private void addButtonsFromregistry(StackPanel sp, string regKey)
        {
            using (RegistryKey colorKey = Registry.CurrentUser.OpenSubKey(regKey))
            {
                var darkSide = Color.FromRgb(0, 0, 0);
                foreach (var entry in colorKey.GetValueNames().OrderBy(x => x))
                {
                    Button newBtn = new Button();
                    
                    if (String.Compare(regKey, "Control Panel\\Desktop\\Colors") == 0)
                    {
                        //to be able to better compare the two color definitions
                        if (String.Compare(entry, "ButtonAlternateFace") == 0) //is this always like so?
                        {
                            addDummyButton(sp);
                        }
                        else if(String.Compare(entry, "MenuText") == 0) //is this always like so?
                        {
                            addDummyButton(sp);
                            addDummyButton(sp);
                        }
                    }
                    
                    var name = entry;
                    var btnName = entry.Replace(" ", "_"); //not needed, just in case
                    //Coloring stuff
                    var colValue = colorKey.GetValue(entry).ToString();
                    var colorsRGB = colValue.Split(' ');
                    var redVal = Int32.Parse(colorsRGB[0]);
                    var greenVal = Int32.Parse(colorsRGB[1]);
                    var blueVal = Int32.Parse(colorsRGB[2]);
                    var redByte = Convert.ToByte(redVal);
                    var greenByte = Convert.ToByte(greenVal);
                    var blueByte = Convert.ToByte(blueVal);
                    var color = Color.FromRgb(redByte, greenByte, blueByte);
                    SolidColorBrush brush = new SolidColorBrush(color);

                    //To make text readable
                    if (isDark(color))
                        newBtn.Foreground = Brushes.White;
                    else
                        newBtn.Foreground = Brushes.Black;

                    newBtn.Background = brush;
                    newBtn.Content = entry + "     (" + colValue + ")";
                    newBtn.Name = btnName;
                    newBtn.IsHitTestVisible = false; //on mouse over it changed the colors, is there a better solution than disabling click-ability?

                    newBtn.ToolTip = colValue;

                    sp.Children.Add(newBtn);

                }
            }
        }

        /// <summary>
        /// Check if the color is dark or light
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        private bool isDark(Color col)
        {
            var sum = col.R + col.G + col.B;
            return sum < 382;            //(255 + 255 + 255 )/2 = 282.5

        }
#region not_used
        /*
         * Tried different ways to get the problem solved that some text are not readable (e.g. gray on gray), 
         * best solution however was isDark() check
         */

        /// <summary>
        /// Get the contrast between to colrs
        /// </summary>
        /// <param name="col1"></param>
        /// <param name="col2"></param>
        /// <returns></returns>
        private bool isReadybleContrast(Color col1, Color col2)
        {
            float MINCONTRAST = 0.5f;
            var brightness1 = getColorBrightness(col1);
            var brightness2 = getColorBrightness(col2);
            return (Math.Abs(brightness1 - brightness2) >= MINCONTRAST);

        }

        //formular from https://en.wikipedia.org/wiki/Relative_luminance
        public float getColorBrightness(Color color)
        {
            return (float)(0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B);
        }


        bool areColorsClose(Color color1, Color color2, int tresholdValue = 50)
        {
            
            return tresholdValue * tresholdValue >= colorCalc(color1, color2);
        }

        int colorCalc(Color color1, Color color2)
        {
            int r = (int)color1.R - color2.R,
                g = (int)color1.G - color2.G,
                b = (int)color1.B - color2.B;
            return (r * r + g * g + b * b);
        }

        private Brush getInvertedColor(Color colorToInvert)
        {
            
            Color invertedColor = Color.FromRgb((byte)~colorToInvert.R, (byte)~colorToInvert.G, (byte)~colorToInvert.B);
            return new SolidColorBrush(invertedColor);
            
        }
#endregion not_used
    }
}
