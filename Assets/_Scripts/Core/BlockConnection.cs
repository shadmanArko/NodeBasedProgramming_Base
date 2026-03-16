using System;

namespace _Scripts.Core
{
    [Serializable]
    public class BlockConnection
    {
        public string fromBlockId;
        public string fromPortName;
        public string toBlockId;
        public string toPortName;

        public BlockConnection() { }

        public BlockConnection(string fromBlockId, string fromPortName,
            string toBlockId,   string toPortName)
        {
            this.fromBlockId  = fromBlockId;
            this.fromPortName = fromPortName;
            this.toBlockId    = toBlockId;
            this.toPortName   = toPortName;
        }
    }
}