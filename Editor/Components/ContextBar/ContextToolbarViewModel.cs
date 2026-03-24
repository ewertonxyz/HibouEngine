using Editor.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;

namespace Editor.Components.ContextBar
{
    public class ContextToolbarViewModel : BaseViewModel
    {
        private string _selectedMode;
        private Visibility _editModeVisibility;
        private Visibility _levelEditorVisibility;

        public ObservableCollection<string> AvailableModes { get; set; }

        public string SelectedMode
        {
            get => _selectedMode;
            set
            {
                _selectedMode = value;
                OnPropertyChanged();
                UpdateContextVisibility();
            }
        }

        public Visibility EditModeVisibility
        {
            get => _editModeVisibility;
            set
            {
                _editModeVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility LevelEditorVisibility
        {
            get => _levelEditorVisibility;
            set
            {
                _levelEditorVisibility = value;
                OnPropertyChanged();
            }
        }

        public ContextToolbarViewModel()
        {
            AvailableModes = new ObservableCollection<string>
            {
                "Edit Mode",
                "Level Editor"
            };
            SelectedMode = "Level Editor";
        }

        private void UpdateContextVisibility()
        {
            if (SelectedMode == "Edit Mode")
            {
                EditModeVisibility = Visibility.Visible;
                LevelEditorVisibility = Visibility.Collapsed;
            }
            else
            {
                EditModeVisibility = Visibility.Collapsed;
                LevelEditorVisibility = Visibility.Visible;
            }
        }
    }
}