using System;

namespace SkyNet
{
    public struct UniqueId
    {
        internal Guid m_guid;

        public string IdString
        {
            get
            {
                if (IsNone)
                    return "NONE";
                return m_guid.ToString();
            }
        }

        public bool IsNone
        {
            get
            {
                return m_guid == Guid.Empty;
            }
        }

        public UniqueId(string guid)
        {
            this = new UniqueId();
            m_guid = new Guid(guid);
        }

        public UniqueId(byte[] guid)
        {
            this = new UniqueId();
            m_guid = new Guid(guid);
        }

        public byte[] ToByteArray()
        {
            return m_guid.ToByteArray();
        }

        public override int GetHashCode()
        {
            return m_guid.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is UniqueId)
                return ((UniqueId)obj).m_guid == m_guid;
            return false;
        }

        public override string ToString()
        {
            if (IsNone)
                return "[UniqueId NONE]";
            return string.Format("[UniqueId {0}]", m_guid.ToString());
        }

        public static UniqueId None
        {
            get
            {
                return new UniqueId();
            }
        }

        public static UniqueId New()
        {
            return new UniqueId() { m_guid = Guid.NewGuid() };
        }

        public static UniqueId Parse(string text)
        {
            if (text == null || text == "" || text == "NONE")
                return None;
            try
            {
                return new UniqueId() { m_guid = new Guid(text) };
            }
            catch
            {
                SkyLog.Warn("Could not parse '"+ text+"' as a UniqueId");
                return None;
            }
        }

        public static bool operator ==(UniqueId a, UniqueId b)
        {
            return a.m_guid == b.m_guid;
        }

        public static bool operator !=(UniqueId a, UniqueId b)
        {
            return a.m_guid != b.m_guid;
        }
    }
}
