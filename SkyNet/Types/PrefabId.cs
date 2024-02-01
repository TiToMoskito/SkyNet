namespace SkyNet
{
    public struct PrefabId
    {
        public int Value;

        public PrefabId(int value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            if (obj is PrefabId)
                return Value == ((PrefabId)obj).Value;
            return false;
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return string.Format("[PrefabId:{0}]", (object)Value);
        }

        public static PrefabId Parse(int val)
        {
            try
            {
                return new PrefabId() { Value = val };
            }
            catch
            {
                SkyLog.Warn("Could not parse '" + val + "' as a PrefabId");
                return new PrefabId();
            }
        }
    }
}
