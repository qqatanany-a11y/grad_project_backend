namespace Event.Application.IServices
{
    public interface IPasswordGenerator
    {
        string Generate(int length = 12);
    }
}
