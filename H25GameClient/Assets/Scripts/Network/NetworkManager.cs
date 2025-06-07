using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnityEngine;

namespace amaz
{
    public class NetworkManager : BaseService
    {
        private UDPServer _server = null;
        private List<DataReceivedEventArgs> _receivedData = null;
        
        public NetworkManager()
        {
            _receivedData = new List<DataReceivedEventArgs>();
            
            _server = new UDPServer(8873);
            _server.DataReceived += OnDataReceivedInSubThread;
            _server.ErrorOccured += OnErrorOccured;
            _server.Start();
        }

        public void OnTick()
        {
            lock (_receivedData)
            {
                foreach (var msg in _receivedData)
                {
                    OnData(msg);
                }
                _receivedData.Clear();
            }
        }

        public void GetServerIPAndPort(out string ipAddress, out int port)
        {
            IPEndPoint localEndPoint = (IPEndPoint)_server.UdpClient.Client.LocalEndPoint;
            ipAddress = localEndPoint.Address.ToString();
            port = localEndPoint.Port;
        }
        
        public string GetLocalIPAddress()
        {
            try
            {
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    // 只考虑以太网、Wi-Fi 等可用的网络接口
                    if (nic.OperationalStatus == OperationalStatus.Up &&
                        (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                         nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211))
                    {
                        IPInterfaceProperties ipProps = nic.GetIPProperties();
                        foreach (UnicastIPAddressInformation ipInfo in ipProps.UnicastAddresses)
                        {
                            if (ipInfo.Address.AddressFamily == AddressFamily.InterNetwork &&
                                !IPAddress.IsLoopback(ipInfo.Address))
                            {
                                return ipInfo.Address.ToString();
                            }
                        }
                    }
                }
    
                return IPAddress.Loopback.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get Local IP Failed: {ex.Message}");
                return "Unknown";
            }
        }
        
        private void OnData(DataReceivedEventArgs data)
        {
            Debug.Log("[network_data]" + BitConverter.ToString(data.Data));
            RacingGame.Instance().GetService<EventDispatcher>().Dispatch(EventDefine.NETWORK_RECV_DATA,data);
        }

        public override void Dispose()
        {
            if (_server != null)
            {
                _server.DataReceived -= OnDataReceivedInSubThread;
                _server.ErrorOccured -= OnErrorOccured;
                _server.Dispose();
            }
        }

        private void OnDataReceivedInMainThread()
        {
            
        }

        private void OnDataReceivedInSubThread(object sender,DataReceivedEventArgs args)
        {
            lock (_receivedData)
            {
                _receivedData.Add(args);
            }
        }

        private void OnErrorOccured(object sender, ErrorEventArgs args)
        {
            
        }
    }
}
