using System;

namespace TankDecks
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (GameLogic game = new GameLogic())
            {
                game.Run();
            }
        }
    }
#endif
}

