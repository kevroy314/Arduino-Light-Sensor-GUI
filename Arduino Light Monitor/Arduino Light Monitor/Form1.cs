using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using System.Drawing.Drawing2D;

namespace Arduino_Light_Monitor
{
    public partial class Form1 : Form
    {
        private double[] scalingValues = { 1.6558441558441558441558441558442, 1.2200956937799043062200956937799, 1.9615384615384615384615384615385, 1.3421052631578947368421052631579 };
        private SerialPort port;
        private Thread readerThread;
        private bool closeThread;
        private bool setScalingValues;

        public Form1()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.DoubleBuffered = true;
            InitializeComponent();
            this.BackgroundImage = new Bitmap(this.Width, this.Height);
            closeThread = false;
            setScalingValues = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return false;
        }

        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            closeThread = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Start Logging")
            {
                button1.Text = "Stop Logging";
                readerThread = new Thread(new ThreadStart(acquire));
                readerThread.Start();
            }
            else if (button1.Text == "Stop Logging")
            {
                closeThread = true;
                button1.Text = "Start Logging";
            }
        }

        int maxChartLength = 512;

        delegate void AddPointDelegate(double point);

        void AddPoint(double point)
        {
            if (chart1.InvokeRequired)
            {
                AddPointDelegate d = new AddPointDelegate(AddPoint);
                this.Invoke(d, new object[] { point });
            }
            else
            {
                if (chart1.Series[0].Points.Count > maxChartLength)
                    chart1.Series[0].Points.RemoveAt(0);
                chart1.Series[0].Points.AddY(point);
            }
        }

        delegate void RefreshFormDelegate(double[] dvals);

        void RefreshForm(double[] dvals)
        {
            if (this.InvokeRequired)
            {
                RefreshFormDelegate d = new RefreshFormDelegate(RefreshForm);
                this.Invoke(d, new object[] { dvals });
            }
            else
            {
                using (Graphics g = Graphics.FromImage(this.BackgroundImage))
                {
                    int segmentWidth = (this.Width / dvals.Length) / 2 + 1;
                    g.FillRectangle(Brushes.Black, this.Bounds);
                    for (int i = 0; i < dvals.Length; i++)
                    {
                        double dval = dvals[i];
                        try { dval *= scalingValues[i]; } catch (Exception) { }
                        int val = (int)dval;
                        Color c = Color.Blue;
                        if(val < 256)
                            c = Color.FromArgb(0, 0, val);
                        else if (val >= 256)
                        {
                            int lightVal = val - 255;
                            if(lightVal>255)
                                lightVal = 255;
                            c = Color.FromArgb(lightVal, lightVal, 255);
                        }

                        LinearGradientBrush b = new LinearGradientBrush(new Point(0, 0), new Point(segmentWidth, 0), Color.Black, c);
                        LinearGradientBrush b2 = new LinearGradientBrush(new Point(segmentWidth, 0), new Point(segmentWidth * 2, 0), c, Color.Black);
                        int x0 = segmentWidth * i * 2;
                        int x1 = segmentWidth * i * 2 + segmentWidth;
                        g.FillRectangle(b, x0, 0, segmentWidth, this.Height);
                        g.FillRectangle(b2, x1, 0, segmentWidth, this.Height);
                    }
                }
                this.Refresh();
            }
        }
        void acquire()
        {
            if(port!=null)
                port.Dispose();
            port = new SerialPort("COM4", 9600);
            port.Open();
            DateTime refreshTimer = DateTime.Now;
            while (!closeThread)
            {
                try
                {
                    while (port.BytesToRead > 0 && !closeThread)
                    {
                        port.ReadLine();
                        string data = port.ReadLine();

                        DateTime newTime = DateTime.Now;

                        if (newTime.Subtract(refreshTimer).Milliseconds > 30)
                        {
                            data = data.Substring(7);
                            string[] vals = data.Split(new char[] { ' ' });
                            double[] dvals = new double[vals.Length];
                            double total = 0;
                            for (int i = 0; i < dvals.Length; i++)
                            {
                                dvals[i] = double.Parse(vals[i].Trim());
                                total += dvals[i];
                            }

                            if (setScalingValues)
                            {
                                scalingValues = new double[dvals.Length];
                                for (int i = 0; i < scalingValues.Length; i++)
                                    scalingValues[i] = 255/dvals[i];
                                setScalingValues = false;
                            }

                            double avg = total / dvals.Length;

                            refreshTimer = newTime;

                            AddPoint(avg);
                            RefreshForm(dvals);
                            port.DiscardInBuffer();
                        }
                    }
                }
                catch (Exception) { }
                Thread.Sleep(10);
            }
            closeThread = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            setScalingValues = true;
        }
    }
}
