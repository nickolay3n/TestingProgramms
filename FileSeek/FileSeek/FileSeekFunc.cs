using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text;

namespace FileSeek
{
    [Serializable]
    public  class FileSeekFunc
    {
        public static Thread thr;
        public static ManualResetEvent myResetEvent;

        public static DateTime timer;//ТАМЙМЕР ОТСЧЕТА
        public static int fileCount;//СЧЕТЧИК ФАЙЛОВ

        public delegate void AccountHandler(int fileCount, string message);
        public static AccountHandler Notify;
        public static void NotifyDisplayMessage( int fileCount, string message)
        {
            Dispatcher.Invoke(Program.F1, () =>
            {
                Program.F1.label4.Text = (DateTime.Now - timer).ToString() + " ОБРАБОТАНО ФАЙЛОВ: " + fileCount.ToString() + " обрабатывается файл: " + message;
            });

        }
        public static void fileSeeker(object state)
        {
            FileSeekFunc.timer = DateTime.Now;
            FileSeekFunc.fileCount = 0;
            Notify += NotifyDisplayMessage;
            #region проверка правописания маски файла
            string file_mask = Program.F1.textBox2.Text;
            foreach (Char invalid_char in Path.GetInvalidFileNameChars())
            {
                if(invalid_char!='?' && invalid_char != '*')
                    file_mask = file_mask.Replace(invalid_char.ToString(), "");
            }
            Dispatcher.Invoke(Program.F1, () =>
            {
                Program.F1.textBox2.Text = file_mask;
            });
            #endregion
            ManualResetEvent MRE = (ManualResetEvent)state;
             MRE.WaitOne();
            List<List<string>> myfileheap = new List<List<string>>();
            //добавил чтобы просто была коллекция файлов и их можно было куда то еще деть\обработать, можно убрать
            try
            {
                dirseeker(Program.F1.textBox1.Text, myfileheap);
            }
            catch
            {
                
                //MessageBox.Show(excpt.Message, "Error", MessageBoxButtons.YesNo);
            }
            Notify?.Invoke(FileSeekFunc.fileCount, "Поиск Окончен");
            //MessageBox.Show("jr", "Error", MessageBoxButtons.YesNo);
        }

