using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
    /// Логика взаимодействия для CreateChat.xaml
    /// </summary>
    public partial class CreateChat : Window
    {

        private Socket socket;

        string UserNameAdmin;

        List<Socket> clients = new List<Socket>();

        List<string> logs = new List<string>();

        public CreateChat(string username)
        {
            UserNameAdmin = username;
            InitializeComponent();
            Users.Items.Add(UserNameAdmin);
            IPEndPoint iPPoint = new IPEndPoint(IPAddress.Any, 8888);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(iPPoint);
            socket.Listen(10);// <-  users count
            logs.Add("Server-start at: " + DateTime.Now + " by user : " + UserNameAdmin);
            AwaitClients();
           
        }

        private async Task AwaitClients()
        {
            while (true)
            {
                var client = await socket.AcceptAsync();
                clients.Add(client);
                RecieveMessage(client);
            }
        }

        private async Task RecieveMessage(Socket client)
        {
            while (true)
            {
                byte[] bytes = new byte[1024];
                await client.ReceiveAsync(bytes, SocketFlags.None);
                string message = Encoding.UTF8.GetString(bytes).Split("\0")[0];

                if (message.Contains("/disconnect"))
                {
                    var Splitted = message.Split("/username:");
                    string username = Splitted[Splitted.Length - 1];
                    clients.Remove(client);
                    Users.Items.Remove(username);
                    logs.Add($"user [{username}] deleted at {DateTime.Now}");
                    Users.Items.Refresh();
                    string send = "/updateUsers:";
                    foreach (var item in Users.Items)
                    {
                        send += (item + ",");
                    }
                    foreach (var clientUpdate in clients)
                    {
                        SendMessage(clientUpdate, send);
                    }
                    return;
                }

                if (message.Contains("/username:"))
                {
                    string result = "";
                    var Splitted = message.Split("/username:");
                    for(int i = 0; i < Splitted.Length-1; i++)
                    {
                        result += Splitted[i];
                    }
                    string username = Splitted[Splitted.Length-1];
                    if (!Users.Items.Contains(username))
                    {
                        Users.Items.Add(username);
                        logs.Add($"new [user] : {username} at " + DateTime.Now);
                    }
                    if(result != "") { ChatMessages.Items.Add($"[{username}] : {result}"); logs.Add($"new message from: {username} : [{result}] at {DateTime.Now}"); }
                }
                else
                {
                    ChatMessages.Items.Add($"[{client.RemoteEndPoint}] : {message}");
                    logs.Add($"new message from [{client.RemoteEndPoint}] : [{message}] at {DateTime.Now}");
                }

                
                foreach (var item in clients)
                {
                    SendMessage(item, message);
                }
            }
        }
        private async Task SendMessage(Socket client, string message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            await client.SendAsync(bytes, SocketFlags.None);
        }

        private void SendMessageBTN_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(MessageTXB.Text))
            {
                MessageBox.Show("Введите сообщение !");
                return;
            }
            if (MessageTXB.Text.Contains("/disconnect"))
            {
                this.Close();
            }
            var message = MessageTXB.Text;
            ChatMessages.Items.Add($"[{UserNameAdmin}] : {message}");
            logs.Add($"Server {UserNameAdmin} sent message : [ {message} ] at {DateTime.Now}");
            MessageTXB.Text = "";
            message += ("/username:" + UserNameAdmin);
            foreach (var item in clients)
            {
                SendMessage(item, message);
            }
        }

        private void Users_LayoutUpdated(object sender, EventArgs e)
        {
            string send = "/updateUsers:";
            foreach (var item in Users.Items)
            {
                send += (item + ",");
            }
            foreach (var client1 in clients)
            {
                SendMessage(client1, send);
            }
        }

        private void CheckLogs_Click(object sender, RoutedEventArgs e)
        {
            LogsDialog logsDialogWindow = new LogsDialog(logs);
            logsDialogWindow.ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (var client1 in clients)
            {
                SendMessage(client1, "/dcalldown");
            }
            socket.Close();
            MainWindow MW = new MainWindow();
            MW.Show();
        }
    }
}
