using FlatFileRepo;

class Program
{
    static void Main()
    {
        var filePath = @"C:\It-Sikkerhed\Softwaresikkerhed\it_sikkerhed_2026f\Softwaresikkerhed\10-2-26\FlatFileRepo\FlatFileRepo\Data\people.json";
        var repo = new PersonRepository(filePath);

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("1) Vis alle");
            Console.WriteLine("2) Tilføj");
            Console.WriteLine("3) Slet");
            Console.WriteLine("4) Opdater");
            Console.WriteLine("0) Exit");
            Console.Write("Vælg: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ShowAll(repo);
                    break;

                case "2":
                    AddPerson(repo);
                    break;

                case "3":
                    RemovePerson(repo);
                    break;

                case "4":
                    UpdatePerson(repo);
                    break;

                case "0":
                    return;

                default:
                    Console.WriteLine("Ugyldigt valg.");
                    break;
            }
        }
    }

    static void ShowAll(PersonRepository repo)
    {
        var people = repo.GetAll().ToList();
        if (people.Count == 0)
        {
            Console.WriteLine("Databasen er tom.");
            return;
        }

        Console.WriteLine();
        foreach (var p in people)
        {
            Console.WriteLine($"{p.Person_Id}: {p.First_Name} {p.Last_Name}, {p.Address} {p.Street_Number}, Enabled={p.Enabled}");
        }
    }

    static void AddPerson(PersonRepository repo)
    {
        try
        {
            var first = ReadRequiredString("First name: ");
            var last = ReadRequiredString("Last name: ");
            var address = ReadRequiredString("Address: ");
            var streetNumber = ReadInt("Street number: ");
            var password = ReadRequiredString("Password: ");
            var enabled = ReadBool("Enabled (true/false): ");

            var person = new PersonModel(
                person_Id: 0,
                first_Name: first,
                last_Name: last,
                address: address,
                street_Number: streetNumber,
                password: password,
                enabled: enabled
            );

            var added = repo.Add(person);
            Console.WriteLine($"Tilføjet med id={added.Person_Id}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Valideringsfejl: {ex.Message}");
        }
    }

    static void RemovePerson(PersonRepository repo)
    {
        var id = ReadInt("Id der skal slettes: ");
        var removed = repo.Remove(id);

        if (removed == null)
            Console.WriteLine("Ingen person med det id.");
        else
            Console.WriteLine($"Slettet: {removed.Person_Id} ({removed.First_Name} {removed.Last_Name})");
    }

    static void UpdatePerson(PersonRepository repo)
    {
        var id = ReadInt("Id der skal opdateres: ");

        var existing = repo.GetById(id);
        if (existing == null)
        {
            Console.WriteLine("Ingen person med det id.");
            return;
        }

        Console.WriteLine($"Opdaterer: {existing.Person_Id} ({existing.First_Name} {existing.Last_Name})");
        Console.WriteLine("Tryk Enter for at beholde nuværende værdi.");

        try
        {
            var first = ReadOptionalString($"First name ({existing.First_Name}): ") ?? existing.First_Name;
            var last = ReadOptionalString($"Last name ({existing.Last_Name}): ") ?? existing.Last_Name;
            var address = ReadOptionalString($"Address ({existing.Address}): ") ?? existing.Address;
            var streetNumber = ReadOptionalInt($"Street number ({existing.Street_Number}): ") ?? existing.Street_Number;
            var password = ReadOptionalString($"Password ({Mask(existing.Password)}): ") ?? existing.Password;
            var enabled = ReadOptionalBool($"Enabled ({existing.Enabled}): ") ?? existing.Enabled;

            var updated = new PersonModel(
                person_Id: id,
                first_Name: first,
                last_Name: last,
                address: address,
                street_Number: streetNumber,
                password: password,
                enabled: enabled
            );

            var result = repo.Update(id, updated);
            Console.WriteLine(result == null ? "Opdatering fejlede." : "Opdateret.");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Valideringsfejl: {ex.Message}");
        }
    }

    static string ReadRequiredString(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var s = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(s)) return s.Trim();
            Console.WriteLine("Må ikke være tom.");
        }
    }

    static string? ReadOptionalString(string prompt)
    {
        Console.Write(prompt);
        var s = Console.ReadLine();
        return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }

    static int ReadInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out var value)) return value;
            Console.WriteLine("Indtast et heltal.");
        }
    }

    static int? ReadOptionalInt(string prompt)
    {
        Console.Write(prompt);
        var s = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(s)) return null;
        return int.TryParse(s, out var v) ? v : null;
    }

    static bool ReadBool(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (bool.TryParse(Console.ReadLine(), out var value)) return value;
            Console.WriteLine("Indtast true eller false.");
        }
    }

    static bool? ReadOptionalBool(string prompt)
    {
        Console.Write(prompt);
        var s = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(s)) return null;
        return bool.TryParse(s, out var v) ? v : null;
    }

    static string Mask(string s) => new string('*', Math.Min(s.Length, 8));
}

