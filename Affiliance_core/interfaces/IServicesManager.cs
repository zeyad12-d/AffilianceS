namespace Affiliance_core.interfaces
{
    public interface IServicesManager
    {
        IAiService AiService { get; }
        ICampanyServices CampanyServices { get; }
    }
}
