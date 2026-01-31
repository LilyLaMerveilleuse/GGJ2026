namespace Constants
{
    /// <summary>
    /// Constantes globales du jeu.
    /// </summary>
    public static class GameConstants
    {
        /// <summary>
        /// Noms des scènes du jeu.
        /// </summary>
        public static class Scenes
        {
            public const string MainMenu = "Main Menu";
            public const string Village = "Village";
        }

        /// <summary>
        /// Constantes physiques pour le contrôleur de personnage.
        /// </summary>
        public static class Physics
        {
            public const float GroundCheckBoxHeight = 0.02f;
            public const float VelocityThreshold = 0.1f;
        }
    }
}
