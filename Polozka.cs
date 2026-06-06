namespace InventarniSystem.Models;

// Jedna položka ve skladu. Toto je hlavní "objekt", se kterým aplikace pracuje.
public class Polozka
{
    public string Nazev { get; set; } = "";
    public string Kategorie { get; set; } = "";
    public int Pocet { get; set; }
    public int MinimalniStav { get; set; }

    // Vypočítaná vlastnost: je položky málo? (počet <= minimální stav)
    public bool NizkyStav => Pocet <= MinimalniStav;

    // Text do sloupce "Stav" v tabulce.
    public string Stav => NizkyStav ? "⚠ Nízký stav" : "OK";
}
