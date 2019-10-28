using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _DataServer
{
    public partial class Form1 : Form
    {
        delegate void TextDegisDelegate(string textt);
        TcpListener serverSocket = new TcpListener(8888);
        TcpClient clientSocket = default(TcpClient);
        public static List<TcpClient> cList = new List<TcpClient>();
        public static HandleClient[] hList = new HandleClient[100];
        public static int kisi = 0;
        public static ListBox lst1,lst2;
        static string durum = "00";
        public Form1()
        {
            InitializeComponent();
            lst1 = listBox1;
            lst2 = listBox2;

        }
      


        public void waitForClients()
        {
            while (true)
            {

                clientSocket = serverSocket.AcceptTcpClient();
                String name = getName(clientSocket);
                DialogResult yn = MessageBox.Show(name + " wants to connect server", "Sure", MessageBoxButtons.YesNo);
                if (yn == DialogResult.Yes)
                {
                    cList.Add(clientSocket);
                    writing(kisi + "-" + name);
                    sendName(clientSocket, "&");
                    Thread.Sleep(20);
                    sendName(clientSocket, ("//" + kisi+1));
                    HandleClient hc = new HandleClient();
                     hList[kisi++] = hc;
                     hc.startClient(clientSocket, name, (kisi - 1));

                }
                else
                {
                    sendName(clientSocket, ("no"));
                    clientSocket.Close();
                }
            } } 

        private String getName(TcpClient c)
        {
            try
            {
                Byte[] bytesForm = new Byte[100024];
                String dataFromClient = String.Empty;
                NetworkStream ns = c.GetStream();
                ns.Read(bytesForm, 0, (int)c.ReceiveBufferSize);
                dataFromClient = System.Text.Encoding.ASCII.GetString(bytesForm);
                dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                //
                dataFromClient = dataFromClient.Substring(3);
                return dataFromClient;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                throw;
            }

        }

        private static bool getAnswer(TcpClient c)
        {
            try
            {
                Byte[] bytesForm = new Byte[100024];
                String dataFromClient = String.Empty;
                Thread.Sleep(200);
                NetworkStream ns = c.GetStream();
                ns.Read(bytesForm, 0, (int)c.ReceiveBufferSize);
                dataFromClient = System.Text.Encoding.ASCII.GetString(bytesForm);
                dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                //
                if (dataFromClient.Substring(0, 2) == "99")
                {
                    return true;
                }
                else if (dataFromClient.Substring(0, 2) == "98")
                {
                    return false;
                }
                return false;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                throw;
            }

        }

        private void sendName(TcpClient client, String text)
        {
            durum = "00|";
            NetworkStream serverStream = client.GetStream();
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes(durum + text + "$");
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();
        }


        public void writing(string a)
        {
            if (listBox1.InvokeRequired)
            {
                TextDegisDelegate del = new TextDegisDelegate(writing);
                listBox1.Invoke(del, new object[] { a });
            }
            else
            {
                listBox1.Items.Add(a);
            }

        }


        private void Form1_Load(object sender, EventArgs e)
        {
            serverSocket = new TcpListener(8888);
            clientSocket = default(TcpClient);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            serverSocket.Start();
            button2.Enabled = true;
            Thread waitC = new Thread(waitForClients);
            waitC.Start();
        }



        public void writing2(string a)
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

        public static byte[] ReadAllBytes(string fileName)
        {
            byte[] buffer = null;
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                buffer = new byte[fs.Length];
                fs.Read(buffer, 0, (int)fs.Length);
            }
            return buffer;
        }

        private static void sendFileName(TcpClient client, String text,string yol)
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

        public static void sendFileName2(TcpClient client, String text, Byte[] by)
        {
            durum = "01|";
            NetworkStream serverStream = client.GetStream();
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes(durum + text + "$");
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();
   
                durum = "02|";
                byte[] array = System.Text.Encoding.ASCII.GetBytes(durum).Concat(by).ToArray().Concat(System.Text.Encoding.ASCII.GetBytes("$")).ToArray();
                serverStream.Write(array, 0, array.Length);
                serverStream.Flush();
            
        }

        private static void sendFile(TcpClient client,string yol)
        {
            NetworkStream serverStream = client.GetStream();
            
            ///byte[] outStream = System.Text.Encoding.ASCII.GetBytes(array + "$");
            FileInfo info = new FileInfo(yol);
            long dosyaBoyutu = info.Length;
            string fn = info.Name;
            sendFileName(client,fn+"*"+ dosyaBoyutu.ToString()+"/Sunucu",yol);
        }



        private void btnDosyaSec_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            if (openFileDialog1.FileName != "openFileDialog1")
            { textBox2.Text = openFileDialog1.FileName; }
        }

        private void btnDosyaGonder_click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1 && textBox2.Text.Trim() != "")
            {
                string index = listBox1.SelectedItem.ToString();
                index = index.Substring(0, index.IndexOf('-'));
                int index2 = Convert.ToInt32(index);
                string yol = openFileDialog1.FileName;
                sendFile(cList[index2],yol);
            }
            else
            {
                MessageBox.Show("Kullanıcı Seçin.");
            }
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            if (richTextBox1.Text.Trim() != "")
            { 
                if (listBox1.SelectedIndex != -1)
                {
                    string index = listBox1.SelectedItem.ToString();
                    index = index.Substring(0, index.IndexOf('-'));
                    int index2 = Convert.ToInt32(index);
                    sendName(cList[index2], richTextBox1.Text);
                    writing2("Sunucu: >>"+listBox1.SelectedItem.ToString()+" : " + richTextBox1.Text);
                    richTextBox1.BeginInvoke((Action)delegate () { richTextBox1.Text = string.Empty; richTextBox1.Focus(); });
                }
                else
                {
                    MessageBox.Show("Kullanıcı Seçin.");
                }
            }
        }
    }







    public class HandleClient
    {


        TcpClient clientSocket;
        //
        string clNo = String.Empty;
        int index = -1;
        bool dongu = true;
       
        public void startClient(TcpClient inClientSocket, String clNo, int indexNo)
        {
            this.clientSocket = inClientSocket;
            //
            this.clNo = clNo;
            this.index = indexNo;
            this.dongu = true;
            Thread clThread = new Thread(doChat);
            clThread.Start();
        }
        public void writeLabel(string s)
        {
            Form1.lst2.BeginInvoke((Action)delegate () { Form1.lst2.Items.Add(s); });
        }


        private void doChat()
        {
            Byte[] bytesForm = new byte[100024];
            Byte[] sendBytes = null;
            String dataFromClient = String.Empty;
            string serverResponse = null;
            string cn = "",text= "", fisim="" , fboyut="";


            try
            {
                while (dongu)
                {
                    if (clientSocket.Connected)
                    {
                        NetworkStream ns = clientSocket.GetStream();
                        try
                        {
                            ns.Read(bytesForm, 0, (int)clientSocket.ReceiveBufferSize);
                            dataFromClient = System.Text.Encoding.ASCII.GetString(bytesForm);
                            dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                            
                            if(dataFromClient.Substring(0,2) == "00")
                            {
                                writeLabel(clNo + ": " + dataFromClient);
                            }else if(dataFromClient.Substring(0,2) == "01")
                            {
                                dataFromClient = dataFromClient.Substring(3);
                                int a1 = dataFromClient.IndexOf('*');
                                int a2 = dataFromClient.IndexOf('<');
                                fisim = dataFromClient.Substring(0,a1);
                                fboyut = dataFromClient.Substring(a1+1, a2-a1-1);
                                cn = dataFromClient.Substring(a2+1);
                                text = clNo + " " + fisim + " ismili " + fboyut + " Bayt Boyutlu Dosyayı " + cn + "\'na göndermek göndermek istiyor.";
                                DialogResult yn = MessageBox.Show(text, "Sure", MessageBoxButtons.YesNo);
                                if (yn == DialogResult.Yes)
                                {
                                    
                                }
                                else
                                {
                                    //sendAnswer(clientSocket, "", "98");
                                }
                            }
                            else if(dataFromClient.Substring(0,2) == "02")
                            {
                                string text2 = fisim + "*" + fboyut.ToString()+"/"+(Form1.hList[int.Parse(cn)].clNo.ToString());
                                Form1.sendFileName2(Form1.cList[int.Parse(cn)], text2, bytesForm.ToArray());
                                text = cn = "";
                            }
                        }
                        catch (System.IO.IOException)
                        {
                            clientSocket.Close();
                            Form1.cList[Form1.cList.IndexOf(clientSocket)] = null;
                            Form1.hList[index] = null;
                            Form1.lst1.BeginInvoke((Action)delegate ()
                            {
                                int a = Form1.lst1.Items.IndexOf(index + "-" + clNo);
                                Form1.lst1.Items.Remove(index + "-" + clNo); Form1.lst1.Refresh();
                            });
                            dongu = true;
                            break;

                        }
                    }
                }

             }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

    }
}
