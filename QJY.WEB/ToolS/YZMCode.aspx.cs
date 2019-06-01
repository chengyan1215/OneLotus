using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace QJY.WEB
{
    public partial class YZMCode : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            NumberChar("chkcode", 4);
        }
        public void NumberChar(string sessionName, int length)
        {
            string Vchar = "2,3,4,5,6,7,8,9,2,3,4,5,6,7,8,9,A,B,C,D,E,F,G,H,J,K,L,M,N,P,Q,R,S,T,U,W,X,Y,Z";
            string[] VcArray = Vchar.Split(',');
            string[] str = new string[length];
            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                int iNum = 0;
                while ((iNum = Convert.ToInt32(VcArray.Length * random.NextDouble())) == VcArray.Length)
                {
                    iNum = Convert.ToInt32(VcArray.Length * random.NextDouble());
                }
                str[i] = VcArray[iNum];
            }
            CreateCheckCodeImage(str);
            string identifycode = "";
            foreach (string s in str)
            {
                identifycode += s;
            }
            Session[sessionName] = identifycode;
        }

        private void CreateCheckCodeImage(string[] checkCode)
        {
            if (checkCode == null || checkCode.Length <= 0)
                return;

            System.Drawing.Bitmap image = new System.Drawing.Bitmap((int)Math.Ceiling((checkCode.Length * 25.0)), 40);
            //System.Drawing.Bitmap image = new System.Drawing.Bitmap(宽, 高); 单位是像素  
            //Math.Ceiling(小数或是双精度浮点数)    //返回大于或等于该数的最小整数
            System.Drawing.Graphics g = Graphics.FromImage(image);

            try
            {
                Random random = new Random();
                //清空图片背景色 
                g.Clear(Color.White);

                //画图片的背景噪音线   i的值越大，线条越多
                for (int i = 0; i < 20; i++)
                {
                    int x1 = random.Next(image.Width);
                    int x2 = random.Next(image.Width);
                    int y1 = random.Next(image.Height);
                    int y2 = random.Next(image.Height);

                    g.DrawLine(new Pen(Color.Silver), x1, y1, x2, y2);
                }

                //定义颜色
                Color[] c = { Color.Black, Color.Red, Color.DarkBlue, Color.Green, Color.Orange, Color.Brown, Color.DarkCyan, Color.Purple };
                //定义字体
                string[] f = { "Verdana", "Microsoft Sans Serif", "Comic Sans MS", "Arial", "宋体" };

                for (int k = 0; k <= checkCode.Length - 1; k++)
                {
                    int cindex = random.Next(7);
                    int findex = random.Next(5);

                    // 设置字体参数
                    Font drawFont = new Font("Arial", 16, (System.Drawing.FontStyle.Italic));//字体大小16px 粗体
                    //Font drae = new Font(要显示的字符, 字体大小, 字体样式);

                    SolidBrush drawBrush = new SolidBrush(c[cindex]);

                    float x = 5.0F;
                    float y = 0.0F;
                    float width = 20.0F; //显示字符的宽度
                    float height = 30.0F;//显示字符的高度
                    int sjx = random.Next(10);
                    int sjy = random.Next(image.Height - (int)height);

                    RectangleF drawRect = new RectangleF(x + sjx + (k * 18), y + sjy, width, height);//28  为字符x抽的间距系数（数字越大间距越宽）
                    //RectangleF d=new RectangleF(

                    StringFormat drawFormat = new StringFormat();
                    drawFormat.Alignment = StringAlignment.Center;

                    g.DrawString(checkCode[k], drawFont, drawBrush, drawRect, drawFormat);
                }

                //画图片的前景噪音点 i的值越大，点数越多
                for (int i = 0; i < 100; i++)
                {
                    int x = random.Next(image.Width);
                    int y = random.Next(image.Height);

                    image.SetPixel(x, y, Color.FromArgb(random.Next()));
                }

                //画图片的边框线 
                g.DrawRectangle(new Pen(Color.Silver), 0, 0, image.Width - 1, image.Height - 1);

                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                Response.ClearContent();
                Response.ContentType = "image/Gif";
                Response.BinaryWrite(ms.ToArray());
            }
            finally
            {
                g.Dispose();
                image.Dispose();
            }
        }
    }
}