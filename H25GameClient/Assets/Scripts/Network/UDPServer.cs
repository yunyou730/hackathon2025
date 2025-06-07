using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

namespace amaz
{
    public class UDPServer : IDisposable
    {
        private UdpClient _udpClient;
        private Thread _receiveThread;
        private bool _isRunning;
        private readonly int _port;
        
        public bool IsRunning => _isRunning;
        public UdpClient UdpClient => _udpClient;
        
        public event EventHandler<DataReceivedEventArgs> DataReceived;
        public event EventHandler<ErrorEventArgs> ErrorOccured;

        public UDPServer(int port)
        {
            _port = port;
        }

        public void Start()
        {
            if (_isRunning)
            {
                return;
            }

            try
            {
                _udpClient = new UdpClient(_port);
                _udpClient.Client.ReceiveBufferSize = 65536;

                _receiveThread = new Thread(ReceiveDataThread);
                _receiveThread.IsBackground = true;
                _receiveThread.Start();
                
                _isRunning = true;
                Debug.Log("UDP Server is running");
            }
            catch (Exception e)
            {
                Debug.Log("UDP Server launch failed:" + e);
                throw;
            }
        }

        public void Stop()
        {
            if (!_isRunning)
                return;
            
            _isRunning = false;
            try
            {
                _receiveThread?.Abort();
                _udpClient?.Close();
                Debug.Log("UDP Server is stopped");
            }
            catch (Exception e)
            {
                Debug.Log("UDP Server stop failed:" + e);
                ErrorOccured?.Invoke(this, new ErrorEventArgs(e));
            }
        }
        

        private void ReceiveDataThread()
        {
            while (_isRunning)
            {
                try
                {
                    IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                    byte[] receiveBytes = _udpClient.Receive(ref remote);
                    DataReceived?.Invoke(this,new DataReceivedEventArgs
                    {
                        Data = receiveBytes,
                        RemoteEndPoint = remote,
                        //Message = Encoding.UTF8.GetString(receiveBytes)
                    });
                }
                catch (SocketException ex)
                {
                    if (_isRunning && ex.SocketErrorCode != SocketError.Interrupted)
                    {
                        Console.WriteLine($"Receiving data socket error: {ex.Message}");
                        ErrorOccured?.Invoke(this, new ErrorEventArgs(ex));
                    }
                }
                catch (ThreadAbortException ex)
                {
                    Console.WriteLine($"UDP Thread abort: {ex.Message}");
                    break;
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                    {
                        Console.WriteLine($"Receive data unknown error: {ex.Message}");
                        ErrorOccured?.Invoke(this, new ErrorEventArgs(ex));
                    }
                }

            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
                _udpClient.Dispose();
            }
        }
    }

    public class DataReceivedEventArgs : EventArgs
    {
        public byte[] Data { get; set; }
        public IPEndPoint RemoteEndPoint { get; set; }
        //public string Message { get; set; }
    }

    public class ErrorEventArgs : EventArgs
    {
        public Exception Exception { get; set; }
        public ErrorEventArgs(Exception ex)
        {
            Exception = ex;
        }
    }
}

