namespace SkyNet
{
    public struct NetworkId
    {
        public uint Value;

        internal NetworkId(uint value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            if (obj is NetworkId)
                return Value == ((NetworkId)obj).Value;
            return false;
        }

        public override int GetHashCode()
        {
            return (int)Value;
        }

        public override string ToString()
        {
            return string.Format("[NetworkId:{0}]", Value);
        }
    }
}
