﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Messenger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CreateNew_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(Username.Text))
            {
                MessageBox.Show("Вы не ввели никнейм!");
                return;
            }
            CreateChat CreatedChat = new CreateChat(Username.Text);
            CreatedChat.Show();
            this.Close();
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if(String.IsNullOrEmpty(Username.Text) || String.IsNullOrEmpty(Ip_adress.Text))
            {
                MessageBox.Show("Введите никнейм или IP-адресс!");
                return;
            }
            UserChat UserConnectedChat = new UserChat(Username.Text, Ip_adress.Text);
            UserConnectedChat.Show();
            this.Close();
        }
    }
}
