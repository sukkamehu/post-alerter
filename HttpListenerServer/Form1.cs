using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;




namespace HttpListenerServer
{
    public partial class Form1 : Form
    {
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        static HttpListener listener;
        private Thread listenThread1;
        public Form1()
        {
            InitializeComponent();
        }

        private const int SnapDist = 100;
        private bool DoSnap(int pos, int edge)
        {
            int delta = pos - edge;
            return delta > 0 && delta <= SnapDist;
        }

        private bool mouseDown;
        private Point lastLocation;

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
            lastLocation = e.Location;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                this.Location = new Point(
                    (this.Location.X - lastLocation.X) + e.X, (this.Location.Y - lastLocation.Y) + e.Y);

                this.Update();
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
            base.OnResizeEnd(e);
            Screen scn = Screen.FromPoint(this.Location);
            if (DoSnap(this.Left, scn.WorkingArea.Left)) this.Left = scn.WorkingArea.Left;
            if (DoSnap(this.Top, scn.WorkingArea.Top)) this.Top = scn.WorkingArea.Top;
            if (DoSnap(scn.WorkingArea.Right, this.Right)) this.Left = scn.WorkingArea.Right - this.Width;
            if (DoSnap(scn.WorkingArea.Bottom, this.Bottom)) this.Top = scn.WorkingArea.Bottom - this.Height;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8000/");
            listener.Prefixes.Add("http://127.0.0.1:8000/");
            listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;

            listener.Start();
            this.listenThread1 = new Thread(new ParameterizedThreadStart(startlistener));
            listenThread1.Start();
           
        }


        private void startlistener(object s)
        {

            while (true)
            {
               
                ////blocks until a client has connected to the server
                ProcessRequest();

            }

        }


        private void ProcessRequest()
        {

            var result = listener.BeginGetContext(ListenerCallback, listener);
            result.AsyncWaitHandle.WaitOne();

        }

        private void ListenerCallback(IAsyncResult result)
        {

            var context = listener.EndGetContext(result);
            Thread.Sleep(1000);
            try
            {
                var data_text = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding).ReadToEnd();

            //functions used to decode json encoded data.
            //JavaScriptSerializer js = new JavaScriptSerializer();
            //var data1 = Uri.UnescapeDataString(data_text);
            //string da = Regex.Unescape(data_text);
            // var unserialized = js.Deserialize(data_text, typeof(String));

            var cleaned_data = System.Web.HttpUtility.UrlDecode(data_text);

            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
            Console.Write(context.Response);
            //use this line to get your custom header data in the request.
            //var headerText = context.Request.Headers["mycustomHeader"];

            //use this line to send your response in a custom header
            //context.Response.Headers["mycustomResponseHeader"] = "mycustomResponse";

            //MessageBox.Show(cleaned_data);
            context.Response.Close();

                label1.Invoke(new Action(() =>
                {
                    label1.Text = cleaned_data;
                }));
            } catch { }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_DoubleClick(object sender, EventArgs e)
        {
           // System.Windows.Forms.Application.ExitThread();
           // System.Windows.Forms.Application.Exit();
        }

        private void label1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // System.Windows.Forms.Application.ExitThread();
            // System.Windows.Forms.Application.Exit();
        }
    }
}
