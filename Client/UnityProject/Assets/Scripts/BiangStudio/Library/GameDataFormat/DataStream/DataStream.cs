using System;
using System.IO;

namespace BiangStudio.GameDataFormat
{
    public class DataStream
    {
        private BinaryReader mBinReader;
        private BinaryWriter mBinWriter;
        private MemoryStream mMemStream;
        private bool mBEMode; //big endian mode

        public DataStream(bool isBigEndian)
        {
            mMemStream = new MemoryStream();
            InitWithMemoryStream(mMemStream, isBigEndian);
        }

        public DataStream(byte[] buffer, bool isBigEndian)
        {
            mMemStream = new MemoryStream(buffer);
            InitWithMemoryStream(mMemStream, isBigEndian);
        }

        public DataStream(byte[] buffer, int index, int count, bool isBigEndian)
        {
            mMemStream = new MemoryStream(buffer, index, count);
            InitWithMemoryStream(mMemStream, isBigEndian);
        }

        private void InitWithMemoryStream(MemoryStream ms, bool isBigEndian)
        {
            mBinReader = new BinaryReader(mMemStream);
            mBinWriter = new BinaryWriter(mMemStream);
            mBEMode = isBigEndian;
        }

        public void Close()
        {
            mMemStream.Close();
            mBinReader.Close();
            mBinWriter.Close();
        }

        public void SetBigEndian(bool isBigEndian)
        {
            mBEMode = isBigEndian;
        }

        public bool IsBigEndian()
        {
            return mBEMode;
        }

        public long Position
        {
            get { return mMemStream.Position; }
            set { mMemStream.Position = value; }
        }

        public long Length
        {
            get { return mMemStream.Length; }
        }

        public byte[] ToByteArray()
        {
            //return mMemStream.GetBuffer();
            return mMemStream.ToArray();
        }

        public long Seek(int offset, SeekOrigin loc)
        {
            return mMemStream.Seek(offset, loc);
        }

        public void WriteRaw(byte[] bytes)
        {
            mBinWriter.Write(bytes);
        }

        public void WriteRaw(byte[] bytes, int offset, int count)
        {
            mBinWriter.Write(bytes, offset, count);
        }

        public void WriteByte(byte value)
        {
            mBinWriter.Write(value);
        }

        public byte ReadByte()
        {
            return mBinReader.ReadByte();
        }

        public void WriteString8(string value)
        {
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            byte[] bytes = encoding.GetBytes(value);
            mBinWriter.Write((byte) bytes.Length);
            mBinWriter.Write(bytes);
        }

        public string ReadString8()
        {
            int len = ReadByte();
            byte[] bytes = mBinReader.ReadBytes(len);
            // System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            return encoding.GetString(bytes);
        }

        private void WriteInteger(byte[] bytes)
        {
            if (mBEMode)
                FlipBytes(bytes);
            mBinWriter.Write(bytes);
        }

        private byte[] FlipBytes(byte[] bytes)
        {
            //Array.Reverse(bytes); 
            for (int i = 0, j = bytes.Length - 1; i < j; ++i, --j)
            {
                byte temp = bytes[i];
                bytes[i] = bytes[j];
                bytes[j] = temp;
            }

            return bytes;
        }

        /// <summary>
        /// signedÐÍÊý¾Ý¶ÁÐ´
        /// </summary>
        public void WriteSInt32(Int32 value)
        {
            WriteInteger(BitConverter.GetBytes(value));
        }

        public Int32 ReadSInt32()
        {
            Int32 val = mBinReader.ReadInt32();
            if (mBEMode)
                return BitConverter.ToInt32(FlipBytes(BitConverter.GetBytes(val)), 0);
            return val;
        }
    }
}