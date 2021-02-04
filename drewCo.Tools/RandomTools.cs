//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright ©2009-2019 Andrew A. Ritz, All Rights Reserved
//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

using System;
using System.Text;
using SysMath = System.Math;

// NOTE ON THREAD SAFETY:
// https://msdn.microsoft.com/en-us/library/system.random(v=vs.110).aspx#ThreadSafety

namespace drewCo.Tools
{
    // ============================================================================================================================
    public static class RandomTools
    {
        private static readonly object RndLock = new object();
        public static readonly Random RNG = new Random((int)DateTime.Now.Ticks);


        // --------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Randomize the casing of a string.  This will randomly change uppercase characters to lowercase, and vice-versa.
        /// </summary>
        public static string RandomizeCase(string input)
        {
            int len = input.Length;
            char[] res = new char[len];

            lock (RndLock)
            {
                for (int i = 0; i < len; i++)
                {
                    char next = input[i];
                    if (char.IsLetter(next) & FiftyFifty_NoLock())
                    {
                        int newVal = (int)next;
                        if (newVal >= 97) { newVal -= 32; } else { newVal += 32; }
                        next = (char)newVal;
                    }
                    res[i] = next;
                }
            }

            return new string(res);
        }

        // --------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a random string of the given length that only contains numbers.
        /// </summary>
        public static string GetNumberString(int size)
        {
            StringBuilder builder = new StringBuilder();
            lock (RndLock)
            {

                char ch;
                for (int i = 0; i < size; i++)
                {
                    ch = Convert.ToChar(Convert.ToInt32(SysMath.Floor(10 * RNG.NextDouble() + 48)));
                    builder.Append(ch);
                }
            }
            return builder.ToString();
        }

         // --------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a random alphabet string of the given length.
        /// </summary>
        public static string GetAlphaNumericString(int size)
        {
            StringBuilder builder = new StringBuilder();
            lock (RndLock)
            {
                char ch;
                for (int i = 0; i < size; i++)
                {
                  if (FiftyFifty_NoLock())
                  {
                    // Number
                    ch = Convert.ToChar(Convert.ToInt32(SysMath.Floor(10 * RNG.NextDouble() + 48)));
                  }
                  else
                  {
                    // Alpha
                    ch = Convert.ToChar(Convert.ToInt32(SysMath.Floor(26 * RNG.NextDouble() + 65)));
                  }
                    builder.Append(ch);
                }
            }
            return builder.ToString();
        }

        // --------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a random alphabet string of the given length.
        /// </summary>
        public static string GetAlphaString(int size)
        {
            StringBuilder builder = new StringBuilder();
            lock (RndLock)
            {
                char ch;
                for (int i = 0; i < size; i++)
                {
                    ch = Convert.ToChar(Convert.ToInt32(SysMath.Floor(26 * RNG.NextDouble() + 65)));
                    builder.Append(ch);
                }
            }
            return builder.ToString();
        }

        // --------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Has a fifty-fifty chance of returning true.
        /// </summary>
        public static bool FiftyFifty()
        {
            lock (RndLock)
            {
                return RNG.NextDouble() <= 0.5d;
            }
        }

        // --------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Non locking version for internal functions.
        /// </summary>
        private static bool FiftyFifty_NoLock()
        {
            return RNG.NextDouble() <= 0.5d;
        }


        // --------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a string that is composed of random characters (0-255)
        /// </summary>
        public static string GetRandomCharString(int size)
        {
            StringBuilder builder = new StringBuilder(size);
            lock (RndLock)
            {
                for (int i = 0; i < size; i++)
                {
                    builder.Append((char)(RNG.NextDouble() * 255));
                }
            }
            return builder.ToString();
        }

    }
}
