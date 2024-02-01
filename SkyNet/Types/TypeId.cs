namespace SkyNet
{
    public struct TypeId
    {
        public uint Value;

        internal TypeId(uint value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            if (obj is TypeId)
                return Value == ((TypeId)obj).Value;
            return false;
        }

        public override int GetHashCode()
        {
            return (int)Value;
        }

        public override string ToString()
        {
            return string.Format("[TypeId:{0}]", Value);
        }

        public static bool operator ==(TypeId a, TypeId b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(TypeId a, TypeId b)
        {
            return a.Value != b.Value;
        }
    }
}
