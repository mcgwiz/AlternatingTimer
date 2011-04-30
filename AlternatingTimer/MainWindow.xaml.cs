using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace AlternatingTimer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        private const int REST_SECONDS = 240;
        private const int GO_SECONDS = 60;

        public bool IsResting
        {
            get;
            set;
        }

        public static readonly DependencyProperty TextContextProperty = DependencyProperty.Register("TextContext", typeof(string), typeof(MainWindow));
        public string TextContext {
            get {
                return (string)GetValue(TextContextProperty);
            }
            set {
                SetValue(TextContextProperty, value);
            }
        }

        public static readonly DependencyProperty BackgroundContextProperty = DependencyProperty.Register("BackgroundContext", typeof(Brush), typeof(MainWindow));
        public Brush BackgroundContext {
            get {
                return (Brush)GetValue(BackgroundContextProperty);
            }
            set {
                SetValue(BackgroundContextProperty, value);
            }
        }

        private int _counter;

        private readonly Timer _updateTimer;
        private readonly Timer _hideTimer;

        public MainWindow() {
            InitializeComponent();
            _updateTimer = new Timer(OnElapsed, null, 1000, 1000);
            _hideTimer = new Timer(_ =>Dispatcher.BeginInvoke((Action)SendToBack), null, 1000, Timeout.Infinite);
            IsResting = true;
            TextContext = "REST";
            BackgroundContext = new SolidColorBrush(Colors.LightGreen);
            SourceInitialized += (x, y) => this.HideMinimizeAndMaximizeButtons();
            Title = "Deskercise: Rest";
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.None;
            MouseLeftButtonDown += (s, e) => DragMove();
            Left = 0;
            Top = 0;
        }

        private void OnElapsed(object state)
        {
            _counter++;
            
            Dispatcher.BeginInvoke((Action)(()=>{
                if (IsResting)
                {
                    TextContext = "REST " + _counter;
                    if (_counter < REST_SECONDS) return;

                    TextContext = "GO";
                    Title = "Deskercise: GO";
                    BackgroundContext = new SolidColorBrush(Colors.Pink);
                    IsResting = false;
                }
                else
                {
                    TextContext = "GO " + _counter;
                    if (_counter < GO_SECONDS) return;

                    BackgroundContext = new SolidColorBrush(Colors.LightGreen);
                    TextContext = "REST";
                    Title = "Deskercise: Rest";
                    IsResting = true;
                }

                Topmost = true;
                _hideTimer.Change(500, Timeout.Infinite);
                _counter = 0;
            }));
        }

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            uint uFlags);

        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;

        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        void SendToBack()
        {
            Topmost = false;
            var hWnd = new WindowInteropHelper(this).Handle;
            SetWindowPos(hWnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
        }

        [DllImport("user32.dll")]
        internal extern static int SetWindowLong(IntPtr hwnd, int index, int value);

        [DllImport("user32.dll")]
        internal extern static int GetWindowLong(IntPtr hwnd, int index);

        internal void HideMinimizeAndMaximizeButtons() {
            const int GWL_STYLE = -16;

            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            long value = GetWindowLong(hwnd, GWL_STYLE);

            SetWindowLong(hwnd, GWL_STYLE, (int)(value & -131073 & -65537));

        }
    }
}
