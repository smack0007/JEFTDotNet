namespace JEFTDotNet
{
    internal static class MathHelper
    {
        // Implementation of algorithm found at "http://graphics.stanford.edu/~seander/bithacks.html#RoundUpPowerOf2".</remarks>
        public static uint RoundNextPowerOf2(uint input)
        {
            if (input == 0)
                return 0;

            input--;
            input |= input >> 1;
            input |= input >> 2;
            input |= input >> 4;
            input |= input >> 8;
            input |= input >> 16;
            input++;

            return input;
        }

        public static uint RoundClosestPowerOf2(uint input)
        {
            uint next = RoundNextPowerOf2(input);
            uint prev = next >> 1;
            return next - input < input - prev ? next : prev;
        }
    }
}
