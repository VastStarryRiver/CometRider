using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Net;
using System.Net.Sockets;



namespace Invariable
{
    public partial class MessageNetManager : Singleton<MessageNetManager>
    {
        private Socket clientSocket;//客户端的Socket对象
        private IPEndPoint point;//服务器的端口对象
        private byte[] msgDataByte;//数据的字节数组
        private bool socketState = false;//客户端Socket的状态
        private Dictionary<string, List<Action<string>>> messageCallBacks = new Dictionary<string, List<Action<string>>>();



        /// <summary>
        /// 启动客户端并连接服务器
        /// </summary>
        public void PlaySocket()
        {
            if (clientSocket != null || string.IsNullOrEmpty(ConfigUtils.WebIpv4Str) || ConfigUtils.WebPortInt == 0)
            {
                return;
            }

            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress address = IPAddress.Parse(ConfigUtils.WebIpv4Str);

            point = new IPEndPoint(address, ConfigUtils.WebPortInt);

            //异步方法连接服务器端
            clientSocket.BeginConnect(point, HandlerConnect, clientSocket);

            //初始化字节数组
            msgDataByte = new byte[clientSocket.ReceiveBufferSize];

            //开始异步接收服务器端的数据
            clientSocket.BeginReceive(msgDataByte, 0, msgDataByte.Length, 0, HandlerReceive, clientSocket);
        }

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        public void StopSocket()
        {
            if (socketState)
            {
                socketState = false;

                clientSocket.Close();

                clientSocket = null;

                Debug.Log($"已经从{ConfigUtils.WebIpv4Str}:{ConfigUtils.WebPortInt}服务器端下线！");
            }
        }

        /// <summary>
        /// 重置连接
        /// </summary>
        public void ResetSocket()
        {
            StopSocket();
            PlaySocket();
        }

        /// <summary>
        /// 连接服务器的回调函数
        /// </summary>
        /// <param name="result"></param>
        private void HandlerConnect(IAsyncResult result)
        {
            if (clientSocket.Connected)
            {
                Socket tempSocket = (Socket)result.AsyncState;
                clientSocket.EndConnect(result);

                socketState = true;

                Debug.Log($"已经成功连接到{ConfigUtils.WebIpv4Str}:{ConfigUtils.WebPortInt}服务器端！");
            }
            else
            {
                Debug.Log($"连接{ConfigUtils.WebIpv4Str}:{ConfigUtils.WebPortInt}服务器端失败！");
            }
        }

        /// <summary>
        /// 接收到服务器数据的回调函数
        /// </summary>
        /// <param name="result"></param>
        private void HandlerReceive(IAsyncResult result)
        {
            if (!socketState)
            {
                return;
            }

            //接收到的数据长度
            int count = clientSocket.EndReceive(result);

            if (count == 0)
            {
                Debug.Log("接收到服务器的数据为空!");
                return;
            }

            //处理接收到的字节数组数据
            string str = Encoding.UTF8.GetString(msgDataByte, 0, count);

            if (str.Contains("|"))
            {
                string[] strings = str.Split('|');

                if (messageCallBacks.ContainsKey(strings[0]) && messageCallBacks[strings[0]].Count > 0)
                {
                    foreach (Action<string> callBack in messageCallBacks[strings[0]])
                    {
                        callBack.Invoke(strings[1]);
                    }
                }
            }

            //重置数据的字节数组
            msgDataByte = new byte[clientSocket.ReceiveBufferSize];

            //继续接收下一条数据
            clientSocket.BeginReceive(msgDataByte, 0, msgDataByte.Length, 0, HandlerReceive, clientSocket);
        }

        /// <summary>
        /// 发送数据到服务器端
        /// </summary>
        /// <param name="text"></param>
        public void Send(string text)
        {
            if (!socketState)
            {
                return;
            }

            //将要发送的数据转码为UTF8格式的字节数组
            byte[] message = Encoding.UTF8.GetBytes(text);

            try
            {
                clientSocket.BeginSend(message, 0, message.Length, 0, HandlerSend, clientSocket);//发送数据
            }
            catch
            {
                Debug.Log("客户端数据发送失败!");
            }
        }

        /// <summary>
        /// 数据成功发送到服务器之后的回调函数
        /// </summary>
        /// <param name="result"></param>
        private void HandlerSend(IAsyncResult result)
        {
            //发送的数据量
            int count = clientSocket.EndSend(result);
            Debug.Log("客户端数据发送成功!长度为:" + count);
        }

        public void BindReceiveMessage(string messageName, Action<string> callBack)
        {
            if (!messageCallBacks.ContainsKey(messageName))
            {
                messageCallBacks[messageName] = new List<Action<string>>();
            }

            if (!messageCallBacks[messageName].Contains(callBack))
            {
                messageCallBacks[messageName].Add(callBack);
            }
        }

        public void UnbindReceiveMessage(string messageName, Action<string> callBack)
        {
            if (messageCallBacks.ContainsKey(messageName) && messageCallBacks[messageName].Contains(callBack))
            {
                messageCallBacks[messageName].Remove(callBack);
            }
        }
    }
}