using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using UnityEngine;
using Random = System.Random;

namespace Assets.Scripts
{
    public static class Utility
    {
        private static readonly Random mRandom = new Random(DateTime.Now.Millisecond);

        public static int GetRandomInt(int aMinValue, int aMaxValue)
        {
            return mRandom.Next(aMinValue, aMaxValue);
        }

        public static float GetRandomFloat(float aMinValue, float aMaxValue)
        {
            return (float)(aMinValue + mRandom.NextDouble()*(aMaxValue - aMinValue));
        }

        public static void PauseGame(bool pauseGame)
        {
            Time.timeScale = pauseGame ? 0.0f : 1.0f;
        }
    }
}
