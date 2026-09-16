using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClassLibrary_Message;
using Microsoft.EntityFrameworkCore;
using System.Linq;


namespace MessangerServer
{
    public partial class Form1 : Form
    {
        TcpListener listener;
        List<TcpClient> clients = new List<TcpClient>();
        Dictionary<string, TcpClient> onlineUsers = new Dictionary<string, TcpClient>();

       

        public SynchronizationContext uiContext;
        public Form1()
        {
            InitializeComponent();
            uiContext = SynchronizationContext.Current;
            buttonStop.Enabled = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private async void Receive(TcpClient tcpClient)
        {
            await Task.Run(async () =>
            {
                NetworkStream netstream = null;
                string userName = null;

                try
                {
                    netstream = tcpClient.GetStream();

                    byte[] arr = new byte[tcpClient.ReceiveBufferSize];

                    while (true)
                    {
                        int len = await netstream.ReadAsync(arr,0,tcpClient.ReceiveBufferSize);

                        if (len == 0)
                        {
                            if (userName != null && onlineUsers.ContainsKey(userName))
                            {
                                onlineUsers.Remove(userName);
                            }

                            netstream.Close();
                            tcpClient.Close();

                            return;
                        }

                        MemoryStream stream = new MemoryStream(arr, 0, len);

                        var jsonFormatter = new DataContractJsonSerializer(typeof(MessageTCP));

                        MessageTCP m = jsonFormatter.ReadObject(stream) as MessageTCP;

                        stream.Close();

                        userName = m.User;

                        MessangerContext context = new MessangerContext();

                        User user = context.Users.FirstOrDefault(u => u.UserName == m.User);

                        if (user == null)
                        {
                            user = new User
                            {
                                UserName = m.User,
                                Host = m.Host
                            };

                            context.Users.Add(user);
                        }
                        else
                        {
                            user.Host = m.Host;
                        }

                        context.SaveChanges();
                        context.Dispose();

                        if (!onlineUsers.ContainsKey(m.User))
                        {
                            onlineUsers.Add(m.User,tcpClient);
                        }

                        
                        if (m.Message == "GET_HISTORY")
                        {
                            string messages = "";

                            context = new MessangerContext();

                            User sender = context.Users.FirstOrDefault(u => u.UserName == m.User);

                            User receiver = context.Users.FirstOrDefault(u => u.UserName == m.Receiver);

                            var history =
                                context.Messages
                                .Include(x => x.Sender)
                                .Include(x => x.Receiver)
                                .Where(x =>
                                    (x.SenderId == sender.Id && x.ReceiverId == receiver.Id)||
                                    (x.SenderId == receiver.Id && x.ReceiverId == sender.Id))
                                .OrderBy(x => x.SendAt).ToList();

                            foreach (Message message in history)
                            {
                                messages += message.Sender.UserName +" -> " + message.Receiver.UserName +
                                    ": " + message.Text + Environment.NewLine;
                            }

                            context.Dispose();

                            byte[] msg = Encoding.UTF8.GetBytes(messages);

                            await netstream.WriteAsync(msg,0,msg.Length);
                        }

                      
                        else if (!string.IsNullOrWhiteSpace(m.Message))
                        {
                            context = new MessangerContext();

                            User sender = context.Users.FirstOrDefault(u => u.UserName == m.User);

                            User receiver =context.Users.FirstOrDefault(u => u.UserName == m.Receiver);

                            Message message = new Message
                                {
                                    SenderId = sender.Id,
                                    ReceiverId = receiver.Id,
                                    Text = m.Message,
                                    SendAt = m.SendAt
                                };

                            context.Messages.Add(message);
                            context.SaveChanges();
                            context.Dispose();

                          
                            if (onlineUsers.ContainsKey(m.Receiver))
                            {
                                TcpClient receiverClient = onlineUsers[m.Receiver];

                                NetworkStream receiverStream = receiverClient.GetStream();

                                MemoryStream messageStream = new MemoryStream();

                                var formatter = new DataContractJsonSerializer(typeof(MessageTCP));

                                formatter.WriteObject(messageStream,m);

                                byte[] messageBytes = messageStream.ToArray();

                                messageStream.Close();

                                await receiverStream.WriteAsync(messageBytes,0,messageBytes.Length);
                            }
                        }

                        string result = m.Host + " - " + m.User + " - " + m.Message;

                        uiContext.Send(
                            d => listBox1.Items.Add(result),
                            null);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Сервер: " + ex.Message);

                    netstream?.Close();
                    tcpClient?.Close();
                }
            });
        }




        private async void Accept()
        {
            await Task.Run(async () =>
            {
                try
                {
                    listener = new TcpListener(IPAddress.Any, 49152);
                    listener.Start();

                    uiContext.Post(d =>
                    {
                        buttonStart.Enabled = false;
                        buttonStop.Enabled = true;
                        label1.Text = "Статус: Сервер запущений";
                    }, null);
                    while (true)
                    {
                        TcpClient client = await listener.AcceptTcpClientAsync();
                        clients.Add(client);
                        Receive(client);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Сервер: " + ex.Message);
                }
            });
        }

        private async void buttonStart_Click(object sender, EventArgs e)
        {
            Accept();
        }
                

        private void buttonStop_Click(object sender, EventArgs e)
        {
            try
            {
                listener?.Stop();

                foreach (TcpClient client in clients)
                {
                    client.Close();
                }

                clients.Clear();

                buttonStart.Enabled = true;
                buttonStop.Enabled = false;

                label1.Text = "Статус: Сервер зупинений";
                listBox1.Items.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Сервер: " + ex.Message);
            }
        }
    }
}
