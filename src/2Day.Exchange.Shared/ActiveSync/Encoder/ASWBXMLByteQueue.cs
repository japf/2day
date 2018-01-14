using System.Collections.Generic;
using System.Text;

namespace Chartreuse.Today.Exchange.ActiveSync
{
    class ASWBXMLByteQueue : Queue<byte>
    {
        public ASWBXMLByteQueue(byte[] bytes)
            : base(bytes)
        {
        }

        public int DequeueMultibyteInt()
        {
            int iReturn = 0;
            byte singleByte = 0xFF;

            do
            {
                iReturn <<= 7;

                singleByte = this.Dequeue();
                iReturn += (int)(singleByte & 0x7F);
            }
            while (this.CheckContinuationBit(singleByte));

            return iReturn;
        }

        private bool CheckContinuationBit(byte byteval)
        {
            byte continuationBitmask = 0x80;
            return (continuationBitmask & byteval) != 0;
        }

        public string DequeueString()
        {
            byte currentByte;
            List<byte> bytesString = new List<byte>();
            do
            {
                currentByte = this.Dequeue();
                bytesString.Add(currentByte);
            }
            while (currentByte != 0x00);

            return Encoding.UTF8.GetString(bytesString.ToArray(), 0, bytesString.Count -1);
        }

        public string DequeueString(int length)
        {
            List<byte> bytesString = new List<byte>();
            for (int i = 0; i < length; i++)
            {
                bytesString.Add(this.Dequeue());
            }

            return Encoding.UTF8.GetString(bytesString.ToArray(), 0, bytesString.Count);
        }
    }
}