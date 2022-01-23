using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.IO.Ports;
using System.Windows.Forms;

namespace TestCam
{
    public partial class Form1 : Form
    {
        private Capture capture;
        private Image<Bgr, Byte> IMG;
        private Image<Gray, Byte> GrayImg;
        private Image<Gray, Byte> BWImg;
        private double myScale;
        private int Xpx, Ypx, N;
        private double Xcm, Ycm, Zcm = 50.0;
        public double d1 = 3.5;

        //SDASDASDASDasdasdas

        static SerialPort _serialPort;
        public byte[] Buff = new byte[2];

        public Form1()
        {
            InitializeComponent();

            _serialPort = new SerialPort
            {
                PortName = "COM3",
                BaudRate = 9600
            };
            _serialPort.Open();
        }

        private void processFrame(object sender, EventArgs e)
        {
            if (capture == null)
            {
                try
                {
                    capture = new Capture();
                }
                catch (NullReferenceException excpt)
                {
                    MessageBox.Show(excpt.Message);
                }
            }

            IMG = capture.QueryFrame();

            GrayImg = IMG.Convert<Gray, Byte>();
            BWImg = GrayImg.ThresholdBinaryInv(new Gray(50), new Gray(255));
            myScale = 59.0 / BWImg.Width;

            Xpx = 0;
            Ypx = 0;
            N = 0;

            for (int i = 0; i < BWImg.Width; i++)
                for (int j = 0; j < BWImg.Height; j++)
                {
                    if (BWImg[j, i].Intensity > 128)
                    {
                        N++;
                        Xpx += i;
                        Ypx += j;
                    }
                }

            if (N > 0)
            {
                Xpx /= N;
                Ypx /= N;

                Xpx -= BWImg.Width / 2;
                Ypx = BWImg.Height / 2 - Ypx;

                Xcm = (Xpx - BWImg.Width / 2) * myScale;
                Ycm = (BWImg.Height / 2 - Ypx) * myScale;

                textBox1.Text = Xcm.ToString();
                textBox2.Text = Ycm.ToString();
                textBox3.Text = N.ToString();

                double diff=9.0;
                double Pz = Ycm-diff,
                       Py = -Xcm,
                       Px = -Zcm;

                double Th1 = Math.Atan(Py / Px);

                double Th2 = Math.Atan(Math.Sin(Th1) * (Pz - d1) / Py);

                Th1 *= (180 / Math.PI);
                Th2 *= (180 / Math.PI) ;


                Th1 += 90;
                Th2 += 90;

                textBox4.Text = Th1.ToString();
                textBox5.Text = Th2.ToString();
                Buff[0] = (byte)Th1;
                Buff[1] = (byte)Th2;

                try
                {
                    _serialPort.Write(Buff, 0, 2);
                }
                catch (Exception ex)
                {
                    return;
                }
            }

            else
            {
                textBox1.Text = "";
                textBox2.Text = "";
                textBox3.Text = N.ToString();
            }

            try
            {
                imageBox2.Image = IMG;
                imageBox3.Image = GrayImg;
                imageBox5.Image = BWImg;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            button1.Enabled = false;
            button2.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            button1.Enabled = true;
            button2.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            GrayImg.Save("D:\\Image" + ".jpg");
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            processFrame(sender, e);
        }

    }
}