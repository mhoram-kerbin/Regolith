namespace Regolith.Common
{
    public interface IAnimatedModule
    {
        void EnableModule();
        void DisableModule();
        bool ModuleIsActive();
    }
}