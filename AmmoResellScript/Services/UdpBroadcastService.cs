using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AmmoResellScript.Services
{
    public static class UdpBroadcastService
    {
        // 广播端口
        private const int BroadcastPort = 8888;

        /// <summary>
        /// 发送 UDP 广播消息
        /// </summary>
        /// <param name="message">要广播的消息内容</param>
        public static void SendBroadcast(string message)
        {
            try
            {
                // 创建 UDP 客户端（允许广播）
                using (UdpClient udpClient = new UdpClient { EnableBroadcast = true })
                {
                    // 转换消息为字节数组
                    byte[] data = Encoding.UTF8.GetBytes(message);

                    // 广播地址（255.255.255.255 为全网广播，也可替换为具体网段如 192.168.1.255）
                    IPEndPoint broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, BroadcastPort);

                    // 发送广播
                    udpClient.Send(data, data.Length, broadcastEndPoint);
                }
            }
            catch (Exception ex)
            {
                // 捕获异常（可根据需要记录日志）
                Console.WriteLine($"UDP 广播失败：{ex.Message}");
            }
        }
    }
}