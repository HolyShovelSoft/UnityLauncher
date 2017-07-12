namespace UnityLauncher.Core.Commands.Projects
{
    public class EditorSelectionChangedMessage
    {
        public EditorInfo NewEditor { get; }

        public EditorSelectionChangedMessage(EditorInfo newEditor)
        {
            NewEditor = newEditor;
        }
    }
}