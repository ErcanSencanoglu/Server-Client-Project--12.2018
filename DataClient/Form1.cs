using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataClient
{
    public partial class Form1 : Form
    {
        TcpClient clientSocket = new TcpClient();
        String name;
        string durum = "00";
        string fileName = @"C:\Users\Ercan\Desktop";
        public Form1()
        {
            InitializeComponent();
            this.name = "xxx";
        }
        //delegate void TextDegisDelegate(string text);
        private void sendName(TcpClient client, String text)
        {
            durum = "00|";
            NetworkStream serverStream = client.GetStream();
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes(durum  + text + "$");
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();
        }
        private void sendAnswer(TcpClient client, String text,string answer)
        {
            durum = answer + "|";
            NetworkStream serverStream = client.GetStream();
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes(durum + text + "$");
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();
        }
        private String getName(NetworkStream n)
        {
            try
            {
                
                Byte[] bytesForm = new Byte[100024];
                String dataFromServer = String.Empty;
                NetworkStream ns = n;
                ns.Read(bytesForm, 0, (int)clientSocket.ReceiveBufferSize);
                dataFromServer = System.Text.Encoding.ASCII.GetString(bytesForm);
                dataFromServer = dataFromServer.Substring(0, dataFromServer.IndexOf("$"));
                if (dataFromServer.Substring(0, 2) == "01")
                {
                    dataFromServer = dataFromServer.Substring(3);
                    string isim = dataFromServer.Substring(0, dataFromServer.IndexOf('*'));
                    string boyut = dataFromServer.Substring(dataFromServer.IndexOf('*') + 1, (dataFromServer.IndexOf('/')-dataFromServer.IndexOf('*')-1));
                    string user = dataFromServer.Substring(dataFromServer.IndexOf('/') + 1);
                    DialogResult yn = MessageBox.Show(user + isim + " isimli " + boyut + " Bayt Boyutlu Dosyayı göndermek istiyor.", "Sure", MessageBoxButtons.YesNo);
                    if (yn == DialogResult.Yes)
                    {
                        /*FolderBrowserDialog file = new FolderBrowserDialog();
                        file.ShowDialog();
                        fileName = file.SelectedPath + isim;*/
                        richTextBox1.BeginInvoke((Action)delegate () { folderBrowserDialog1.ShowDialog(); });
                       
                        fileName = folderBrowserDialog1.SelectedPath +"\\"+ isim;
                        sendAnswer(clientSocket,"00","99");
                        dataFromServer = "---" + isim + " isimli ve " + boyut + " boyutlu dosya " + user + " tarafından alınıyor.";
                        return "";
                    }
                    else
                    {
                        sendAnswer(clientSocket, "", "98");
                        dataFromServer = "---Dosya reddedildi";
                    }
                   
                }
                else if (dataFromServer.Substring(0, 2) == "02")
                {
                    try
                    {
                        using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                        {
                            fs.Write(bytesForm, 0, bytesForm.Length);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception caught in process: {0}", ex);
                    }
                    dataFromServer = "---Dosya kaydedildi.";

                }
                dataFromServer = dataFromServer.Substring(3);
                return dataFromServer;
            }
            catch (Exception e)
            {

                MessageBox.Show(e.ToString());
                throw;
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
          
            name = txtName.Text;
            try
            {
                clientSocket = new TcpClient();
                clientSocket.Connect("127.0.0.1", 8888);
                sendName(clientSocket, name);
                if (getName(clientSocket.GetStream()) != "no")
                {
                    label1.Text = "Client Socket Prgram - Server Connected...";
                    this.Text = name;
                    button1.Enabled = false;
                    Thread clThread = new Thread(doChat);
                    clThread.Start();
                }
                else
                {
                    MessageBox.Show("Refused");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("File Sending fail." + ex.Message);
            } 
           



            
        }
        private void doChat()
        {
            NetworkStream ns2 = clientSocket.GetStream();
            while (true)
            {
                writing("Sunucu: " + getName(ns2));
            }

        }



        public void writing(string a)
        {
            //String b = a.Substring(a.Length - 1, 2);

                if (listBox2.InvokeRequired)
                {
                    //TextDegisDelegate del = new TextDegisDelegate(writing);
                    //label3.Invoke(del, new object[] { a });
                    listBox2.BeginInvoke((Action)delegate () { listBox2.Items.Add(a); });
                 }
                else
                {
                    listBox2.Items.Add(a);
                 }
           
         


        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            sendName(clientSocket, richTextBox1.Text);
            writing("Ben: " + richTextBox1.Text);
            richTextBox1.BeginInvoke((Action)delegate () { richTextBox1.Text = string.Empty; richTextBox1.Focus(); });
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // string index = listBox1.SelectedItem.ToString();
            //index = index.Substring(0, index.IndexOf('-'));//değişmesi gerekiyor.
            if(textBox1.Text.Trim() != "" && textBox2.Text.Trim() != "")
            {
                int index2 = Convert.ToInt32(textBox1.Text);
                sendFile(index2);
            }
            else { MessageBox.Show("Gerekli Alanları Doldurun.");textBox1.Focus(); }
            
        }
        private void sendFile(int index2)
        {
            string yol = openFileDialog1.FileName;
            NetworkStream serverStream = clientSocket.GetStream();

            ///byte[] outStream = System.Text.Encoding.ASCII.GetBytes(array + "$");
            FileInfo info = new FileInfo(yol);
            long dosyaBoyutu = info.Length;
            string fn = info.Name;
            //string index;
            //index = index.Substring(0, index.IndexOf('-'));//değişmesi gerekiyor.
            //index = " 1";//değişmesi gerekiyor.
            //int index2 = Convert.ToInt32(index);
            sendFileName(clientSocket, fn + "*" + dosyaBoyutu.ToString()+"<"+index2, yol);
        }
        private void sendFileName(TcpClient client, String text, string yol)
        {
            durum = "01|";
            NetworkStream serverStream = client.GetStream();
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes(durum + text + "$");
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();

            durum = "02|";
            byte[] array = System.Text.Encoding.ASCII.GetBytes(durum).Concat(ReadAllBytes(yol)).ToArray().Concat(System.Text.Encoding.ASCII.GetBytes("$")).ToArray();
            serverStream.Write(array, 0, array.Length);
            serverStream.Flush();

        }
        public byte[] ReadAllBytes(string fileName)
        {
            byte[] buffer = null;
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                buffer = new byte[fs.Length];
                fs.Read(buffer, 0, (int)fs.Length);
            }
            return buffer;
        }

       private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnDosyaSec_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            if (openFileDialog1.FileName != "openFileDialog1")
            { textBox2.Text = openFileDialog1.FileName; }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
