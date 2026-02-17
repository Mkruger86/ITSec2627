using FlatFileRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FlatFileRepo
{
    public class PersonRepository
    {
        private readonly string _filePath;
        private readonly List<PersonModel> _people;
        private int _nextId;

        public PersonRepository(string filePath)
        {
            _filePath = filePath;

            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            _people = LoadFromFile();

            _nextId = (_people.Count == 0)
                ? 1
                : _people.Max(p => p.Person_Id) + 1;
        }

        public IEnumerable<PersonModel> GetAll() => _people.ToList();

        public PersonModel? GetById(int id) => _people.FirstOrDefault(p => p.Person_Id == id);

        public PersonModel Add(PersonModel person)
        {
            person.ValidatePerson();

            person.Person_Id = _nextId++;
            _people.Add(person);
            SaveToFile();
            return person;
        }

        public PersonModel? Remove(int id)
        {
            var existing = GetById(id);
            if (existing == null) return null;

            _people.Remove(existing);
            SaveToFile();
            return existing;
        }

        public PersonModel? Update(int id, PersonModel updated)
        {
            updated.ValidatePerson();

            var existing = GetById(id);
            if (existing == null) return null;

            existing.First_Name = updated.First_Name;
            existing.Last_Name = updated.Last_Name;
            existing.Address = updated.Address;
            existing.Street_Number = updated.Street_Number;
            existing.Password = updated.Password;
            existing.Enabled = updated.Enabled;

            SaveToFile();
            return existing;
        }

        private List<PersonModel> LoadFromFile()
        {
            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "[]");
                return new List<PersonModel>();
            }

            var json = File.ReadAllText(_filePath);
            if (string.IsNullOrWhiteSpace(json)) return new List<PersonModel>();

            return JsonSerializer.Deserialize<List<PersonModel>>(json) ?? new List<PersonModel>();
        }

        private void SaveToFile()
        {
            var json = JsonSerializer.Serialize(_people, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_filePath, json);
        }
    }
}
