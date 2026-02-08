namespace CheeseUtilMod.Shared.CustomData;

public static class Pegs
{
    public static class DualPort
    {
        /// <summary>
        /// Load via command
        /// </summary>
        public const int LOAD = 0;

        /// <summary>
        /// Port A output enable
        /// </summary>
        public const int A_OE = 1;

        /// <summary>
        /// Port A write
        /// </summary>
        public const int A_W = 2;

        /// <summary>
        /// Port B output enable
        /// </summary>
        public const int B_OE = 3;

        /// <summary>
        /// Port B write
        /// </summary>
        public const int B_W = 4;

        /// <summary>
        /// How many control pegs are there
        /// </summary>
        public const int ControlPegs = 5;

        /// <summary>
        /// How many address bits are there by default
        /// </summary>
        public const int DefaultAddressWidth = 4;

        /// <summary>
        /// How many data bits are there by default
        /// </summary>
        public const int DefaultBitWidth = 2;
    }
}