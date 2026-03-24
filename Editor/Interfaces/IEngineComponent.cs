namespace Editor.Interfaces
{
    public interface IEngineComponent
    {
        string ComponentName { get; }
        void Initialize();
    }
}