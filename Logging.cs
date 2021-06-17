/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Simulator.Bridge
{
    public class LoggingBridgeInstance : IBridgeInstance
    {
        Stream File;

        // for logging purposes bridge being "connected" means file is open
        public Status Status
            => File == null ? Status.Disconnected : Status.Connected;

        public LoggingBridgeInstance()
        {
        }

        // "connect" means open file for writing
        public void Connect(string connection)
        {
            if (File != null)
            {
                Disconnect();
            }

            try
            {
                var path = Path.Combine(Simulator.Web.Config.PersistentDataPath, connection + ".txt.gz");
                File = new GZipStream(new FileStream(path, FileMode.Create), CompressionMode.Compress, false);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return;
            }
        }

        // "disconnect" means close file
        public void Disconnect()
        {
            if (File != null)
            {
                // this prevents writes to be happening at the same time
                lock (this)
                {
                    File.Close();
                    File = null;
                }
            }
        }

        // our "publish" functionality will be just writing string to file
        // before each message we'll prepend line with data type & topic name
        // NOTE: this should be asynchronous for performance reasons
        public void Write(string type, string topic, string data, Action completed)
        {
            Task.Run(() =>
            {
                // this Write method can be called from multiple threads at the same time
                // we want to make sure that type/topic line and data line are next to each other
                lock (this)
                {
                    if (File != null)
                    {
                        var bytes = Encoding.UTF8.GetBytes($"{type} {topic}\n");
                        File.Write(bytes, 0, bytes.Length);

                        bytes = Encoding.UTF8.GetBytes(data);
                        File.Write(bytes, 0, bytes.Length);
                        File.WriteByte((byte)'\n');
                    }
                }

                completed();
            });
        }
    }
}
