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
using System.Windows.Threading;

namespace MoveWindowsHere
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Task task= null;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_MoveAll(object sender, RoutedEventArgs e)
        {
            if (task != null && !task.IsCompleted) {
                logbox.Text += "Task is already running \n";
                return;
            }
            IntPtr me = ApiWrapper.GetActiveWindow();
            ApiWrapper.RECT destinationRect = ApiWrapper.GetWindowRect(me);
            if (behindMe.IsChecked == false) {
                ApiWrapper.ShowWindow(me, 3);
                destinationRect = ApiWrapper.GetWindowRect(me);
                ApiWrapper.ShowWindow(me, 1);
            }
            
            logbox.Text = $"I'll try to move all the windows in {destinationRect} \n";
            WindowsEnumerator we = new WindowsEnumerator();
            string filter = FilterTextBox.Text;
            task = Task.Run(() =>
            {
                we.ApplyLambdaToallWindows((x) =>
                {
                    if (x == me) return;
                    int size = ApiWrapper.GetWindowTextLength(x);
                    StringBuilder sb = new StringBuilder(size);
                    ApiWrapper.GetWindowText(x, sb, size);
                    if (filter.Length == 0 || sb.ToString().IndexOf(filter) >= 0) {
                        bool iconic = ApiWrapper.IsIconic(x);
                        bool maximized = ApiWrapper.IsZoomed(x);
                        Application.Current.Dispatcher.Invoke(() => {
                            logbox.Text += $"Moving {x} {sb} \n";
                            if(iconic)
                                logbox.Text += " iconic ";

                            if (maximized)
                                logbox.Text += " maximized ";

                            logbox.Text += "\n";
                            logbox.ScrollToEnd();
                        });


                        if (iconic)
                            ApiWrapper.ShowWindow(x, 9);
                        ApiWrapper.RECT r2;
                        r2 = ApiWrapper.GetWindowRect(x);

                        int h = r2.Bottom - r2.Top;
                        int w = r2.Right - r2.Left;
                        
                        ApiWrapper.MoveWindow(x, destinationRect.Left, destinationRect.Top, w, h, true);
                        if (iconic) 
                            ApiWrapper.ShowWindow(x, 11);
                        if (maximized) 
                            ApiWrapper.ShowWindow(x, 3);
                    }
                }, null);
                Application.Current.Dispatcher.Invoke(() => { logbox.Text += "Done \n"; });
            }
            );
        }
    }
}
