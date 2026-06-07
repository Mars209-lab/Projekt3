using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using InventarniSystem.Models;

namespace InventarniSystem.Services;

// Stará se o veškerou práci se soubory.
// Data se automaticky ukládají do složky AppData (data zůstanou i po vypnutí PC).
public static class SouborService
{
    // Cesta do AppData, např.: C:\Users\<jmeno>\AppData\Roaming\InventarniSystem
    private static readonly string Slozka = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "InventarniSystem");

    // Veřejná cesta - hodí se zobrazit uživateli, kde data leží.
    public static string SlozkaAppData => Slozka;

    private static string Cesta(string soubor) => Path.Combine(Slozka, soubor);

    private static void ZajistiSlozku()
    {
        if (!Directory.Exists(Slozka))
            Directory.CreateDirectory(Slozka);
    }

    // ---------- POLOŽKY ----------
    // Ukládáme do TXT, jeden řádek = jedna položka, hodnoty oddělené středníkem.
    public static void UlozPolozky(IEnumerable<Polozka> polozky)
    {
        ZajistiSlozku();
        var radky = polozky.Select(p =>
            $"{p.Nazev};{p.Kategorie};{p.Pocet};{p.MinimalniStav}");
        File.WriteAllLines(Cesta("polozky.txt"), radky, Encoding.UTF8);
    }

    // Načte položky z hlavního souboru v AppData.
    public static List<Polozka> NactiPolozky() => NactiPolozkyZ(Cesta("polozky.txt"));

    // Načte položky z libovolného TXT (používá se i pro Import).
    public static List<Polozka> NactiPolozkyZ(string cesta)
    {
        var seznam = new List<Polozka>();
        if (!File.Exists(cesta)) return seznam;

        foreach (var radek in File.ReadAllLines(cesta, Encoding.UTF8))
        {
            if (string.IsNullOrWhiteSpace(radek)) continue;

            var casti = radek.Split(';');
            if (casti.Length < 4) continue; // poškozený řádek přeskočíme

            seznam.Add(new Polozka
            {
                Nazev = casti[0],
                Kategorie = casti[1],
                Pocet = int.TryParse(casti[2], out var po) ? po : 0,
                MinimalniStav = int.TryParse(casti[3], out var min) ? min : 0
            });
        }
        return seznam;
    }

    // ---------- HISTORIE ZMĚN ----------
    public static void UlozHistorii(IEnumerable<ZmenaZaznam> historie)
    {
        ZajistiSlozku();
        var radky = historie.Select(z =>
            $"{z.Cas.ToString("o", CultureInfo.InvariantCulture)};{z.Akce};{z.Polozka};{z.Detail}");
        File.WriteAllLines(Cesta("historie.txt"), radky, Encoding.UTF8);
    }

    public static List<ZmenaZaznam> NactiHistorii()
    {
        var seznam = new List<ZmenaZaznam>();
        var cesta = Cesta("historie.txt");
        if (!File.Exists(cesta)) return seznam;

        foreach (var radek in File.ReadAllLines(cesta, Encoding.UTF8))
        {
            if (string.IsNullOrWhiteSpace(radek)) continue;

            var c = radek.Split(';');
            if (c.Length < 4) continue;

            seznam.Add(new ZmenaZaznam
            {
                Cas = DateTime.TryParse(c[0], CultureInfo.InvariantCulture,
                        DateTimeStyles.RoundtripKind, out var dt) ? dt : DateTime.Now,
                Akce = c[1],
                Polozka = c[2],
                Detail = c[3]
            });
        }
        return seznam;
    }

    // ---------- ULOŽENÉ FILTRY ----------
    // Uloží poslední hledaný text a vybranou kategorii (řádek 1 = text, řádek 2 = kategorie).
    public static void UlozFiltr(string hledat, string kategorie)
    {
        ZajistiSlozku();
        File.WriteAllLines(Cesta("nastaveni.txt"),
            new[] { hledat ?? "", kategorie ?? "" }, Encoding.UTF8);
    }

    public static (string hledat, string kategorie) NactiFiltr()
    {
        var cesta = Cesta("nastaveni.txt");
        if (!File.Exists(cesta)) return ("", "Vše");

        var r = File.ReadAllLines(cesta, Encoding.UTF8);
        var hledat = r.Length > 0 ? r[0] : "";
        var kat = r.Length > 1 && !string.IsNullOrWhiteSpace(r[1]) ? r[1] : "Vše";
        return (hledat, kat);
    }

    // ---------- SEZNAM KATEGORIÍ ----------
    public static void UlozKategorie(IEnumerable<string> kategorie)
    {
        ZajistiSlozku();
        File.WriteAllLines(Cesta("kategorie.txt"), kategorie, Encoding.UTF8);
    }

    public static List<string> NactiKategorie()
    {
        var cesta = Cesta("kategorie.txt");
        if (!File.Exists(cesta)) return new List<string>();

        return File.ReadAllLines(cesta, Encoding.UTF8)
                   .Where(k => !string.IsNullOrWhiteSpace(k))
                   .ToList();
    }

    // ---------- EXPORT do čitelného TXT ----------
    // Vytvoří přehledný textový soupis inventáře (pro tisk / odevzdání).
    public static void Export(string cesta, IEnumerable<Polozka> polozky)
    {
        var sb = new StringBuilder();
        sb.AppendLine("INVENTÁRNÍ SOUPIS");
        sb.AppendLine("Vytvořeno: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
        sb.AppendLine(new string('-', 50));

        foreach (var p in polozky)
        {
            var upozorneni = p.NizkyStav ? "   <-- NÍZKÝ STAV" : "";
            sb.AppendLine($"{p.Nazev} | kategorie: {p.Kategorie} | počet: {p.Pocet} | min: {p.MinimalniStav}{upozorneni}");
        }

        File.WriteAllText(cesta, sb.ToString(), Encoding.UTF8);
    }
}
