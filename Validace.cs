namespace InventarniSystem.Services;

// Kontrola vstupů z formuláře. Vrací, jestli jsou data v pořádku,
// a případně chybovou hlášku.
public static class Validace
{
    public static (bool ok, string chyba) Zkontroluj(
        string nazev, string kategorie, string pocetText, string minText)
    {
        if (string.IsNullOrWhiteSpace(nazev))
            return (false, "Zadejte název položky.");

        // Středník používáme jako oddělovač v souboru, proto ho v textu nepovolíme.
        if (nazev.Contains(';') || kategorie.Contains(';'))
            return (false, "Název ani kategorie nesmí obsahovat středník ';'.");

        if (!int.TryParse(pocetText, out var pocet) || pocet < 0)
            return (false, "Počet kusů musí být celé číslo 0 nebo větší.");

        if (!int.TryParse(minText, out var min) || min < 0)
            return (false, "Minimální stav musí být celé číslo 0 nebo větší.");

        return (true, "");
    }
}
