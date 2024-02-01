using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkyNet
{
    public class EventDispatcher
    {
        private List<EventListener> m_targets = new List<EventListener>();

        public void Dispatch(Event _evnt)
        {
            IEventFactory eventFactory = Factory.GetEventFactory(_evnt.Data.TypeId);

            for (int i = 0; i < m_targets.Count; i++)
            {
                EventListener target = m_targets[i];

                if (target.Behaviour.enabled && target.GameObject.activeSelf)
                {
                    try
                    {
                        eventFactory.Dispatch(_evnt, target.Behaviour);
                    }
                    catch (Exception ex)
                    {
                        SkyLog.Error("User code threw exception when invoking {0}");
                        SkyLog.Exception(ex);
                    }
                }
            }
        }

        public void Add(MonoBehaviour _behaviour)
        {
            m_targets.Add(new EventListener()
            {
                Behaviour = _behaviour,
                GameObject = _behaviour.gameObject
            });
        }

        public void Remove(MonoBehaviour _behaviour)
        {
            for (int i = 0; i < m_targets.Count; i++)
            {
                if(ReferenceEquals(m_targets[i].Behaviour, _behaviour))
                {
                    m_targets.RemoveAt(i);
                }
            }
        }

        private struct EventListener
        {
            public GameObject GameObject;
            public MonoBehaviour Behaviour;
        }
    }
}
