using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileSeek
{ 
    [Serializable]
    public  partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            //myParamsToSeek.fileToSeekPath = textBox1.Text;
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            //myParamsToSeek.fileToSeekMask = textBox2.Text;
        }
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            /*
            #region Десериализация
            ParamsToSeek myParamsToSeek = new ParamsToSeek();
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream fs = new FileStream("FileSeek.dat", FileMode.OpenOrCreate))
            {
                myParamsToSeek  = (ParamsToSeek)formatter.Deserialize(fs);
            }
            textBox1.Text = myParamsToSeek.fileToSeekPath;
            textBox2.Text = myParamsToSeek.fileToSeekMask;
            textBox3.Text = myParamsToSeek.fileToSeekContainsThisText;
            #endregion*/

            try
            {
                #region Десериализация
                List<string> mySerialParams = new List<string>();
                mySerialParams.Add(textBox1.Text);
                mySerialParams.Add(textBox2.Text);
                mySerialParams.Add(textBox3.Text);
                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream fs = new FileStream("FileSeek.dat", FileMode.OpenOrCreate))
                {
                    mySerialParams = (List<string>)formatter.Deserialize(fs);
                }
                textBox1.Text = mySerialParams[0];
                textBox2.Text = mySerialParams[1];
                textBox3.Text = mySerialParams[2];
                #endregion
            }
            catch { }
           

    }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            #region Сериализация
            List<string> mySerialParams = new List<string>();
            mySerialParams.Add(textBox1.Text);
            mySerialParams.Add(textBox2.Text);
            mySerialParams.Add(textBox3.Text);
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream fs = new FileStream("FileSeek.dat", FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, mySerialParams);
            }
            #endregion
            
            try
            {
                FileSeekFunc.myResetEvent?.Close();
                if (FileSeekFunc.thr?.ThreadState == ThreadState.Suspended)
                    FileSeekFunc.thr?.Interrupt();
                else
                    FileSeekFunc.thr?.Abort();
            }
            catch { }


        }

        private void button1_Click(object sender, EventArgs e)//старт
        {
            Program.F1.treeView1.Nodes.Clear();
            //убиваем если был запущен поиск
            FileSeekFunc.myResetEvent?.Close();
            try { FileSeekFunc.thr?.Abort(); } catch { FileSeekFunc.thr?.Interrupt(); }

            FileSeekFunc.thr = new Thread(FileSeekFunc.fileSeeker);
            
            FileSeekFunc.myResetEvent = new ManualResetEvent(true);
            FileSeekFunc.thr.Start(FileSeekFunc.myResetEvent);
        }

        private void button2_Click(object sender, EventArgs e)//пауза
        {
            //FileSeekFunc.myResetEvent?.Reset();
            try
            {
                
                if (FileSeekFunc.thr?.ThreadState != ThreadState.Suspended)
                    FileSeekFunc.thr?.Suspend();
            }
            catch { }     
        }

        private void button3_Click(object sender, EventArgs e)//продолжить
        {
            //FileSeekFunc.myResetEvent?.Set();
            if(FileSeekFunc.thr?.ThreadState== ThreadState.Suspended) FileSeekFunc.thr?.Resume();
        }

        private void button4_Click(object sender, EventArgs e)//стоп
        {
            //FileSeekFunc.myResetEvent?.Close();
            try {
                FileSeekFunc.myResetEvent?.Close();
                if (FileSeekFunc.thr?.ThreadState == ThreadState.Suspended)
                    FileSeekFunc.thr?.Interrupt();
                else
                    FileSeekFunc.thr?.Abort();
            }
            catch {}

        }
    }
}
