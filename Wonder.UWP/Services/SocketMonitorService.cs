using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace Wonder.UWP.Services
{
    class SocketMonitorService : BaseMonitorService
    {
        private StreamSocketListener mListener;
        private readonly IList<StreamSocket> mSockets;

        public SocketMonitorService()
        {
            mSockets = new List<StreamSocket>();
        }

        protected override async Task<bool> TryStartAsync()
        {
            try
            {
                mListener = new StreamSocketListener();
                mListener.ConnectionReceived += OnConnectionReceived;
                var localServiceName = "12138";
                await mListener.BindServiceNameAsync(localServiceName);
                return true;
            }
            catch (Exception ex) when (ex is ObjectDisposedException)
            {
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async void OnConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            try
            {
                mSockets.Add(args.Socket);
                var stream = args.Socket.InputStream.AsStreamForRead();
                var buffer = new byte[256];
                int i;
                while ((i = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    var value = new byte[i];
                    Array.Copy(buffer, 0, value, 0, value.Length);
                    RaiseValueChanged(value);
                }
            }
            catch (Exception ex) when (ex is IOException)
            {
                // 连接中断
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                mSockets.Remove(args.Socket);
            }
        }

        protected override async Task TrySendAsync(byte[] value)
        {
            foreach (var socket in mSockets)
            {
                try
                {
                    var stream = socket.OutputStream.AsStreamForWrite();
                    await stream.WriteAsync(value, 0, value.Length);
                    await stream.FlushAsync();
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }
        }

        protected override bool TryStop()
        {
            foreach (var socket in mSockets)
            {
                socket.Dispose();
            }
            mSockets.Clear();
            mListener.ConnectionReceived -= OnConnectionReceived;
            mListener.Dispose();
            mListener = null;
            return true;
        }
    }
}
