using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Wonder.UWP.Services
{
    class TcpMonitorService : BaseMonitorService
    {
        private readonly TcpListener mListener;
        private readonly IList<TcpClient> mClients;

        public TcpMonitorService()
        {
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            int port = 12138;
            mListener = new TcpListener(localAddr, port);
            mClients = new List<TcpClient>();
        }

        protected override Task<bool> TryStartAsync()
        {
            try
            {
                mListener.Start();
                Accept();
                return Task.FromResult(true);
            }
            catch (Exception ex) when (ex is SocketException)
            {
                return Task.FromResult(false);
            }
        }

        private async void Accept()
        {
            try
            {
                while (true)
                {
                    var client = await mListener.AcceptTcpClientAsync();
                    mClients.Add(client);
                    var task = CommunicateAsync(client);
                }
            }
            catch (Exception ex) when (ex is SocketException)
            {
                // Do nothing...
            }
            finally
            {
                this.Stop();
            }
        }

        private async Task CommunicateAsync(TcpClient client)
        {
            try
            {
                var stream = client.GetStream();
                byte[] buffer = new byte[256];
                int i;
                while ((i = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    byte[] value = new byte[i];
                    Array.Copy(buffer, 0, value, 0, value.Length);
                    //bytes.CopyTo(value, 0);
                    this.RaiseValueChanged(value);
                }
            }
            catch (Exception ex) when (ex is SocketException)
            {
                mClients.Remove(client);
            }
        }

        protected override async Task TrySendAsync(byte[] value)
        {
            foreach (var client in mClients)
            {
                try
                {
                    var stream = client.GetStream();
                    await stream.WriteAsync(value, 0, value.Length);
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }
        }

        protected override bool TryStop()
        {
            try
            {
                foreach (var client in mClients)
                {
                    client.Close();
                    client.Dispose();
                }
                mClients.Clear();
                mListener.Stop();
                return true;
            }
            catch (Exception ex) when (ex is SocketException)
            {
                return false;
            }
        }
    }
}
