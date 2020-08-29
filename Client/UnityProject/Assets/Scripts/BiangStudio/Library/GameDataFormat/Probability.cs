namespace BiangStudio.GameDataFormat
{
    public interface Probability
    {
        int Probability { get; set; }
        bool IsSingleton { get; set; }

        Probability ProbabilityClone();
    }
}