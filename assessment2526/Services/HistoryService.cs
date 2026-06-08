
using assessment2526.Models;
using System.Collections.ObjectModel;

namespace assessment2526.Services
{
    public static class HistoryService
    {
        public static ObservableCollection<SessionStats> Runs { get; } = new();
    }
}
