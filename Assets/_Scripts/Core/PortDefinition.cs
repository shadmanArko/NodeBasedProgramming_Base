using System;

namespace _Scripts.Core
{
    [Serializable]
    public class PortDefinition
    {
        public string name;
        public string dataType;
        public bool isOutput;

        public PortDefinition(string name, string dataType, bool isOutput)
        {
            this.name     = name;
            this.dataType = dataType;
            this.isOutput = isOutput;
        }
    }
}