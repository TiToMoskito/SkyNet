using System.Threading;

namespace SkyNet
{
    public static class NetStats
    {
        public static int numPacketsOut { get; private set; }
        public static int numBufferedPacketsOut { get; private set; }
        public static int numBytesOut { get; private set; }

        public static int numPacketsIn { get; private set; }
        public static int numBytesIn { get; private set; }

        public static int numBufferedPerSecond { get; private set; }
        public static int lastBufferedPerSecond { get; private set; }

        private static int numPacketsOutTmp;
        private static int numBufferedPacketsOutTmp;
        private static int numBytesOutTmp;
        private static int numPacketsInTmp;
        private static int numBytesInTmp;
        private static int numBufferedPerSecondTmp;
        private static int lastBufferedPerSecondTmp;

        private static Thread thread;

        internal static void ReceivePacket(int _size)
        {
            numPacketsInTmp++;
            numBytesInTmp += _size;

            CheckThread();
        }

        internal static void SentPacket(int _size)
        {
            numPacketsOutTmp++;
            numBytesOutTmp += _size;

            CheckThread();
        }

        private static void CheckThread()
        {
            if(thread == null)
            {
                thread = new Thread(ClearStats);
                thread.Start();                
            }
        }

        private static void ClearStats()
        {
            while (true)
            {
                numPacketsOut = numPacketsOutTmp;
                numBytesOut = numBytesOutTmp;
                numBufferedPacketsOut = numBufferedPacketsOutTmp;
                numPacketsIn = numPacketsInTmp;
                numBytesIn = numBytesInTmp;
                numBufferedPerSecond = numBufferedPerSecondTmp;
                lastBufferedPerSecond = lastBufferedPerSecondTmp;

                numPacketsOutTmp = 0;
                numBytesOutTmp = 0;
                numBufferedPacketsOutTmp = 0;
                numPacketsInTmp = 0;
                numBytesInTmp = 0;
                numBufferedPerSecondTmp = 0;
                lastBufferedPerSecondTmp = 0;

                Thread.Sleep(1000);
            }
        }
    }
}
