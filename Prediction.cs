namespace AI
{
    public enum PredictionType
    {
        Fall,
        Rise,
        Still
    }

    public class Prediction
    {
        public double Amount { get; set; }
        public PredictionType Direction { get; set; }
    }
}
