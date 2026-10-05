using ClassLibrary_Message;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MessangerServer
{
    public partial class Form1 : Form
    {
        private TcpListener listener;
        private List<TcpClient> clients = new List<TcpClient>();
        private Dictionary<string, TcpClient> onlineUsers = new Dictionary<string, TcpClient>();
        public SynchronizationContext uiContext;

        public Form1()
        {
            InitializeComponent();
            uiContext = SynchronizationContext.Current;
            buttonStop.Enabled = false;
        }


        private async Task SendText(TcpClient client, string text)
        {
            NetworkStream stream = client.GetStream();
            byte[] data = Encoding.UTF8.GetBytes(text + "\n");
            await stream.WriteAsync(data, 0, data.Length);
        }

        private MessageTCP DeserializeMessage(string json)
        {
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            DataContractJsonSerializer formatter = new DataContractJsonSerializer(typeof(MessageTCP));
            MessageTCP message = (MessageTCP)formatter.ReadObject(stream);
            stream.Close();
            return message;
        }

        private async void Receive(TcpClient tcpClient)
        {
            NetworkStream netstream = tcpClient.GetStream();
            StreamReader reader = new StreamReader(netstream, Encoding.UTF8);
            string userName = "";

            try
            {
                while (true)
                {
                    string json = await reader.ReadLineAsync();

                    if (json == null)
                    {
                        break;
                    }

                    MessageTCP m = DeserializeMessage(json);

                    userName = m.User;

                    using (MessangerContext context = new MessangerContext())
                    {
                        User user = context.Users.FirstOrDefault(x => x.UserName == m.User);

                        if (user == null)
                        {
                            user = new User();
                            user.UserName = m.User;
                            user.Host = m.Host;
                            context.Users.Add(user);
                        }
                        else
                        {
                            user.Host = m.Host;
                        }
                        context.SaveChanges();
                    }


                    bool newUser = !onlineUsers.ContainsKey(m.User);

                    onlineUsers[m.User] = tcpClient;

                    if (newUser)
                    {
                        await SendOnlineUsers();
                    }


                    if (m.Message == "GET_HISTORY")
                    {
                        await SendHistory(tcpClient, m.User, m.Receiver);
                    }


                    else if (m.Message != "")
                    {
                        await SaveAndSendMessage(m);
                    }

                    string result = m.Host + " - " + m.User + " - " + m.Message;

                    uiContext.Post(
                        d =>
                        {
                            listBox1.Items.Add(result);
                        },
                        null);
                }
            }
            catch
            {
            }

            if (userName != "")
            {
                onlineUsers.Remove(userName);
            }

            netstream.Close();
            tcpClient.Close();
            clients.Remove(tcpClient);
        }


        private async Task SendOnlineUsers()
        {
            foreach (var item in onlineUsers)
            {
                string currentUser = item.Key;
                TcpClient client = item.Value;
                string usersMessage = "USERS|";

                foreach (string user in onlineUsers.Keys)
                {

                    if (user != currentUser)
                    {
                        usersMessage += user + "|";
                    }
                }

                await SendText(client, usersMessage);
            }
        }
        private async Task SaveAndSendMessage(MessageTCP m)
        {
            using (MessangerContext context = new MessangerContext())
            {
                User sender = context.Users.FirstOrDefault(x => x.UserName == m.User);

                User receiver = context.Users.FirstOrDefault(x => x.UserName == m.Receiver);

                if (sender == null || receiver == null)
                {
                    return;
                }

                Message message = new Message();
                message.SenderId = sender.Id;
                message.ReceiverId = receiver.Id;
                message.Text = m.Message;
                message.SendAt = m.SendAt;

                context.Messages.Add(message);
                context.SaveChanges();
            }


            if (onlineUsers.ContainsKey(m.Receiver))
            {
                TcpClient receiverClient = onlineUsers[m.Receiver];
                await SendMessageToClient(receiverClient, m);
            }
        }


        private async Task SendMessageToClient(TcpClient client, MessageTCP message)
        {
            MemoryStream stream = new MemoryStream();
            DataContractJsonSerializer formatter = new DataContractJsonSerializer(typeof(MessageTCP));
            formatter.WriteObject(stream, message);
            string json = Encoding.UTF8.GetString(stream.ToArray());
            stream.Close();
            await SendText(client, "MESSAGE|" + json);
        }


        private async Task SendHistory(TcpClient tcpClient, string senderName, string receiverName)
        {
            using (MessangerContext context = new MessangerContext())
            {
                User sender = context.Users.FirstOrDefault(x => x.UserName == senderName);
                User receiver = context.Users.FirstOrDefault(x => x.UserName == receiverName);

                List<Message> history = context.Messages
                        .Include(x => x.Sender)
                        .Include(x => x.Receiver)
                        .Where(x =>
                            (x.SenderId == sender.Id && x.ReceiverId == receiver.Id) ||
                            (x.SenderId == receiver.Id && x.ReceiverId == sender.Id))
                        .OrderBy(x => x.SendAt)
                        .ToList();


                StringBuilder messages = new StringBuilder();

                foreach (Message message in history)
                {
                    string senderUserName = message.Sender.UserName;
                    string receiverUserName = message.Receiver.UserName;
                    string text = message.Text;
                    text = text.Replace("\r\n", "\\n");
                    text = text.Replace( "\n", "\\n");
                    messages.Append(senderUserName + " -> " + receiverUserName + ": " + text);
                    messages.Append("\\n");
                }

                await SendText(tcpClient, "HISTORY|" + messages.ToString());
            }
        }


        private async void Accept()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, 49152);
                listener.Start();

                buttonStart.Enabled = false;
                buttonStop.Enabled = true;

                label1.Text = "Статус: Сервер працює";

                while (true)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    clients.Add(client);
                    Receive(client);
                }
            }
            catch
            {
            }
        }


        private void buttonStart_Click(object sender, EventArgs e)
        {
            Accept();
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            listener.Stop();

            foreach (TcpClient client in clients)
            {
                client.Close();
            }

            clients.Clear();
            onlineUsers.Clear();
            buttonStart.Enabled = true;
            buttonStop.Enabled = false;
            label1.Text = "Статус: Сервер зупинено";
            listBox1.Items.Clear();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            listener?.Stop();
            foreach (TcpClient client in clients)
            {
                client.Close();
            }
            clients.Clear();
            onlineUsers.Clear();
        }

    }


}
