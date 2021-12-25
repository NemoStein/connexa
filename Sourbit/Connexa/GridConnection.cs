using System;

namespace Sourbit.Connexa
{
    [Serializable]
    public struct GridConnection
    {
        public bool OneWay;
        public int Origin;
        public int Target;
        public float Weigth;
    }
}