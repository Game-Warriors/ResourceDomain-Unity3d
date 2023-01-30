
namespace GameWarriors.ResourceDomain.Editor
{
    public interface IResourceTabElement
    {
        int CurrentIndex { get; }
        string SearchPattern { get;}   
        int Count { get; }
        bool IsInSearch { get; }

        void ResetDraw();
        void DrawElement(int width, int height);
        void AddNewElement();
        void SaveElement<T>(T input);
        void ApplySearchPatten(string newPattern);
        void ClearSearchPatten();
    }
}
