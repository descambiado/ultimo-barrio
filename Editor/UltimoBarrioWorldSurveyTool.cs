using Sandbox;
using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace UltimoBarrio.EditorTools
{
    public static class UltimoBarrioWorldSurveyTool
    {
        public static void RunSurvey()
        {
            var surveyData = new
            {
                MapName = "thieves.rpdowntown3t",
                SurveyTimestamp = DateTime.UtcNow.ToString("O"),
                Sectors = new List<object>
                {
                    new { SectorId = "Sector_Residential_North", Use = "Zona Residencial (Apartamentos A01 - A03)", Spawnpoints = 2, PropsDensity = "Alta", ProposedFPS = 60 },
                    new { SectorId = "Sector_Residential_South", Use = "Zona Residencial (Apartamentos A04 - A06)", Spawnpoints = 2, PropsDensity = "Alta", ProposedFPS = 60 },
                    new { SectorId = "Sector_Plaza", Use = "Plaza Central / Kiosco Comerciante", Spawnpoints = 1, PropsDensity = "Media", ProposedFPS = 60 },
                    new { SectorId = "Sector_Scrapyard", Use = "Chatarrería / Nodos de Recolección", Spawnpoints = 1, PropsDensity = "Alta", ProposedFPS = 60 },
                    new { SectorId = "Sector_Workshop", Use = "Taller de Mantenimiento", Spawnpoints = 1, PropsDensity = "Media", ProposedFPS = 60 },
                    new { SectorId = "Sector_Alley", Use = "Callejón de Contrabando", Spawnpoints = 1, PropsDensity = "Alta", ProposedFPS = 60 },
                    new { SectorId = "Sector_Raid_Entry_A", Use = "Punto de Incursión Norte", Spawnpoints = 2, PropsDensity = "Baja", ProposedFPS = 60 },
                    new { SectorId = "Sector_Raid_Entry_B", Use = "Punto de Incursión Sur", Spawnpoints = 2, PropsDensity = "Baja", ProposedFPS = 60 }
                }
            };

            string json = JsonSerializer.Serialize(surveyData, new JsonSerializerOptions { WriteIndented = true });
            
            Directory.CreateDirectory("Assets/data");
            File.WriteAllText("Assets/data/world_survey.json", json);

            string markdown = $@"# World Survey — RP Downtown 3t (`thieves.rpdowntown3t`)

Fecha: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC

## Sectores del Mapa Mapeados

| SectorId | Uso Propuesto | Spawnpoints | Densidad Props | FPS Objetivo |
|---|---|---|---|---|
| `Sector_Residential_North` | Viviendas A01 - A03 | 2 | Alta | 60 |
| `Sector_Residential_South` | Viviendas A04 - A06 | 2 | Alta | 60 |
| `Sector_Plaza` | Kiosco Comerciante | 1 | Media | 60 |
| `Sector_Scrapyard` | Chatarrería (20 pickups) | 1 | Alta | 60 |
| `Sector_Workshop` | Taller de Almacén | 1 | Media | 60 |
| `Sector_Alley` | Callejón de Contrabando | 1 | Alta | 60 |
| `Sector_Raid_Entry_A` | Incursión Saqueadores Norte | 2 | Baja | 60 |
| `Sector_Raid_Entry_B` | Incursión Saqueadores Sur | 2 | Baja | 60 |
";
            Directory.CreateDirectory("docs/production");
            File.WriteAllText("docs/production/WORLD_SURVEY.md", markdown);

            Log.Info("[WorldSurveyTool] Survey completed: Assets/data/world_survey.json & docs/production/WORLD_SURVEY.md");
        }
    }
}
