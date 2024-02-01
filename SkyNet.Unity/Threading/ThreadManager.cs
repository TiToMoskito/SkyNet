using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace SkyNet
{
    public class ThreadManager : MonoBehaviour, IThreadRunner
    {
        public delegate void UpdateEvent();
        public static UpdateEvent unityUpdate = null;
        public static UpdateEvent unityFixedUpdate = null;

        /// <summary>
        /// The singleton instance of the Main Thread Manager
        /// </summary>
        private static ThreadManager _instance;
        public static ThreadManager Instance
        {
            get
            {
                if (_instance == null)
                    Create();

                return _instance;
            }
        }

        /// <summary>
        /// This will create a main thread manager if one is not already created
        /// </summary>
        public static void Create()
        {
            if (_instance != null)
                return;

            ThreadManagement.Initialize();

            if (!ReferenceEquals(_instance, null))
                return;

            new GameObject("Thread Manager").AddComponent<ThreadManager>();
        }

        /// <summary>
        /// A list of functions to run
        /// </summary>
        private static Queue<Action> mainThreadActions = new Queue<Action>();
        private static Queue<Action> mainThreadActionsRunner = new Queue<Action>();

        // Setup the singleton in the Awake
        private void Awake()
        {
            // If an instance already exists then delete this copy
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            // Assign the static reference to this object
            _instance = this;

            // This object should move through scenes
            DontDestroyOnLoad(gameObject);
        }

        public void Execute(Action action)
        {
            Run(action);
        }

        /// <summary>
        /// Add a function to the list of functions to call on the main thread via the Update function
        /// </summary>
        /// <param name="action">The method that is to be run on the main thread</param>
        public static void Run(Action action)
        {
            // Only create this object on the main thread
            if (ReferenceEquals(Instance, null) && ThreadManagement.IsMainThread)
            {
                Create();
            }

            // Make sure to lock the mutex so that we don't override
            // other threads actions
            lock (mainThreadActions)
            {
                mainThreadActions.Enqueue(action);
            }
        }

        private void HandleActions()
        {
            lock (mainThreadActions)
            {
                // Flush the list to unlock the thread as fast as possible
                if (mainThreadActions.Count > 0)
                {
                    while (mainThreadActions.Count > 0)
                        mainThreadActionsRunner.Enqueue(mainThreadActions.Dequeue());
                }
            }

            // If there are any functions in the list, then run
            // them all and then clear the list
            if (mainThreadActionsRunner.Count > 0)
            {
                while (mainThreadActionsRunner.Count > 0)
                    mainThreadActionsRunner.Dequeue()();
            }
        }

        private void FixedUpdate()
        {
            HandleActions();

            if (unityFixedUpdate != null)
                unityFixedUpdate();
        }

        public static void ThreadSleep(int length)
        {
            System.Threading.Thread.Sleep(length);
        }
    }

    public static class ThreadManagement
    {
        public static int MainThreadId { get; private set; }

        public static int GetCurrentThreadId()
        {
            return Thread.CurrentThread.ManagedThreadId;
        }

        public static void Initialize() { MainThreadId = GetCurrentThreadId(); }

        public static bool IsMainThread
        {
            get { return GetCurrentThreadId() == MainThreadId; }
        }
    }
}
