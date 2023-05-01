using SharpGL;
using SharpGL.SceneGraph;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
           
        }
        OpenGL gl = new OpenGL();
        public void Init()
        {

        }
        public void draw(uint nEnum)
        {
            //使用PushName的话，其实内存是布局是：命中的图元位于绘制顺序的第几个、最大Z、最小Z、ID（将选中的ID按顺序都加入（包括初始化的0））。
            //使用LoadName的话，内存布局：未知、最大Z、最小Z、命中的ID。
            //关于第一个未知，因为不管是命中一个还是同时命中两个，其第一个值一直为1

            //选择模式和渲染模式都要渲染的为公共部分
            gl.Color(0.0, 1.0, 0.0);
            if (OpenGL.GL_SELECT == nEnum)
            {
                //gl.PushName(1);
                gl.LoadName(1);
            }
            gl.Begin(OpenGL.GL_QUADS);
            {
                gl.Vertex(-1, 1, 0);
                gl.Vertex(-1, -1, 0);
                gl.Vertex(1, -1, 0);
                gl.Vertex(1, 1, 0);
            }
            gl.End();

            gl.Color(1.0, 0.0, 0.0);
            if (OpenGL.GL_SELECT == nEnum)
            {
                //gl.PushName(2);
                gl.LoadName(2);
            }        
            gl.Rect(0, 0, 5, 5);

            gl.Color(0.0, 0.0, 1.0);
            gl.PointSize(50.0f);
            gl.Enable(OpenGL.GL_POINT_SMOOTH);
            if (OpenGL.GL_SELECT == nEnum)
            {
                //gl.PushName(3);
                gl.LoadName(3);
            }
            gl.Begin(OpenGL.GL_POINTS);
            {
                gl.Vertex(0, 0, 0);
            }
            gl.End();
            //不需要选择的部分
            if (OpenGL.GL_RENDER == nEnum)
            {
                gl.Color(1.0, 1.0, 1.0);
                gl.Rect(-5, -5, 0, 0);
            }         
            gl.Finish();
            gl.Flush();
        }

        private void openGLControl1_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                //设置缓冲区
                uint[] buffer = new uint[32];
                gl.SelectBuffer(32, buffer);
                //获取当前视口矩阵
                int[] viewport = new int[4];
                gl.GetInteger(OpenGL.GL_VIEWPORT, viewport);

                //进入渲染模式
                gl.RenderMode(OpenGL.GL_SELECT);
                //初始化名字栈
                gl.InitNames();
                //压入名字0，保证名字栈非空
                gl.PushName(0);

                //进入投影矩阵模式
                gl.MatrixMode(OpenGL.GL_PROJECTION);
                //矩阵压栈，保存之前的投影矩阵
                gl.PushMatrix();
                //加载单位矩阵，清除之前的矩阵变换
                gl.LoadIdentity();

                //设置拾取框矩阵
                gl.PickMatrix(e.X, viewport[3] - e.Y, 4, 4, viewport);
                //设置投影矩阵，保证投影矩阵与之前一致。
                gl.Ortho(-10, 10, -10, 10, -10, 10);
                //gl.Perspective(50.0f, (double)Width / (double)Height, 0.01, 100);
                gl.LookAt(5, -5, -5, 0, 0, 0, 0, 0, 1);
                
                //再次渲染，这里我认为只需要在选择模式下渲染需要选择的物体
                draw(OpenGL.GL_SELECT);

                //进入投影矩阵模式
                gl.MatrixMode(OpenGL.GL_PROJECTION);
                //矩阵出栈，恢复之前的投影矩阵
                gl.PopMatrix();

                //获取命中个数
                int num_picks = gl.RenderMode(OpenGL.GL_RENDER);
                MessageBox.Show("Select Num:" + num_picks.ToString());
                //使用PushName()
                //uint[] selected = new uint[5];
                //int old_index = 0;
                //int now_index = 0;
                //int ID_increase = 0;
                //for(int i = 0; i < num_picks; i++)
                //{
                //    //这里-1是把初始化的0给减去，只是selected值-1，now_index值没变
                //    selected[i] = buffer[old_index] - 1;
                //    ID_increase = (int)buffer[old_index] - 1;
                //    now_index = old_index + 3 + ID_increase;                    
                //    old_index = now_index + 1;

                //}

                //使用LoadName()
                uint[] selected = new uint[5];
                for (int i = 0; i < num_picks; i++)
                {
                    selected[i] = buffer[(i + 1) * 4 - 1];
                }
                for (int i = 0; i < selected.Length; i++)
                {
                    switch (selected[i])
                    {
                        case 0:

                            break;
                        case 1:
                            MessageBox.Show("选中了绿色");
                            break;
                        case 2:
                            MessageBox.Show("选中了红色");
                            break;
                        case 3:
                            MessageBox.Show("选中了蓝色");
                            break;
                        default:

                            break;
                    }
                }
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void openGLControl1_OpenGLDraw(object sender, RenderEventArgs args)
        {
            //完整不变是这样的
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            draw(OpenGL.GL_RENDER);
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Ortho(-10, 10, -10, 10, -10, 10);
            //gl.Perspective(50.0f, (double)Width / (double)Height, 0.01, 100);
            gl.LookAt(5, -5, -5, 0, 0, 0, 0, 0, 1);


            //要注意的是：投影分为透视与正射两种。
            //要注意的是：投影分为透视与正射两种。要先设置投影矩阵函数再设置视点函数LookAt，这两个函数要搭配使用，以保证可控的视觉效果。
            //而如果在正交投影中设置LooAt视点函数时，视点函数的eye的Z值要位于正射投影后两个参数，即Z值之间才能看见图像。

            //测试,有问题是这样的，点击一次后会铺满窗口
            //gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            //gl.LoadIdentity();
            //gl.LookAt(0, 0, -5, 0, 0, 0, 0, 1, 0);
            //draw(OpenGL.GL_RENDER);
        }

        private void openGLControl1_OpenGLInitialized(object sender, EventArgs e)
        {

        }
    }
}
