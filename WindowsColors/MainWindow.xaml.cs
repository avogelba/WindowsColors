//using Gat.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
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
        //https://www.pinvoke.net/default.aspx/user32/AppendMenu.html , however there seems to be a InsertMenu already in user32.dll
        //https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-insertmenua
        //Note:  The InsertMenu function has been superseded by the InsertMenuItem function. You can still use InsertMenu, however, if you do not need any of the extended features of InsertMenuItem. 

        //https://docs.microsoft.com/en-us/windows/desktop/api/winuser/ns-winuser-tagmenuiteminfoa
        [StructLayout(LayoutKind.Sequential)]
        public class MENUITEMINFO
        {
            public int cbSize;
            public uint fMask;
            public uint fType;
            public uint fState;
            public uint wID;
            public IntPtr hSubMenu;
            public IntPtr hbmpChecked;
            public IntPtr hbmpUnchecked;
            public IntPtr dwItemData;
            public IntPtr dwTypeData;
            public uint cch;
            public IntPtr hbmpItem;

            public MENUITEMINFO()
            {
                cbSize = Marshal.SizeOf(typeof(MENUITEMINFO));
            }
        }
        
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool InsertMenuA(IntPtr hMenu, Int32 uPosition, Int32 uFlags, UIntPtr uIDNewItem,string lpNewItem); //Not working as expected

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool InsertMenu(IntPtr hMenu, Int32 wPosition, Int32 wFlags, Int32 wIDNewItem, string lpNewItem);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool InsertMenuItemA(IntPtr hmenu,UInt32 item,bool fByPosition,  MENUITEMINFO lpmi);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool GetMenuItemInfo(IntPtr hMenu, int uItem, bool fByPosition, MENUITEMINFO lpmii);
        
        //https://docs.microsoft.com/en-us/windows/desktop/menurc/wm-syscommand
        public const Int32 WM_SYSCOMMAND = 0x112;

        //For more see: https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-insertmenua
        public const Int32 MF_SEPARATOR = 0x800; //Draws a horizontal dividing line. This flag is used only in a drop-down menu, submenu, or shortcut menu. The line cannot be grayed, disabled, or highlighted. The lpNewItem and uIDNewItem parameters are ignored. 
        public const Int32 MF_BYCOMMAND = 0x000; //Indicates that the uPosition parameter gives the identifier of the menu item. The MF_BYCOMMAND flag is the default if neither the MF_BYCOMMAND nor MF_BYPOSITION flag is specified. 
        public const Int32 MF_BYPOSITION = 0x400; //Indicates that the uPosition parameter gives the zero-based relative position of the new menu item. If uPosition is -1, the new menu item is appended to the end of the menu. 
        public const Int32 MF_STRING = 0x000; //Specifies that the menu item is a text string; the lpNewItem parameter is a pointer to the string. 

        public const Int32 _MyAboutMenuID = 1001;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //ALternative: IntPtr windowHandle = new WindowInteropHelper(myWindow).Handle;
            var windowHandle = Process.GetCurrentProcess().MainWindowHandle;
            IntPtr systemMenuHandle = GetSystemMenu(windowHandle, false);
            
            //Not sure if positions are fixed on all PC's, on my system it are 5 and 6. InsertMenuA did not work properly
            InsertMenu(systemMenuHandle, 5, MF_BYPOSITION | MF_SEPARATOR, 0, String.Empty);
            InsertMenu(systemMenuHandle, 6, MF_BYPOSITION, _MyAboutMenuID, "About...");

            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(new HwndSourceHook(WndProc));

            addButtonsFromregistry(stackPanel1, "Control Panel\\Colors");
            addButtonsFromregistry(stackPanel2, "Control Panel\\Desktop\\Colors");
        }

        //https://referencesource.microsoft.com/#windowsbase/Shared/MS/Win32/HwndWrapper.cs
        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_SYSCOMMAND)
            {
                switch (wParam.ToInt32())
                {
                    case _MyAboutMenuID:
                        //I actually wanted to try something else than adding a About.XAML (https://github.com/avogelba/GetUpdates/blob/master/GetUpdates/About.xaml)
                        //But I was diassapointed, or to stupid to use properly:
			//Was far to big and complicated: https://www.nuget.org/packages/WpfAboutView
                        
			//looked OK but old: https://www.nuget.org/packages/AboutBox/
			//Code:
                        //BitmapImage appBi = new BitmapImage(new System.Uri("pack://application:,,,/AboutLogo.png"));
                        //BitmapImage cBi = new BitmapImage(new System.Uri("pack://application:,,,/cLogo.png"));
                        // About about = new About();
                        //             about.IsSemanticVersioning = true;
                        //             about.ApplicationLogo = appBi;
                        //             about.PublisherLogo = cBi;
                        //             about.HyperlinkText = "https://github.com/avogelba/WindowsColors";
                        //about.AdditionalNotes = "Click on main Program to close About."; //very strange behaviour to close ABout
                        //about.Show();

			//I ended with my own About.XAML again:
                        About aboutWND = new About();
                        //über Mutterfenster Legen:
                        aboutWND.Left = this.Left + (this.Width - this.ActualWidth) / 2;
                        aboutWND.Top = this.Top + (this.Height - this.ActualHeight) / 2;
                        aboutWND.Owner = this;
                        aboutWND.Show();



                        break;
                }
            }
            return IntPtr.Zero;
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
