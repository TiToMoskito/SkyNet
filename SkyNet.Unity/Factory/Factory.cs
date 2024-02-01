using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SkyNet.Unity")]
namespace SkyNet
{
    internal static class Factory
    {
        private static Dictionary<Type, IFactory> m_factoriesByType = new Dictionary<Type, IFactory>();
        private static Dictionary<TypeId, IFactory> m_factoriesById = new Dictionary<TypeId, IFactory>();
        private static Dictionary<UniqueId, IFactory> m_factoriesByKey = new Dictionary<UniqueId, IFactory>();

        internal static void Register(IFactory factory)
        {
            if (!m_factoriesById.ContainsKey(factory.TypeID))
                m_factoriesById.Add(factory.TypeID, factory);

            if (!m_factoriesByKey.ContainsKey(factory.TypeKey))
                m_factoriesByKey.Add(factory.TypeKey, factory);

            if (!m_factoriesByType.ContainsKey(factory.TypeObject))
                m_factoriesByType.Add(factory.TypeObject, factory);
        }

        internal static IFactory GetFactory(TypeId _id)
        {
            if (m_factoriesById.ContainsKey(_id))
                return m_factoriesById[_id];
            SkyLog.Error("Unknown factory "+ _id);
            return null;
        }

        internal static IFactory GetFactory(UniqueId _id)
        {
            if (m_factoriesByKey.ContainsKey(_id))
                return m_factoriesByKey[_id];
            SkyLog.Error("Unknown factory "+ _id);
            return null;
        }

        internal static IEventFactory GetEventFactory(TypeId _id)
        {
            return (IEventFactory)m_factoriesById[_id];
        }

        internal static IEventFactory GetEventFactory(UniqueId _id)
        {
            return (IEventFactory)m_factoriesByKey[_id];
        }

        internal static Event NewEvent(TypeId _id)
        {
            Event @event = (Event)Create(_id);
            return @event;
        }

        internal static Event NewEvent(UniqueId _id)
        {
            Event @event = (Event)Create(_id);
            return @event;
        }

        internal static IEntitySerializer NewSerializer(TypeId id)
        {
            return (IEntitySerializer)Create(id);
        }

        internal static IEntitySerializer NewSerializer(UniqueId guid)
        {
            return (IEntitySerializer)Create(guid);
        }

        private static object Create(TypeId _id)
        {
            if (!m_factoriesById.ContainsKey(_id))
                SkyLog.Error("Unknown "+ _id);
            return m_factoriesById[_id].Create();
        }

        private static object Create(UniqueId _id)
        {
            if (!m_factoriesByKey.ContainsKey(_id))
                SkyLog.Error("Unknown "+ _id);
            return m_factoriesByKey[_id].Create();
        }
    }
}
