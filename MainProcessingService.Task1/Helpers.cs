using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MainProcessingService.Task1
{
    public static class Helpers
    {

        private static readonly Dictionary<string,List<byte[]>> TempChunksList = new Dictionary<string, List<byte[]>>();
        public static byte[] Combine(params byte[][] arrays)
        {
            var rv = new byte[arrays.Sum(a => a.Length)];
            var offset = 0;
            foreach (var array in arrays)
            {
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }

        public static void FileSaver(string currentFileName, string fileId, bool isLastChunk, byte[] chunk)
        {
            if(!TempChunksList.ContainsKey(fileId))
                TempChunksList.Add(fileId, new List<byte[]>());

            TempChunksList[fileId].Add(chunk);
            if(!isLastChunk) return;

            var result = Helpers.Combine(TempChunksList[fileId].ToArray());
            File.WriteAllBytes(Constants.Path + "\\" + currentFileName, result);
            TempChunksList.Remove(fileId);
        }

        public static void InitializeStatusSender(CancellationToken token)
        {
            Task.Factory.StartNew(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    Thread.Sleep(RabbitMqActions.StatusSendingRate);
                    RabbitMqActions.SendServiceStatus();
                }
            }, token);
        }
    }
}
