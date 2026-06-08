using Avalonia.Controls;
using Avalonia.Interactivity;
using InventarniSystem.Models;
using InventarniSystem.Services;

namespace InventarniSystem.Views;

// Formulář pro přidání NEBO úpravu položky.
// Po uložení vrací vyplněnou položku, po zrušení vrací null.
public partial class PolozkaWindow : Window
{
    // Konstruktor pro PŘIDÁNÍ nové položky.
    public PolozkaWindow()
    {
        InitializeComponent();
    }

    // Konstruktor pro ÚPRAVU - předvyplní pole hodnotami existující položky.
    public PolozkaWindow(Polozka polozka) : this()
    {
        Title = "Úprava položky";
        NazevBox.Text = polozka.Nazev;
        KategorieBox.Text = polozka.Kategorie;
        PocetBox.Text = polozka.Pocet.ToString();
        MinBox.Text = polozka.MinimalniStav.ToString();
    }

    private void Ulozit_Click(object? sender, RoutedEventArgs e)
    {
        var nazev = NazevBox.Text ?? "";
        var kategorie = KategorieBox.Text ?? "";
        var pocet = PocetBox.Text ?? "";
        var min = MinBox.Text ?? "";

        // Kontrola vstupů přes oddělenou validační třídu.
        var (ok, chyba) = Validace.Zkontroluj(nazev, kategorie, pocet, min);
        if (!ok)
        {
            ChybaText.Text = chyba;
            return;
        }

        var vysledek = new Polozka
        {
            Nazev = nazev.Trim(),
            Kategorie = kategorie.Trim(),
            Pocet = int.Parse(pocet),
            MinimalniStav = int.Parse(min)
        };

        Close(vysledek); // vrátí vyplněnou položku hlavnímu oknu
    }

    private void Zrusit_Click(object? sender, RoutedEventArgs e)
    {
        Close(null); // nic nevracíme
    }
}
