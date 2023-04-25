using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Messenger
{
    /// <summary>
    /// Логика взаимодействия для UserChat.xaml
    /// </summary>
    public partial class UserChat : Window
    {
        private Socket server;
        private string UserName;
        private bool isFirstInit = true;
        public UserChat(string username, string ipadress)
        {
            UserName = username;
            InitializeComponent();

            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                server.ConnectAsync(ipadress, 8888);
                RecieveMessage();
            }
            catch
            {
                MessageBox.Show("Неверный IP адрес");
                Close();
            }
        }

        private async Task RecieveMessage()
        {
            while (true)
            {
                if (isFirstInit)
                {
                    SendMessage("/username:" + UserName);
                    isFirstInit = false;
                }
                else
                {
                    byte[] bytes = new byte[1024];
                    await server.ReceiveAsync(bytes, SocketFlags.None);
                    string message = Encoding.UTF8.GetString(bytes).Split("\0")[0];
                    if (message.Contains("/updateUsers:"))//апдейт пользователей
                    {
                        var collection = message.Split("/updateUsers:")[1].Split(",");
                        Users.Items.Clear();
                        foreach (var item in collection)
                        {
                            if (item != "") { Users.Items.Add(item); }
                        }
                        Users.Items.Refresh();
                        continue;
                    }
                    if (message.Contains("/dcalldown"))
                    {
                        SendMessage("/disconnect/username:" + UserName);
                        this.Close();
                    }

                    if (message.Contains("/username:"))//ник к сообщению
                    {
                        string result = "";
                        var Splitted = message.Split("/username:");
                        for (int i = 0; i < Splitted.Length - 1; i++)
                        {
                            result += Splitted[i];
                        }
                        string username = Splitted[Splitted.Length - 1];
                        if (result != "") { ChatMessages.Items.Add($"[{username}] : {result}"); }
                    }
                    else
                    {
                        ChatMessages.Items.Add($"[{server.RemoteEndPoint}] : {message}");
                    }
                }
            }
        }
        private async Task SendMessage(string message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            await server.SendAsync(bytes, SocketFlags.None);
        }

        private void SendMessageBTN_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(MessageTXB.Text))
            {
                MessageBox.Show("Введите сообщение!");
                return;
            }
            if(MessageTXB.Text.Contains("/disconnect"))
            {
                SendMessage(MessageTXB.Text + "/username:" + UserName);
                this.Close();
            }
            SendMessage(MessageTXB.Text+"/username:"+UserName);
            MessageTXB.Text = "";
        }

        private void Leave_Click(object sender, RoutedEventArgs e)
        {
            SendMessage("/disconnect/username:"+UserName);
            this.Close();
        }

        private void UserChat_Closing(object sender, CancelEventArgs e)
        {
            SendMessage("/disconnect" + "/username:" + UserName);
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
