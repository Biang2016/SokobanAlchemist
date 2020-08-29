using System;

namespace BiangStudio.GameDataFormat
{
    public class DataHolder
    {
        public byte[] mRecvDataCache = new byte[16384]; //use array as buffer for efficiency consideration
        public byte[] mRecvData;

        private int mTail = -1;
        private int packLen;

        public void PushData(byte[] data, int length)
        {
            if (mRecvDataCache == null)
                mRecvDataCache = new byte[length];

            if (this.Count + length > this.Capacity) //current capacity is not enough, enlarge the cache
            {
                byte[] newArr = new byte[this.Count + length];
                mRecvDataCache.CopyTo(newArr, 0);
                mRecvDataCache = newArr;
            }

            Array.Copy(data, 0, mRecvDataCache, mTail + 1, length);
            mTail += length;
        }

        public bool IsFinished()
        {
            if (this.Count == 0)
            {
                //skip if no data is currently in the cache
                return false;
            }

            if (this.Count >= 4)
            {
                DataStream reader = new DataStream(mRecvDataCache, true);
                packLen = reader.ReadSInt32();
                if (packLen > 0)
                {
                    if (this.Count - 4 >= packLen)
                    {
                        mRecvData = new byte[packLen];
                        Array.Copy(mRecvDataCache, 4, mRecvData, 0, packLen);
                        return true;
                    }

                    return false;
                }

                return false;
            }

            return false;
        }

        public void Reset()
        {
            mTail = -1;
        }

        public void RemoveFromHead()
        {
            int countToRemove = packLen + 4;
            if (countToRemove > 0 && this.Count - countToRemove > 0)
            {
                Array.Copy(mRecvDataCache, countToRemove, mRecvDataCache, 0, this.Count - countToRemove);
            }

            mTail -= countToRemove;
        }

        public int Capacity
        {
            get { return mRecvDataCache != null ? mRecvDataCache.Length : 0; }
        }

        //indicate how much data is currently in cache in bytes
        public int Count
        {
            get { return mTail + 1; }
        }
    }
}