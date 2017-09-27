namespace Lib.Util
{
    public class Event
    {
        public delegate void Handler();

        public event Handler EventHandler;

        public void Invoke()
        {
            if (EventHandler == null)
            {
                return;
            }
            EventHandler();
        }

        public void AddListener(Handler handler)
        {
            EventHandler += handler;
        }

        public void RemoveListener(Handler handler)
        {
            EventHandler -= handler;
        }

        public bool HasListener(Handler handler)
        {
            foreach (var existingHandler in EventHandler.GetInvocationList())
            {
                if (existingHandler == handler)
                {
                    return true;
                }
            }

            return false;
        }

        public void RemoveAllListeners()
        {
            var list = EventHandler.GetInvocationList();
            foreach (var existingHandler in list)
            {
                EventHandler -= (Handler)existingHandler;
            }
        }
    }

    public class Event<T>
    {
        public delegate void Handler<U>(U value) where U : T;

        public event Handler<T> EventHandler;

        public void Invoke(T value)
        {
            if (EventHandler == null)
            {
                return;
            }
            EventHandler(value);
        }

        public void AddListener(Handler<T> handler)
        {
            EventHandler += handler;
        }

        public void RemoveListener(Handler<T> handler)
        {
            EventHandler -= handler;
        }

        public bool HasListener(Handler<T> handler)
        {
            foreach (var existingHandler in EventHandler.GetInvocationList())
            {
                if (existingHandler == handler)
                {
                    return true;
                }
            }

            return false;
        }

        public void RemoveAllListeners()
        {
            var list = EventHandler.GetInvocationList();
            foreach (var existingHandler in list)
            {
                EventHandler -= (Handler<T>)existingHandler;
            }
        }
    }

    public class Event<T0, T1>
    {
        public delegate void Handler<U0, U1>(U0 value, U1 value2)
            where U0 : T0
            where U1 : T1;

        public event Handler<T0, T1> EventHandler;

        public void Invoke(T0 value, T1 value2)
        {
            if (EventHandler == null)
            {
                return;
            }
            EventHandler(value, value2);
        }

        public void AddListener(Handler<T0, T1> handler)
        {
            EventHandler += handler;
        }

        public void RemoveListener(Handler<T0, T1> handler)
        {
            EventHandler -= handler;
        }

        public bool HasListener(Handler<T0, T1> handler)
        {
            foreach (var existingHandler in EventHandler.GetInvocationList())
            {
                if (existingHandler == handler)
                {
                    return true;
                }
            }

            return false;
        }

        public void RemoveAllListeners()
        {
            var list = EventHandler.GetInvocationList();
            foreach (var existingHandler in list)
            {
                EventHandler -= (Handler<T0, T1>)existingHandler;
            }
        }

    }
}