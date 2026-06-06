using System;
using System.Collections.Generic;
using System.Linq;
using InventarniSystem.Models;

namespace InventarniSystem.Services;

// Aplikační logika skladu - je úplně oddělená od GUI.
// Drží seznam položek + historii změn a umí s nimi pracovat.
public class SkladService
{
    public List<Polozka> Polozky { get; } = new();
    public List<ZmenaZaznam> Historie { get; } = new();

    // Přidá novou položku a zapíše to do historie.
    public void Pridej(Polozka p)
    {
        Polozky.Add(p);
        Zaznamenej("Přidání", p.Nazev, $"počet: {p.Pocet}, kategorie: {p.Kategorie}");
    }

    // Upraví existující položku (přepíše hodnoty) a zapíše to do historie.
    public void Uprav(Polozka puvodni, Polozka nova)
    {
        puvodni.Nazev = nova.Nazev;
        puvodni.Kategorie = nova.Kategorie;
        puvodni.Pocet = nova.Pocet;
        puvodni.MinimalniStav = nova.MinimalniStav;
        Zaznamenej("Úprava", nova.Nazev, $"počet: {nova.Pocet}, kategorie: {nova.Kategorie}");
    }

    // Smaže položku a zapíše to do historie.
    public void Smaz(Polozka p)
    {
        Polozky.Remove(p);
        Zaznamenej("Smazání", p.Nazev, "");
    }

    // Vyhledávání: filtruje podle textu v názvu a podle kategorie.
    public List<Polozka> Najdi(string hledat, string kategorie)
    {
        IEnumerable<Polozka> vysledek = Polozky;

        if (!string.IsNullOrWhiteSpace(hledat))
            vysledek = vysledek.Where(p =>
                p.Nazev.Contains(hledat, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(kategorie) && kategorie != "Vše")
            vysledek = vysledek.Where(p => p.Kategorie == kategorie);

        return vysledek.ToList();
    }

    // Vrátí položky, kterých je málo (pro upozornění).
    public List<Polozka> NizkyStav() => Polozky.Where(p => p.NizkyStav).ToList();

    // Vrátí seznam unikátních kategorií (pro filtr).
    public List<string> Kategorie() =>
        Polozky.Select(p => p.Kategorie)
               .Where(k => !string.IsNullOrWhiteSpace(k))
               .Distinct()
               .OrderBy(k => k)
               .ToList();

    // Pomocná metoda - přidá záznam do historie.
    private void Zaznamenej(string akce, string polozka, string detail)
    {
        Historie.Add(new ZmenaZaznam
        {
            Cas = DateTime.Now,
            Akce = akce,
            Polozka = polozka,
            Detail = detail
        });
    }
}
