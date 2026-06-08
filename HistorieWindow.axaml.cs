using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using InventarniSystem.Models;

namespace InventarniSystem.Views;

// Okno zobrazující historii změn skladu.
public partial class HistorieWindow : Window
{
    public HistorieWindow()
    {
        InitializeComponent();
    }

    public HistorieWindow(List<ZmenaZaznam> historie) : this()
    {
        // Nejnovější změny nahoře.
        Tabulka.ItemsSource = historie.OrderByDescending(z => z.Cas).ToList();
    }

    private void Zavrit_Click(object? sender, RoutedEventArgs e) => Close();
}
