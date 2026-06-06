using System;

namespace InventarniSystem.Models;

// Jeden záznam v historii změn skladu (kdy, jaká akce, jaká položka).
public class ZmenaZaznam
{
    public DateTime Cas { get; set; }
    public string Akce { get; set; } = "";      // např. Přidání, Úprava, Smazání
    public string Polozka { get; set; } = "";
    public string Detail { get; set; } = "";

    // Hezky naformátovaný čas pro zobrazení v tabulce.
    public string CasText => Cas.ToString("dd.MM.yyyy HH:mm:ss");
}
