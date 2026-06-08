using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using InventarniSystem.Models;
using InventarniSystem.Services;

namespace InventarniSystem.Views;

public partial class MainWindow : Window
{
    // Aplikační logika skladu (oddělená vrstva).
    private readonly SkladService _sklad = new();

    // Kolekce, kterou zobrazuje tabulka. Když do ní přidám/uberu, tabulka se sama překreslí.
    private readonly ObservableCollection<Polozka> _zobrazene = new();

    // Seznam kategorií pro ComboBox ("Vše" + kategorie z položek).
    private List<string> _kategorieSeznam = new();

    // Příznak, aby se filtry nespouštěly během načítání na startu.
    private bool _nacitam = true;

    public MainWindow()
    {
        InitializeComponent();

        // 1) Načtení dat z AppData (položky + historie).
        foreach (var p in SouborService.NactiPolozky())
            _sklad.Polozky.Add(p);
        foreach (var z in SouborService.NactiHistorii())
            _sklad.Historie.Add(z);

        // Propojení tabulky s kolekcí.
        Tabulka.ItemsSource = _zobrazene;

        NaplnKategorie();

        // 2) Obnovení posledního uloženého filtru.
        var (hledat, kategorie) = SouborService.NactiFiltr();
        HledatBox.Text = hledat;
        KategorieBox.SelectedItem = kategorie;
        if (KategorieBox.SelectedItem == null)
            KategorieBox.SelectedIndex = 0;

        _nacitam = false;
        Obnov();
    }

    // Naplní ComboBox kategorií. Zachová vybranou kategorii, pokud stále existuje.
    private void NaplnKategorie()
    {
        var vybrane = KategorieBox.SelectedItem as string;

        _kategorieSeznam = new List<string> { "Vše" };
        _kategorieSeznam.AddRange(_sklad.Kategorie());

        KategorieBox.ItemsSource = _kategorieSeznam;

        if (vybrane != null && _kategorieSeznam.Contains(vybrane))
            KategorieBox.SelectedItem = vybrane;
        else
            KategorieBox.SelectedIndex = 0;
    }

    // Překreslí tabulku podle aktuálního hledání + filtru a aktualizuje stavový řádek.
    private void Obnov()
    {
        var hledat = HledatBox.Text ?? "";
        var kategorie = KategorieBox.SelectedItem as string ?? "Vše";

        _zobrazene.Clear();
        foreach (var p in _sklad.Najdi(hledat, kategorie))
            _zobrazene.Add(p);

        var nizky = _sklad.NizkyStav().Count;
        StavovyRadek.Text = nizky > 0
            ? $"Položek celkem: {_sklad.Polozky.Count}    |    ⚠ Nízký stav u {nizky} položek!"
            : $"Položek celkem: {_sklad.Polozky.Count}    |    Vše v pořádku.";
    }

    // Uloží vše do AppData (voláme po každé změně).
    private void UlozVse()
    {
        SouborService.UlozPolozky(_sklad.Polozky);
        SouborService.UlozHistorii(_sklad.Historie);
        SouborService.UlozKategorie(_sklad.Kategorie());
        SouborService.UlozFiltr(HledatBox.Text ?? "", KategorieBox.SelectedItem as string ?? "Vše");
    }

    // ---------- Vyhledávání a filtr ----------
    private void Hledat_Zmena(object? sender, TextChangedEventArgs e)
    {
        if (_nacitam) return;
        Obnov();
        SouborService.UlozFiltr(HledatBox.Text ?? "", KategorieBox.SelectedItem as string ?? "Vše");
    }

    private void Kategorie_Zmena(object? sender, SelectionChangedEventArgs e)
    {
        if (_nacitam) return;
        Obnov();
        SouborService.UlozFiltr(HledatBox.Text ?? "", KategorieBox.SelectedItem as string ?? "Vše");
    }

    // ---------- Přidání ----------
    private async void Pridat_Click(object? sender, RoutedEventArgs e)
    {
        var okno = new PolozkaWindow();
        var vysledek = await okno.ShowDialog<Polozka?>(this);

        if (vysledek != null)
        {
            _sklad.Pridej(vysledek);
            NaplnKategorie();
            Obnov();
            UlozVse();
        }
    }

    // ---------- Úprava ----------
    private async void Upravit_Click(object? sender, RoutedEventArgs e)
    {
        if (Tabulka.SelectedItem is not Polozka vybrana)
        {
            await Zprava("Nejdřív vyberte položku, kterou chcete upravit.");
            return;
        }

        var okno = new PolozkaWindow(vybrana);
        var vysledek = await okno.ShowDialog<Polozka?>(this);

        if (vysledek != null)
        {
            _sklad.Uprav(vybrana, vysledek);
            NaplnKategorie();
            Obnov();
            UlozVse();
        }
    }

    // ---------- Smazání ----------
    private async void Smazat_Click(object? sender, RoutedEventArgs e)
    {
        if (Tabulka.SelectedItem is not Polozka vybrana)
        {
            await Zprava("Nejdřív vyberte položku, kterou chcete smazat.");
            return;
        }

        _sklad.Smaz(vybrana);
        NaplnKategorie();
        Obnov();
        UlozVse();
    }

    // ---------- Historie změn (samostatné okno) ----------
    private void Historie_Click(object? sender, RoutedEventArgs e)
    {
        var okno = new HistorieWindow(_sklad.Historie);
        okno.ShowDialog(this);
    }

    // ---------- Export do TXT ----------
    private async void Export_Click(object? sender, RoutedEventArgs e)
    {
        var soubor = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export inventáře do TXT",
            SuggestedFileName = "inventar.txt",
            DefaultExtension = "txt"
        });

        if (soubor != null)
        {
            SouborService.Export(soubor.Path.LocalPath, _sklad.Polozky);
            await Zprava("Inventář byl exportován.");
        }
    }

    // ---------- Import z TXT (bonus: načítání dat z TXT) ----------
    private async void Import_Click(object? sender, RoutedEventArgs e)
    {
        var soubory = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Import inventáře z TXT",
            AllowMultiple = false
        });

        if (soubory.Count > 0)
        {
            var nove = SouborService.NactiPolozkyZ(soubory[0].Path.LocalPath);
            foreach (var p in nove)
                _sklad.Pridej(p);

            NaplnKategorie();
            Obnov();
            UlozVse();
            await Zprava($"Naimportováno {nove.Count} položek.");
        }
    }

    // Jednoduché informační okno (dialog s textem a tlačítkem OK).
    private async Task Zprava(string text)
    {
        var ok = new Button
        {
            Content = "OK",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
        };

        var panel = new StackPanel
        {
            Margin = new Avalonia.Thickness(16),
            Spacing = 12,
            Children =
            {
                new TextBlock { Text = text, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                ok
            }
        };

        var okno = new Window
        {
            Title = "Informace",
            Width = 340,
            Height = 140,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = panel
        };

        ok.Click += (_, _) => okno.Close();
        await okno.ShowDialog(this);
    }
}
