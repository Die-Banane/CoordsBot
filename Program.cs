using Discord;
using Discord.Rest;
using Discord.WebSocket;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CoordsBot;

class Program
{
    private static DiscordSocketClient _client = null!;
    private static readonly string Token = Environment.GetEnvironmentVariable("DISCORD_TOKEN")!;

    private static Dictionary<string, Coords> _coords = null!;
    
    private static bool _isInitialized;

    // private static async Task InitializeCommands()
    // {
    //     await CreateAddCommand();
    //     await CreateRemoveCommand();
    //     await CreateGetCommand();
    //     await CreateListCommand();

    //     _isInitialized = true;
    // }
    
    static async Task Main()
    {
        if (File.Exists("coords.json"))
        {
            _coords = JsonSerializer.Deserialize<Dictionary<string, Coords>>
                (await File.ReadAllTextAsync("coords.json"))!;
        }
        else
        {
            _coords = new();
        }
        
        _client = new();
        
        await _client.LoginAsync(TokenType.Bot, Token);
        await _client.StartAsync();

        // _client.Ready += async () =>
        // {
        //     //await _client.GetGuild(1442176467671978138).DeleteApplicationCommandsAsync();

        //     if (_isInitialized) return;
        //     await InitializeCommands();
        // };
        
        
        _client.SlashCommandExecuted += async command =>
        {
            string structure = (string)command.Data.Options.First(o => o.Name == "struktur").Value;
            
            switch (command.Data.Name)
            {
                case "add-coords":
                    if (_coords.ContainsKey(structure))
                        _coords.Remove(structure);

                    int x = Convert.ToInt32(command.Data.Options.First(o => o.Name == "x").Value);
                    int y = Convert.ToInt32(command.Data.Options.First(o => o.Name == "y").Value);
                    int z = Convert.ToInt32(command.Data.Options.First(o => o.Name == "z").Value);

                    _coords.Add(structure, new Coords(x, y, z));
                    
                    await File.WriteAllTextAsync("coords.json", JsonSerializer.Serialize(_coords));

                    await command.RespondAsync($"Die Koordinaten von {structure} wurde erfolgreich regristriert");
                    break;
                
                case "remove":
                    if (_coords.ContainsKey((string)command.Data.Options.First().Value))
                    {
                        _coords.Remove(structure);
                        await File.WriteAllTextAsync("coords.json", JsonSerializer.Serialize(_coords));
                        
                        await command.RespondAsync($"Die Koordinaten von {structure} wurden entfernt");
                    }
                    else
                    {
                        await command.RespondAsync($"es gibt keine Koordinaten zu {structure}");
                    }
                    break;
                
                case "get-coords":
                    if (_coords.ContainsKey((string)command.Data.Options.First().Value))
                    {
                        _coords.TryGetValue(structure, out var coords);

                        await command.RespondAsync($"{structure}: X: {coords.X}, Y: {coords.Y}, Z: {coords.Z}");
                    }
                    else
                    {
                        await command.RespondAsync("es wurden keine Koordinaten zu dieser Struktur gefunden");
                    }
                    break;

                case "list-coords":
                    string response = String.Empty;

                    foreach (var coord in _coords)
                    {
                        response += $"{coord.Key}: X: {coord.Value.X}, Y: {coord.Value.Y}, Z: {coord.Value.Z} \n";
                    }
                    
                    await command.RespondAsync(response);
                    break; 
            }
        };

        _client.Log += message =>
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        };
        
        await Task.Delay(-1);
    }

    // private static async Task CreateAddCommand()
    // {
    //     var command = new SlashCommandBuilder()
    //         .WithName("add-coords")
    //         .WithDescription("fügt Koordinaten zu einer Struktur oder einem Ort hinzu")
    //         .AddOption("struktur", ApplicationCommandOptionType.String, "Die Struktur", true)
    //         .AddOption("x", ApplicationCommandOptionType.Integer, "X Koordinate", true)
    //         .AddOption("y", ApplicationCommandOptionType.Integer, "Y Koordinate", true)
    //         .AddOption("z", ApplicationCommandOptionType.Integer, "Z Koordinate", true);
            
    //     await _client.CreateGlobalApplicationCommandAsync(command.Build());
    // }
    
    // private static async Task CreateRemoveCommand()
    // {
    //     var command = new SlashCommandBuilder()
    //         .WithName("remove")
    //         .WithDescription("löscht die Koordinaten zu einer bereits regristrierten Struktur")
    //         .AddOption("struktur", ApplicationCommandOptionType.String, "Die Struktur", true);
            
    //     await _client.CreateGlobalApplicationCommandAsync(command.Build());
    // }

    // private static async Task CreateGetCommand()
    // {
    //     var command = new SlashCommandBuilder()
    //         .WithName("get-coords")
    //         .WithDescription("gibt die Koordinaten zu einer der angegebenen Struktur zurück")
    //         .AddOption("struktur", ApplicationCommandOptionType.String, "Die Struktur", true);
            
    //     await _client.CreateGlobalApplicationCommandAsync(command.Build());
    // }

    // private static async Task CreateListCommand()
    // {
    //     var command = new SlashCommandBuilder()
    //     .WithName("list-coords")
    //     .WithDescription("listet alle regristrierten Strukturen mit ihren dazugehörigen Koordinaten auf");

    //     await _client.CreateGlobalApplicationCommandAsync(command.Build());
    // }

    private record struct Coords(int X, int Y, int Z);
}