        public static void dirseeker(string rootdir, List<List<string>> myfileheap)
        {
            #region проходим все папки, на каждый лист дерева папок запускаем поиск файлов в папке
            if (!Directory.Exists(rootdir))//папка сущетсвует??
            {
                MessageBox.Show("Folder not exists", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try { inDirFileSeek(rootdir, myfileheap); }//ищем и в корне тоже
            catch { }
            string[] directories = Directory.GetDirectories(rootdir, "*", SearchOption.AllDirectories);
            foreach (string podDir in directories)
                try { inDirFileSeek(podDir, myfileheap); }
                catch { }

            #endregion
        }

        public static void inDirFileSeek(string rootdir, List<List<string>> myfileheap)
        {
            #region поиск файлов в папке

                        string maska = (Program.F1.textBox2.Text=="")?"*.*" : Program.F1.textBox2.Text;
            //foreach (string f in GetFilesSafe(rootdir, maska))
            
            foreach (string f in Directory.GetFiles(rootdir, maska, SearchOption.AllDirectories)) 
            {

                //ОТОБРАЖАЕМ ФАЙЛ В ДЕРЕВО
                Notify?.Invoke(FileSeekFunc.fileCount, f);
                FileSeekFunc.fileCount++;

                if (Program.F1.textBox3.Text == "" ||  fileContainsString(f)) //если файл содержит строку, добавляем его в дерево
                {
                    
                    treeViewAddElement(f);
                    
                    /*List<string> file = new List<string>();
                    file.Add(f);
                    file.Add(rootdir);
                    myfileheap.Add(file);*/
                }
            }
            #endregion
        }

        //public static async Task<bool> fileContainsString(string path)
        /*вариант с посимвольным сравниванием
        public static bool fileContainsString(string path)
        {
            #region ПРОВЕРЯЕМ ЕСТЬ ЛИ В ФАЙЛЕ ИСКОМЫЙ ТЕКСТ
            try
            {
                if (Program.F1.textBox3.Lines.Length == 0) return true;//если в текст боксе пусто
                using (StreamReader sr = new StreamReader(path))
                {
                    int i = 0;//char position in textbox
                    while (!sr.EndOfStream)//посимвольное сравнивание
                    {
                        char tempFileChar = (char)sr.Read();//читаем символ
                        
                        if (tempFileChar == Program.F1.textBox3.Text[i] && i == (Program.F1.textBox3.Text.Length - 1))//символ равен и дошли до конца текстбокса
                            return true;
                        
                        if (tempFileChar == Program.F1.textBox3.Text[i])
                            i++;//символ равен, переходим на следущую позицию символа в текстбоксе
                        else i = 0;//несовпадение, текстбокс законо сравнивается
                    }
                }
            }
            catch 
            {
                //ловим ошибки на чтение
                return false;
            }
            return false;//цикл завершился не найдя последовательно все строчки из текстбокса
            #endregion
        }*/

        public static bool fileContainsString(string path)
        {
            #region ПРОВЕРЯЕМ ЕСТЬ ЛИ В ФАЙЛЕ ИСКОМЫЙ ТЕКСТ
            try
            {
                
               return File.ReadAllText(path).Contains(Program.F1.textBox3.Text);
                    
            }
            catch
            {
                //ловим ошибки на чтение
                return false;
            }
            return false;//цикл завершился не найдя последовательно все строчки из текстбокса
            #endregion
        }
        /*вариант не работающий старый
        public static bool fileContainsString(string path)
        {
            #region ПРОВЕРЯЕМ ЕСТЬ ЛИ В ФАЙЛЕ ИСКОМЫЙ ТЕКСТ
            try
            {
                if (Program.F1.textBox3.Lines.Length == 0) return true;//если в текст боксе пусто
                using (StreamReader sr = new StreamReader(path))
                {
                    while (!sr.EndOfStream)
                    {
                        if (sr.ReadLine().Contains(Program.F1.textBox3.Lines[0]))//найдено вхождение первой строки
                        {
                            for (int j = 1; j < Program.F1.textBox3.Lines.Length; j++)
                            {
                                if (sr.ReadLine().Contains(Program.F1.textBox3.Lines[j]))
                                {
                                    if (j == (Program.F1.textBox3.Lines.Length - 1))
                                        return true;//цикл сравнил все строки из текст бокса и все тру
                                }
                                else
                                {
                                    if (sr.ReadLine().Contains(Program.F1.textBox3.Lines[j + 1]))
                                        continue;//цикл нашел очереную строчку, идем дальше
                                    else
                                        break;//не все строчки были найдены
                                }

                            }
                        }
                    }
                }
            }
            catch
            {
                //ловим ошибки на чтение
                return false;
            }
            return false;//цикл завершился не найдя последовательно все строчки из текстбокса
            #endregion
        }*/

        public static void treeViewAddElement(string f)
        {
            #region ДОБАВЛЯЕМ ЭЛЕМЕНТ В ДЕРЕВО
            if (f.Length == 0) return;
            string[] strNodes = f.Split('\\');
            Dispatcher.Invoke(Program.F1, () =>
            {
                if (Program.F1.treeView1.Nodes.Count == 0 && strNodes.Length!=0)
                    Program.F1.treeView1.Nodes.Add(strNodes[0]);//добавляем корень
                TreeNode temproot= (Program.F1.treeView1.Nodes.Count == 0)?null : Program.F1.treeView1.Nodes[0];//временный указатель на начала дерева
                for (int i =0;i< strNodes.Length;i++)
                {
                    TreeNode tempFLAG = null;
                    foreach (TreeNode temp in temproot.Nodes)
                    {
                        if ((i+1) < strNodes.Length)//проверяем есть ли узел выше в строке файла
                        if (temp.Text == strNodes[i+1])//переход на уровень выше если такой узел есть
                        {
                            temproot = temp;
                            tempFLAG = temp; 
                            break;
                        }
                    }
                    if (tempFLAG == null && ((i + 1) < strNodes.Length))//если такого узла нету то добавляем его
                    {
                        temproot.Nodes.Add(strNodes[i+1]);
                        foreach (TreeNode temp in temproot.Nodes)
                        {
                            if ((i + 1) < strNodes.Length)//проверяем есть ли узел выше в строке файла
                                if (temp.Text == strNodes[i + 1])//переход на уровень выше если такой узел есть
                                {
                                    temproot = temp;
                                    tempFLAG = temp;
                                    break;
                                }
                        }
                    }  
                }
                Program.F1.treeView1.ExpandAll();
            });
            #endregion
        }

    }




    public delegate void AsyncAction();

    public delegate void DispatcherInvoker(Form form, AsyncAction a);

    public class Dispatcher
    {
        public static void Invoke(Form form, AsyncAction action)
        {
            if (!form.InvokeRequired)
            {
                action();
            }
            else
            {
                form.Invoke((DispatcherInvoker)Invoke, form, action);
            }
        }
    }
    
}
