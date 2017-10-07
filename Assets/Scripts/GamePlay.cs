using System;

namespace Solitaire.Game
{
    public static class GamePlay
    {
        private enum State
        {
            NotStarted,
            Running,
            Paused,
            Ended
        }
        private static State state;

        public static bool Running
        {
            get
            {
                return state == State.Running;
            }
        }

        private static DateTime startTime;
        private static TimeSpan timePassed = TimeSpan.Zero;
        public static TimeSpan TimePassed
        {
            get
            {
                if (Running)
                    timePassed = DateTime.Now - startTime;

                return timePassed;
            }
        }
        
        public static void Reset()
        {
            timePassed = TimeSpan.Zero;
            state = State.NotStarted;
        }

        public static void Start()
        {
            if (state != State.NotStarted)
                throw new InvalidOperationException();

            startTime = DateTime.Now;
            state = State.Running;
        }

        public static void PauseOrResume()
        {
            if (state == State.Paused)
                Resume();
            else if (state == State.Running)
                Pause();
            else
                throw new InvalidOperationException();
        }
        private static void Pause()
        {
            if (state != State.Running)
                throw new InvalidOperationException();

            var _ = TimePassed;  // trigger storage
            state = State.Paused;
        }
        private static void Resume()
        {
            if (state != State.Paused)
                throw new InvalidOperationException();

            startTime = DateTime.Now - timePassed;
            state = State.Running;
        }

        public static void Stop()
        {
            if (state == State.NotStarted || state == State.Ended)
                throw new InvalidOperationException();

            var _ = TimePassed;  // trigger storage
            state = State.Ended;
        }
    }
}
